using System.Text.Json;
using System.Text.Json.Serialization;
using BallKnowledge.MatchEngine;

namespace BallKnowledge.Tests;

internal static class TestData
{
    public static EngineConfig LoadConfig()
    {
        var root = FindRepoRoot();
        var json = File.ReadAllText(Path.Combine(root, "design", "constants.json"));
        return JsonSerializer.Deserialize<EngineConfig>(json)!
               ?? throw new InvalidOperationException("Could not load config");
    }

    public static IReadOnlyList<TeamDefinition> LoadTeams()
    {
        var root = FindRepoRoot();
        var json = File.ReadAllText(Path.Combine(root, "design", "teams.json"));
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        return JsonSerializer.Deserialize<IReadOnlyList<TeamDefinition>>(json, options)!
               ?? throw new InvalidOperationException("Could not load teams");
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "design")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repo root for tests.");
    }
}
