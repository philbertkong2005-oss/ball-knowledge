using System.Text.Json;
using System.Text.Json.Serialization;
using BallKnowledge.MatchEngine;

var exitCode = await ProgramEntry.RunAsync(args);
Environment.ExitCode = exitCode;

internal static class ProgramEntry
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0)
        {
            PrintUsage();
            return 1;
        }

        var designRoot = ResolveDesignRoot(args, out var filteredArgs);
        var config = await LoadConfigAsync(designRoot);
        var teams = await LoadTeamsAsync(designRoot);
        var engine = new MatchEngine(config);

        return filteredArgs[0] switch
        {
            "match" => RunMatch(engine, teams, filteredArgs.Skip(1).ToArray()),
            "validate" => RunValidate(engine, teams),
            _ => 1,
        };
    }

    private static int RunMatch(MatchEngine engine, IReadOnlyList<TeamDefinition> teams, string[] args)
    {
        var seed = 12345;
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seed" && int.TryParse(args[i + 1], out var parsed))
            {
                seed = parsed;
            }
        }

        var match = engine.SimulateMatch(teams[0], teams[1], seed);
        var board = engine.PricePublicMarkets(teams[0], teams[1]);

        Console.WriteLine("Ball Knowledge Match Demo");
        Console.WriteLine($"Seed: {seed}");
        Console.WriteLine($"{match.HomeTeam.Name} {match.FullTimeHomeGoals}-{match.FullTimeAwayGoals} {match.AwayTeam.Name}");
        Console.WriteLine($"Half-time: {match.HalfTimeHomeGoals}-{match.HalfTimeAwayGoals}");
        Console.WriteLine($"Shots: {match.HomeStats.Shots}-{match.AwayStats.Shots}");
        Console.WriteLine($"Shots on target: {match.HomeStats.ShotsOnTarget}-{match.AwayStats.ShotsOnTarget}");
        Console.WriteLine($"Corners: {match.HomeStats.Corners}-{match.AwayStats.Corners}");
        Console.WriteLine();
        Console.WriteLine("Hidden factors:");
        foreach (var factor in match.ActiveFactors)
        {
            Console.WriteLine($"- {factor.Name} ({factor.Scope})");
        }

        Console.WriteLine();
        Console.WriteLine("Selected markets:");
        foreach (var market in board.Markets.Where(m => m.Kind is MarketKind.OneXTwo or MarketKind.OverUnder or MarketKind.BothTeamsToScore).Take(4))
        {
            Console.WriteLine($"- {market.DisplayName}");
            foreach (var outcome in market.Outcomes.Take(3))
            {
                Console.WriteLine($"  {outcome.DisplayName}: {ToAmerican(outcome.Odds)} ({outcome.Odds:F2})");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Commentary:");
        foreach (var line in match.Commentary.OrderBy(item => item.Minute))
        {
            Console.WriteLine($"{line.Minute:00}' {line.Text}");
        }

        return 0;
    }

    private static int RunValidate(MatchEngine engine, IReadOnlyList<TeamDefinition> teams)
    {
        var report = engine.RunValidation(teams);
        Console.WriteLine("Ball Knowledge Validation Report");
        Console.WriteLine($"Fixtures: {report.FixtureCount}");
        Console.WriteLine($"Blind ROI: {report.Blind.Roi:P2} (95% CI {report.Blind.Interval.Lower:P2} to {report.Blind.Interval.Upper:P2})");
        Console.WriteLine($"Informed ROI: {report.Informed.Roi:P2} (95% CI {report.Informed.Interval.Lower:P2} to {report.Informed.Interval.Upper:P2})");
        Console.WriteLine($"Even-money win rate: {report.EvenMoneySubset.WinRate:P2} (95% CI {report.EvenMoneySubset.Interval.Lower:P2} to {report.EvenMoneySubset.Interval.Upper:P2}) on {report.EvenMoneySubset.Bets} bets");
        Console.WriteLine($"Blind gate passed: {report.BlindGatePassed}");
        Console.WriteLine($"Informed gate passed: {report.InformedGatePassed}");
        Console.WriteLine($"Even-money gate passed: {report.EvenMoneyGatePassed}");
        Console.WriteLine($"Gate 1 passed: {report.Gate1Passed}");
        foreach (var note in report.Notes)
        {
            Console.WriteLine($"Note: {note}");
        }

        return 0;
    }

    // Convert decimal odds to American / moneyline format (e.g. 1.29 -> -345, 5.48 -> +448).
    private static string ToAmerican(double dec)
    {
        if (dec <= 1.0) return "n/a";
        return dec >= 2.0
            ? $"+{(int)Math.Round((dec - 1.0) * 100)}"
            : $"-{(int)Math.Round(100.0 / (dec - 1.0))}";
    }

    private static async Task<EngineConfig> LoadConfigAsync(string designRoot)
    {
        var text = await File.ReadAllTextAsync(Path.Combine(designRoot, "constants.json"));
        return JsonSerializer.Deserialize<EngineConfig>(text)
               ?? throw new InvalidOperationException("Failed to load constants.json");
    }

    private static async Task<IReadOnlyList<TeamDefinition>> LoadTeamsAsync(string designRoot)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
        };
        var text = await File.ReadAllTextAsync(Path.Combine(designRoot, "teams.json"));
        return JsonSerializer.Deserialize<IReadOnlyList<TeamDefinition>>(text, options)
               ?? throw new InvalidOperationException("Failed to load teams.json");
    }

    private static string ResolveDesignRoot(string[] args, out string[] filteredArgs)
    {
        string? explicitDesignRoot = null;
        var remaining = new List<string>();
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--design-root" && i + 1 < args.Length)
            {
                explicitDesignRoot = args[++i];
                continue;
            }

            remaining.Add(args[i]);
        }

        filteredArgs = remaining.ToArray();
        if (!string.IsNullOrWhiteSpace(explicitDesignRoot))
        {
            return explicitDesignRoot;
        }

        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, "design");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate design/ relative to the executable. Use --design-root.");
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  match --seed 12345 [--design-root <path>]");
        Console.WriteLine("  validate [--design-root <path>]");
    }
}
