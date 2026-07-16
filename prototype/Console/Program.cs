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
            "board" => RunBoard(engine, teams, filteredArgs.Skip(1).ToArray()),
            "stats" => RunStats(engine, teams, filteredArgs.Skip(1).ToArray()),
            _ => 1,
        };
    }

    // Tuning dashboard: simulate N matches across all fixtures, report realism metrics.
    private static int RunStats(MatchEngine engine, IReadOnlyList<TeamDefinition> teams, string[] args)
    {
        var n = 3000;
        for (var i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == "--n" && int.TryParse(args[i + 1], out var parsed)) n = parsed;
        }

        double totalGoals = 0, homeGoals = 0, awayGoals = 0;
        int home = 0, draw = 0, away = 0, over25 = 0, btts = 0;
        for (var i = 0; i < n; i++)
        {
            var h = i % teams.Count;
            var a = (i / teams.Count + h + 1) % teams.Count;
            if (a == h) a = (a + 1) % teams.Count;
            var m = engine.SimulateMatch(teams[h], teams[a], 100000 + i * 13);
            var hg = m.FullTimeHomeGoals;
            var ag = m.FullTimeAwayGoals;
            totalGoals += hg + ag; homeGoals += hg; awayGoals += ag;
            if (hg > ag) home++; else if (hg == ag) draw++; else away++;
            if (hg + ag > 2) over25++;
            if (hg > 0 && ag > 0) btts++;
        }

        Console.WriteLine($"Stats over {n} matches (all fixtures):");
        Console.WriteLine($"  Avg total goals : {totalGoals / n:F2}   (real football ~2.6-2.8)");
        Console.WriteLine($"  Avg home / away : {homeGoals / n:F2} / {awayGoals / n:F2}");
        Console.WriteLine($"  Home / Draw / Away wins : {100.0 * home / n:F1}% / {100.0 * draw / n:F1}% / {100.0 * away / n:F1}%   (real ~45/27/28)");
        Console.WriteLine($"  Over 2.5 goals  : {100.0 * over25 / n:F1}%   (real ~50%)");
        Console.WriteLine($"  Both teams score: {100.0 * btts / n:F1}%   (real ~50%)");
        return 0;
    }

    // Prints the FULL odds board (every market + every outcome) for a fixture, in American odds.
    private static int RunBoard(MatchEngine engine, IReadOnlyList<TeamDefinition> teams, string[] args)
    {
        var homeIdx = 0;
        var awayIdx = 1;
        for (var i = 0; i + 1 < args.Length; i++)
        {
            if (args[i] == "--home" && int.TryParse(args[i + 1], out var h)) homeIdx = h;
            if (args[i] == "--away" && int.TryParse(args[i + 1], out var a)) awayIdx = a;
        }

        var home = teams[homeIdx];
        var away = teams[awayIdx];
        var board = engine.PricePublicMarkets(home, away);

        Console.WriteLine($"Full odds board: {home.Name} (home) vs {away.Name} (away)");
        Console.WriteLine("Odds shown American / moneyline, with decimal in parentheses. Priced on public info only (no hidden factors).");
        foreach (var market in board.Markets)
        {
            Console.WriteLine();
            Console.WriteLine($"== {market.DisplayName} ==");
            foreach (var outcome in market.Outcomes)
            {
                Console.WriteLine($"  {outcome.DisplayName,-28} {ToAmerican(outcome.Odds),8}  ({outcome.Odds:F2})");
            }
        }

        return 0;
    }

    private static int RunMatch(MatchEngine engine, IReadOnlyList<TeamDefinition> teams, string[] args)
    {
        var seed = 12345;
        var homeIdx = 0;
        var awayIdx = 1;
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--seed" && int.TryParse(args[i + 1], out var parsed)) seed = parsed;
            if (args[i] == "--home" && int.TryParse(args[i + 1], out var h)) homeIdx = h;
            if (args[i] == "--away" && int.TryParse(args[i + 1], out var a)) awayIdx = a;
        }

        var match = engine.SimulateMatch(teams[homeIdx], teams[awayIdx], seed);
        var board = engine.PricePublicMarkets(teams[homeIdx], teams[awayIdx]);

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
