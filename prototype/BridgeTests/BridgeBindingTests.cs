using BallKnowledge.MatchEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace BallKnowledge.BridgeTests;

/// <summary>
/// These tests exercise the NETSTANDARD2.1 engine assembly (see the csproj — the reference is
/// pinned so this is the same binary Unity loads) with Newtonsoft.Json, the JSON library Unity
/// uses. They exist because the dependency-free DLL's [JsonPropertyName] shims are invisible
/// to every JSON reader unless the loader opts in (EngineJsonContractResolver), and the
/// failure mode is SILENT zeros that the Phase 1 proof cannot catch (it runs on net8.0 with
/// the real BCL attribute). If any test here fails, the Unity bridge is broken regardless of
/// what `dotnet test` on BallKnowledge.Tests or `validate` says.
/// </summary>
public class BridgeBindingTests
{
    private static readonly JsonSerializerSettings UnityLoaderSettings = new()
    {
        ContractResolver = new EngineJsonContractResolver(),
        Converters = { new StringEnumConverter() },
    };

    [Fact]
    public void EngineAssemblyIsTheNetstandardBuild()
    {
        // If this fails, the csproj's SetTargetFramework pin broke and every other test
        // here is validating the wrong (net8.0) binary.
        var framework = typeof(EngineConfig).Assembly
            .GetCustomAttributesData()
            .Where(a => a.AttributeType.Name == "TargetFrameworkAttribute")
            .Select(a => a.ConstructorArguments[0].Value as string)
            .FirstOrDefault();

        Assert.Equal(".NETStandard,Version=v2.1", framework);
    }

    [Fact]
    public void ConstantsJsonBindsCompletely_EveryKeyRoundTrips()
    {
        var rawJson = File.ReadAllText(DesignPath("constants.json"));
        var expected = JObject.Parse(rawJson);

        var config = JsonConvert.DeserializeObject<EngineConfig>(rawJson, UnityLoaderSettings);
        Assert.NotNull(config);

        // Re-serialize through the same resolver and demand every key in constants.json
        // comes back identical — a key that fails to bind reads back as 0/null and mismatches.
        var actual = JObject.Parse(JsonConvert.SerializeObject(config, UnityLoaderSettings));

        foreach (var property in expected.Properties())
        {
            var bound = actual[property.Name];
            Assert.True(bound is not null, $"'{property.Name}' did not bind at all");
            Assert.True(JToken.DeepEquals(property.Value, bound),
                $"'{property.Name}' bound wrong: expected {property.Value.ToString(Formatting.None)}, got {bound.ToString(Formatting.None)}");
        }

        Assert.Equal(expected.Count, actual.Count);
    }

    [Fact]
    public void ConstantsJsonSentinelValues_BindExactly()
    {
        // Independent of the round-trip mechanism: three values checked by hand, including
        // a camelCase formation multiplier — the value a snake_case naming strategy zeroes.
        var config = LoadConfig();

        Assert.Equal(0.205, config.ConversionBase);
        Assert.Equal(1.24, config.HomeAdvantage);
        Assert.Equal(1.0, config.FormationMods["4-4-2"].AtkMult);
    }

    [Fact]
    public void TeamsJsonBindsCompletely_EveryTeamAndPlayer()
    {
        var rawJson = File.ReadAllText(DesignPath("teams.json"));
        var expected = JArray.Parse(rawJson);

        var teams = JsonConvert.DeserializeObject<IReadOnlyList<TeamDefinition>>(rawJson, UnityLoaderSettings);
        Assert.NotNull(teams);
        Assert.Equal(expected.Count, teams.Count);

        for (var i = 0; i < teams.Count; i++)
        {
            var raw = (JObject)expected[i];
            var team = teams[i];

            Assert.Equal((string?)raw["name"], team.Name);
            // "ATK" is uppercase in teams.json — the key no naming convention can reach.
            Assert.Equal((int?)raw["ATK"], team.Atk);
            Assert.Equal((int?)raw["DEF"], team.Def);
            Assert.Equal((string?)raw["baseFormation"], team.BaseFormation);
            Assert.Equal((double?)raw["teamForm"], team.TeamForm);
            Assert.Equal(((JArray?)raw["namedPlayers"])?.Count, team.NamedPlayers.Count);

            for (var p = 0; p < team.NamedPlayers.Count; p++)
            {
                var rawPlayer = (JObject)raw["namedPlayers"]![p]!;
                Assert.Equal((string?)rawPlayer["id"], team.NamedPlayers[p].Id);
                Assert.Equal((string?)rawPlayer["role"], team.NamedPlayers[p].Role.ToString(),
                    ignoreCase: true);
            }
        }
    }

    [Fact]
    public void EngineRunsOnNewtonsoftLoadedConfig_Deterministically()
    {
        // End-to-end: the exact assembly + loader Unity will use actually simulates.
        var engine = new MatchEngine.MatchEngine(LoadConfig());
        var teams = LoadTeams();

        var first = engine.SimulateMatch(teams[0], teams[1], seed: 42);
        var second = engine.SimulateMatch(teams[0], teams[1], seed: 42);

        Assert.Equal(first.FullTimeHomeGoals, second.FullTimeHomeGoals);
        Assert.Equal(first.FullTimeAwayGoals, second.FullTimeAwayGoals);
        Assert.InRange(first.FullTimeHomeGoals + first.FullTimeAwayGoals, 0, 20);
    }

    [Fact]
    public void NewtonsoftDefaultSettings_StillDoNotBind_SoTheResolverStaysMandatory()
    {
        // Documents the trap. If this ever FAILS, default Newtonsoft started binding our
        // config — the shim/attribute situation changed and the Unity loader decision
        // must be revisited, not assumed.
        var rawJson = File.ReadAllText(DesignPath("constants.json"));
        var config = JsonConvert.DeserializeObject<EngineConfig>(rawJson);

        Assert.NotNull(config);
        Assert.Equal(0, config.SchemaVersion);
        Assert.Equal(0.0, config.ConversionBase);
        Assert.Null(config.FormationMods);
    }

    private static EngineConfig LoadConfig() =>
        JsonConvert.DeserializeObject<EngineConfig>(
            File.ReadAllText(DesignPath("constants.json")), UnityLoaderSettings)!;

    private static IReadOnlyList<TeamDefinition> LoadTeams() =>
        JsonConvert.DeserializeObject<IReadOnlyList<TeamDefinition>>(
            File.ReadAllText(DesignPath("teams.json")), UnityLoaderSettings)!;

    private static string DesignPath(string fileName)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "design")))
            {
                return Path.Combine(current.FullName, "design", fileName);
            }

            current = current.Parent!;
        }

        throw new DirectoryNotFoundException("Could not locate repo root.");
    }
}
