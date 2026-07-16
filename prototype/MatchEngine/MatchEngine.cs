using System.Globalization;

namespace BallKnowledge.MatchEngine;

public sealed class MatchEngine
{
    private static readonly string[] Formations =
    [
        "4-4-2",
        "4-3-3",
        "5-3-2",
        "4-2-3-1",
        "3-4-2-1",
        "4-5-1",
        "4-1-4-1",
        "3-5-2",
        "4-2-2-2",
        "3-4-3",
    ];

    private readonly EngineConfig _config;
    private readonly Dictionary<string, MarketCatalogue> _marketCache = new(StringComparer.Ordinal);

    public MatchEngine(EngineConfig config)
    {
        _config = config;
    }

    public EngineConfig Config => _config;

    public MatchResult SimulateMatch(TeamDefinition homeTeam, TeamDefinition awayTeam, int seed)
    {
        var resolved = ResolveFixture(homeTeam, awayTeam, seed, includeHiddenFactors: true);
        var simulation = SimulateResolvedFixture(resolved, seed, createCommentary: true);
        return ToMatchResult(resolved, seed, simulation);
    }

    public IReadOnlyList<AppliedFactor> SampleFactors(TeamDefinition homeTeam, TeamDefinition awayTeam, int seed)
    {
        return ResolveFixture(homeTeam, awayTeam, seed, includeHiddenFactors: true).Factors;
    }

    public ResolvedFixture ResolveFixture(TeamDefinition homeTeam, TeamDefinition awayTeam, int seed, bool includeHiddenFactors)
    {
        var rng = new Random(seed);
        var home = TeamRuntime.Create(homeTeam);
        var away = TeamRuntime.Create(awayTeam);
        var fixture = new ResolvedFixture(home, away);

        if (!includeHiddenFactors)
        {
            return fixture;
        }

        var requestedCount = DrawFactorCount(rng);
        var selectedKinds = WeightedWithoutReplacement(_config.FactorRarity, requestedCount * 4, rng);
        var blockedTeams = new HashSet<string>(StringComparer.Ordinal);
        var blockedPlayers = new HashSet<string>(StringComparer.Ordinal);

        foreach (var factorName in selectedKinds)
        {
            if (fixture.Factors.Count >= requestedCount)
            {
                break;
            }

            if (TryApplyFactor(fixture, factorName, rng, blockedTeams, blockedPlayers, out var factor))
            {
                fixture.Factors.Add(factor);
            }
        }

        return fixture;
    }

    public MarketCatalogue PricePublicMarkets(TeamDefinition homeTeam, TeamDefinition awayTeam)
    {
        var key = $"PUBLIC|{homeTeam.Name}|{awayTeam.Name}|{_config.PricingSimCount}";
        if (_marketCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var resolved = ResolveFixture(homeTeam, awayTeam, seed: 0, includeHiddenFactors: false);
        var priced = PriceResolvedFixture(resolved, StableHash(key), _config.PricingSimCount);
        _marketCache[key] = priced;
        return priced;
    }

    public MarketCatalogue PriceTrueMarkets(ResolvedFixture fixture)
    {
        var key = $"TRUE|{fixture.Signature}|{_config.PricingSimCount}";
        if (_marketCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var priced = PriceResolvedFixture(fixture, StableHash(key), _config.PricingSimCount);
        _marketCache[key] = priced;
        return priced;
    }

    public ValidationReport RunValidation(IReadOnlyList<TeamDefinition> teams, int? seed = null)
    {
        var fixtureCount = _config.ValidationFixtureCount;
        var blindReturns = new List<FixtureReturn>(fixtureCount);
        var informedReturns = new List<FixtureReturn>(fixtureCount);
        var evenMoneyReturns = new List<FixtureReturn>(fixtureCount);
        var notes = new List<string>();
        var validationSeed = seed ?? 20260715;
        var blindRandom = new Random(validationSeed ^ 0x514A7);

        for (var i = 0; i < fixtureCount; i++)
        {
            var homeIndex = i % teams.Count;
            var awayIndex = (i / teams.Count + homeIndex + 1) % teams.Count;
            if (awayIndex == homeIndex)
            {
                awayIndex = (awayIndex + 1) % teams.Count;
            }

            var home = teams[homeIndex];
            var away = teams[awayIndex];
            var matchSeed = validationSeed + (i * 7919);
            var resolved = ResolveFixture(home, away, matchSeed, includeHiddenFactors: true);
            var actualSimulation = SimulateResolvedFixture(resolved, matchSeed, createCommentary: false);
            var actualResult = ToMatchResult(resolved, matchSeed, actualSimulation);
            var publicBoard = PricePublicMarkets(home, away);
            var trueBoard = PriceTrueMarkets(resolved);

            var blindMarket = publicBoard.Markets.Single(m => m.Kind == MarketKind.OneXTwo);
            var blindChoice = blindMarket.Outcomes[blindRandom.Next(blindMarket.Outcomes.Count)];
            blindReturns.Add(new FixtureReturn(Settle(blindMarket, blindChoice, actualResult), 1d));

            var fixtureInformedProfit = 0d;
            var fixtureInformedStake = 0d;
            var fixtureEvenMoneyProfit = 0d;
            var fixtureEvenMoneyStake = 0d;
            var fixtureEvenMoneyWins = 0;
            var fixtureEvenMoneyBets = 0;

            foreach (var boardMarket in publicBoard.Markets)
            {
                var trueMarket = trueBoard.Markets.Single(m => m.MarketId == boardMarket.MarketId);
                var best = FindBestSelection(boardMarket, trueMarket);
                if (best is null || best.Edge <= _config.EdgeThreshold)
                {
                    continue;
                }

                var outcome = boardMarket.Outcomes.Single(o => o.OutcomeId == best.OutcomeId);
                var settlement = Settle(boardMarket, outcome, actualResult);
                fixtureInformedProfit += settlement;
                fixtureInformedStake += 1d;

                if (best.Odds >= _config.EvenMoneyOddsRange[0] && best.Odds <= _config.EvenMoneyOddsRange[1])
                {
                    fixtureEvenMoneyProfit += settlement;
                    fixtureEvenMoneyStake += 1d;
                    fixtureEvenMoneyBets++;
                    if (settlement > 0d)
                    {
                        fixtureEvenMoneyWins++;
                    }
                }
            }

            informedReturns.Add(new FixtureReturn(fixtureInformedProfit, fixtureInformedStake));
            evenMoneyReturns.Add(new FixtureReturn(fixtureEvenMoneyProfit, fixtureEvenMoneyStake, fixtureEvenMoneyWins, fixtureEvenMoneyBets));
        }

        var blindMetric = CalculateRoiMetric(blindReturns, validationSeed + 11);
        var informedMetric = CalculateRoiMetric(informedReturns, validationSeed + 23);
        var evenMetric = CalculateWinRateMetric(evenMoneyReturns, validationSeed + 37);

        var blindGate = blindMetric.Roi >= _config.BlindRoiBand[0] &&
                        blindMetric.Roi <= _config.BlindRoiBand[1] &&
                        blindMetric.Interval.Lower >= _config.BlindRoiBand[0] &&
                        blindMetric.Interval.Upper <= _config.BlindRoiBand[1];

        var informedGate = informedMetric.Roi >= _config.InformedRoiMin &&
                           informedMetric.Interval.Lower >= _config.InformedRoiMin;

        var evenGate = evenMetric.Bets < _config.MinEvenMoneyBets ||
                       (evenMetric.WinRate >= _config.InformedWinRateMin &&
                        evenMetric.WinRate <= _config.InformedWinRateMax &&
                        evenMetric.Interval.Lower >= _config.InformedWinRateMin &&
                        evenMetric.Interval.Upper <= _config.InformedWinRateMax);

        if (evenMetric.Bets < _config.MinEvenMoneyBets)
        {
            notes.Add($"Even-money subset below gate threshold: {evenMetric.Bets} bets < {_config.MinEvenMoneyBets}.");
        }

        return new ValidationReport(
            fixtureCount,
            blindMetric,
            informedMetric,
            evenMetric with { IsGateable = evenMetric.Bets >= _config.MinEvenMoneyBets },
            blindGate,
            informedGate,
            evenGate,
            blindGate && informedGate && evenGate,
            notes);
    }

    public AccumulatorQuote PriceAccumulator(IReadOnlyList<AccumulatorLeg> legs)
    {
        var odds = 1d;
        var won = true;
        foreach (var leg in legs)
        {
            odds *= leg.Odds;
            won &= leg.Won;
        }

        return new AccumulatorQuote(odds, won);
    }

    internal static double Settle(MarketQuote market, OutcomeQuote outcome, MatchResult result)
    {
        return EvaluateSelection(market, outcome.OutcomeId, result) switch
        {
            SettlementResult.Win => outcome.Odds - 1d,
            SettlementResult.Push => 0d,
            _ => -1d,
        };
    }

    internal static SettlementResult EvaluateSelection(MarketQuote market, string outcomeId, MatchResult result)
    {
        return market.Kind switch
        {
            MarketKind.OneXTwo => EvaluateOneXTwo(outcomeId, result),
            MarketKind.OverUnder => EvaluateOverUnder(outcomeId, result),
            MarketKind.CorrectScore => EvaluateCorrectScore(outcomeId, result),
            MarketKind.HalfTimeFullTime => EvaluateHtFt(outcomeId, result),
            MarketKind.BothTeamsToScore => EvaluateBtts(outcomeId, result),
            MarketKind.AsianHandicap => EvaluateHandicap(outcomeId, result),
            MarketKind.FirstGoalscorer => EvaluateFirstGoalscorer(outcomeId, result),
            MarketKind.AnytimeGoalscorer => EvaluateAnytimeGoalscorer(outcomeId, result),
            _ => SettlementResult.Lose,
        };
    }

    internal static int SamplePoisson(double lambda, Random rng)
    {
        if (lambda <= 0d)
        {
            return 0;
        }

        var l = Math.Exp(-lambda);
        var k = 0;
        var p = 1d;
        do
        {
            k++;
            p *= rng.NextDouble();
        }
        while (p > l);

        return k - 1;
    }

    internal static double SoftMultiplier(double stat) => Clamp(0.5d + (stat / 100d), 0.2d, 2d);

    internal static double Clamp(double value, double min, double max) => Math.Max(min, Math.Min(max, value));

    internal static IReadOnlyList<string> WeightedWithoutReplacement(IReadOnlyDictionary<string, double> weights, int count, Random rng)
    {
        var pool = weights.ToDictionary(kvp => kvp.Key, kvp => kvp.Value, StringComparer.Ordinal);
        var selected = new List<string>();
        while (pool.Count > 0 && selected.Count < count)
        {
            var total = pool.Sum(kvp => kvp.Value);
            var roll = rng.NextDouble() * total;
            var cumulative = 0d;
            foreach (var kvp in pool.ToList())
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                {
                    selected.Add(kvp.Key);
                    pool.Remove(kvp.Key);
                    break;
                }
            }
        }

        return selected;
    }

    internal static int StableHash(string text)
    {
        unchecked
        {
            var hash = 2166136261;
            foreach (var ch in text)
            {
                hash ^= ch;
                hash *= 16777619;
            }

            return (int)hash;
        }
    }

    private MatchResult ToMatchResult(ResolvedFixture fixture, int seed, MatchSimulation simulation)
    {
        return new MatchResult(
            seed,
            fixture.Home.Source,
            fixture.Away.Source,
            new TeamMatchStats(fixture.Home.Source.Name, simulation.HomeGoals, simulation.HomeHalfTimeGoals, simulation.HomeShots, simulation.HomeShotsOnTarget, simulation.HomeCorners, simulation.HomeSaves),
            new TeamMatchStats(fixture.Away.Source.Name, simulation.AwayGoals, simulation.AwayHalfTimeGoals, simulation.AwayShots, simulation.AwayShotsOnTarget, simulation.AwayCorners, simulation.AwaySaves),
            GetOutcome(simulation.HomeGoals, simulation.AwayGoals),
            simulation.Goals.OrderBy(goal => goal.Minute).ToList(),
            fixture.Factors.ToList(),
            simulation.Commentary.OrderBy(line => line.Minute).ToList(),
            fixture.Home.ToDebugState(),
            fixture.Away.ToDebugState());
    }

    private MatchSimulation SimulateResolvedFixture(ResolvedFixture fixture, int baseSeed, bool createCommentary)
    {
        var rng = new Random(baseSeed);
        var commentary = new List<CommentaryLine>();
        if (createCommentary)
        {
            commentary.Add(new CommentaryLine(0, CommentaryEventType.Kickoff, CommentaryTemplates.RenderKickoff(fixture.Home.Source.Name, fixture.Away.Source.Name, rng)));
            var minute = 2;
            foreach (var factor in fixture.Factors)
            {
                commentary.Add(new CommentaryLine(minute++, CommentaryEventType.FactorLeak, CommentaryTemplates.RenderFactorLeak(factor, fixture.Home.Source.Name, fixture.Away.Source.Name)));
            }
        }

        var goals = new List<GoalEvent>();
        var homeShots = 0;
        var awayShots = 0;
        var homeShotsOnTarget = 0;
        var awayShotsOnTarget = 0;
        var homeGoals = 0;
        var awayGoals = 0;
        var homeHalfTimeGoals = 0;
        var awayHalfTimeGoals = 0;
        var homeCorners = 0;
        var awayCorners = 0;
        var homeSaves = 0;
        var awaySaves = 0;

        var firstHalfWeight = 1d / (1d + _config.SecondHalfFactor);
        var secondHalfWeight = _config.SecondHalfFactor / (1d + _config.SecondHalfFactor);

        SimulateHalf(fixture, rng, firstHalfWeight, 0, goals, commentary, createCommentary, ref homeShots, ref awayShots, ref homeShotsOnTarget, ref awayShotsOnTarget, ref homeGoals, ref awayGoals, ref homeCorners, ref awayCorners, ref homeSaves, ref awaySaves);
        homeHalfTimeGoals = homeGoals;
        awayHalfTimeGoals = awayGoals;

        if (createCommentary)
        {
            commentary.Add(new CommentaryLine(45, CommentaryEventType.HalfTime, CommentaryTemplates.RenderHalfTime(fixture.Home.Source.Name, fixture.Away.Source.Name, homeHalfTimeGoals, awayHalfTimeGoals, rng)));
        }

        SimulateHalf(fixture, rng, secondHalfWeight, 45, goals, commentary, createCommentary, ref homeShots, ref awayShots, ref homeShotsOnTarget, ref awayShotsOnTarget, ref homeGoals, ref awayGoals, ref homeCorners, ref awayCorners, ref homeSaves, ref awaySaves);

        if (createCommentary)
        {
            commentary.Add(new CommentaryLine(90, CommentaryEventType.FullTime, CommentaryTemplates.RenderFullTime(fixture.Home.Source.Name, fixture.Away.Source.Name, homeGoals, awayGoals, rng)));
        }

        return new MatchSimulation(homeGoals, awayGoals, homeHalfTimeGoals, awayHalfTimeGoals, homeShots, awayShots, homeShotsOnTarget, awayShotsOnTarget, homeCorners, awayCorners, homeSaves, awaySaves, goals, commentary);
    }

    private void SimulateHalf(
        ResolvedFixture fixture,
        Random rng,
        double halfWeight,
        int baseMinute,
        List<GoalEvent> goals,
        List<CommentaryLine> commentary,
        bool createCommentary,
        ref int homeShots,
        ref int awayShots,
        ref int homeShotsOnTarget,
        ref int awayShotsOnTarget,
        ref int homeGoals,
        ref int awayGoals,
        ref int homeCorners,
        ref int awayCorners,
        ref int homeSaves,
        ref int awaySaves)
    {
        var homeState = BuildHalfState(fixture.Home, fixture.Away, isHome: true, halfWeight);
        var awayState = BuildHalfState(fixture.Away, fixture.Home, isHome: false, halfWeight);

        var homeHalfShots = SamplePoisson(homeState.ShotsExpectation, rng);
        var awayHalfShots = SamplePoisson(awayState.ShotsExpectation, rng);
        homeShots += homeHalfShots;
        awayShots += awayHalfShots;

        var homeHalfCorners = SamplePoisson(homeState.CornersExpectation, rng);
        var awayHalfCorners = SamplePoisson(awayState.CornersExpectation, rng);
        homeCorners += homeHalfCorners;
        awayCorners += awayHalfCorners;

        if (createCommentary)
        {
            for (var i = 0; i < homeHalfCorners; i++)
            {
                commentary.Add(new CommentaryLine(baseMinute + 1 + rng.Next(44), CommentaryEventType.Corner, CommentaryTemplates.RenderCorner(fixture.Home.Source.Name, rng)));
            }

            for (var i = 0; i < awayHalfCorners; i++)
            {
                commentary.Add(new CommentaryLine(baseMinute + 1 + rng.Next(44), CommentaryEventType.Corner, CommentaryTemplates.RenderCorner(fixture.Away.Source.Name, rng)));
            }
        }

        SimulateShots(fixture.Home, fixture.Away, homeState, homeHalfShots, true, baseMinute, rng, goals, commentary, createCommentary, ref homeShotsOnTarget, ref homeGoals, ref awaySaves);
        SimulateShots(fixture.Away, fixture.Home, awayState, awayHalfShots, false, baseMinute, rng, goals, commentary, createCommentary, ref awayShotsOnTarget, ref awayGoals, ref homeSaves);
    }

    private void SimulateShots(
        TeamRuntime attackingTeam,
        TeamRuntime defendingTeam,
        HalfState state,
        int shots,
        bool isHomeTeam,
        int baseMinute,
        Random rng,
        List<GoalEvent> goals,
        List<CommentaryLine> commentary,
        bool createCommentary,
        ref int shotsOnTarget,
        ref int goalsScored,
        ref int opponentSaves)
    {
        for (var i = 0; i < shots; i++)
        {
            var shooter = attackingTeam.DrawShooter(rng);
            var minute = baseMinute + 1 + rng.Next(44);
            var pOnTarget = Clamp(state.OnTargetBase * SoftMultiplier(shooter.Finishing), 0.05d, 0.95d);
            if (rng.NextDouble() > pOnTarget)
            {
                if (createCommentary && rng.NextDouble() < 0.18d)
                {
                    commentary.Add(new CommentaryLine(minute, CommentaryEventType.Chance, CommentaryTemplates.RenderChance(attackingTeam.Source.Name, shooter.DisplayName, rng)));
                }

                continue;
            }

            shotsOnTarget++;
            var pGoal = Clamp(
                state.ConversionBase * SoftMultiplier(shooter.Finishing) * (1.5d - SoftMultiplier(defendingTeam.GetEffectiveKeeperReliability()) / 2d),
                0.02d,
                0.98d);

            if (rng.NextDouble() <= pGoal)
            {
                goalsScored++;
                var assist = attackingTeam.DrawAssist(shooter, rng, state.AssistChance);
                goals.Add(new GoalEvent(minute, isHomeTeam, shooter.Id, shooter.DisplayName, shooter.IsOtherPlayer, assist?.Id, assist?.DisplayName));
                if (createCommentary)
                {
                    commentary.Add(new CommentaryLine(minute, CommentaryEventType.Goal, CommentaryTemplates.RenderGoal(attackingTeam.Source.Name, shooter.DisplayName, minute, assist?.DisplayName, rng)));
                }
            }
            else
            {
                opponentSaves++;
                if (createCommentary)
                {
                    commentary.Add(new CommentaryLine(minute, CommentaryEventType.Save, CommentaryTemplates.RenderSave(defendingTeam.Source.Name, defendingTeam.GetKeeperName(), rng)));
                }
            }
        }
    }

    private HalfState BuildHalfState(TeamRuntime attacking, TeamRuntime defending, bool isHome, double halfWeight)
    {
        var formation = _config.FormationMods[attacking.Formation];
        var defendingFormation = _config.FormationMods[defending.Formation];
        var basePool = attacking.GetBaseAttackPool();
        var livePool = attacking.GetLiveAttackPool();
        var attackScale = basePool <= 0d ? 1d : livePool / basePool;
        var atkEff = attacking.Source.Atk *
                     formation.AtkMult *
                     attacking.Source.TeamForm *
                     attackScale *
                     attacking.TeamAtkModifier *
                     attacking.MatchAttackModifier *
                     (isHome ? _config.HomeAdvantage : 1d);
        var defEff = defending.Source.Def *
                     defendingFormation.DefMult *
                     defending.Source.TeamForm *
                     defending.TeamDefModifier;
        var ratio = Math.Max(0.35d, atkEff / Math.Max(1d, defEff));
        var heightFactor = Clamp(0.5d + (attacking.Source.Height / 100d), 0.5d, 1.5d);
        return new HalfState(
            _config.ShotBase * halfWeight * ratio * formation.ShotMult * attacking.MatchShotModifier,
            _config.CornerBase * halfWeight * ratio * heightFactor * formation.CornerMult,
            _config.OnTargetBase * attacking.MatchOnTargetModifier,
            _config.ConversionBase * attacking.MatchConversionModifier,
            Clamp(_config.AssistRate * formation.PassMult, 0d, 1d));
    }

    private MarketCatalogue PriceResolvedFixture(ResolvedFixture fixture, int baseSeed, int simulationCount)
    {
        var exactScores = new Dictionary<string, int>(StringComparer.Ordinal);
        var htft = new Dictionary<string, int>(StringComparer.Ordinal);
        var firstScorer = new Dictionary<string, int>(StringComparer.Ordinal);
        var anytimeScorer = new Dictionary<string, int>(StringComparer.Ordinal);
        var oneXTwo = new Dictionary<string, int>(StringComparer.Ordinal) { ["H"] = 0, ["D"] = 0, ["A"] = 0 };
        var btts = new Dictionary<string, int>(StringComparer.Ordinal) { ["Y"] = 0, ["N"] = 0 };
        var ou = _config.OverUnderLines.ToDictionary(line => line, _ => new Dictionary<string, int>(StringComparer.Ordinal) { ["O"] = 0, ["U"] = 0 });
        var handicaps = _config.HandicapLines.ToDictionary(line => line, _ => new HandicapTally());

        foreach (var outcome in GetEligibleScorerOutcomes(fixture))
        {
            firstScorer[outcome] = 0;
            anytimeScorer[outcome] = 0;
        }

        for (var i = 0; i < simulationCount; i++)
        {
            var simulation = SimulateResolvedFixture(fixture, baseSeed + (i * 17), createCommentary: false);
            var match = ToMatchResult(fixture, baseSeed + (i * 17), simulation);
            oneXTwo[EncodeMatchOutcome(match.FullTimeHomeGoals, match.FullTimeAwayGoals)]++;

            var totalGoals = match.FullTimeHomeGoals + match.FullTimeAwayGoals;
            foreach (var line in _config.OverUnderLines)
            {
                ou[line][totalGoals > line ? "O" : "U"]++;
            }

            foreach (var line in _config.HandicapLines)
            {
                AccumulateHandicap(handicaps[line], line, match);
            }

            var scoreKey = match.FullTimeHomeGoals <= _config.CorrectScoreCap && match.FullTimeAwayGoals <= _config.CorrectScoreCap
                ? $"{match.FullTimeHomeGoals}-{match.FullTimeAwayGoals}"
                : "AOS";
            exactScores[scoreKey] = exactScores.TryGetValue(scoreKey, out var scoreCount) ? scoreCount + 1 : 1;

            var htftKey = $"{EncodeMatchOutcome(match.HalfTimeHomeGoals, match.HalfTimeAwayGoals)}/{EncodeMatchOutcome(match.FullTimeHomeGoals, match.FullTimeAwayGoals)}";
            htft[htftKey] = htft.TryGetValue(htftKey, out var htftCount) ? htftCount + 1 : 1;
            btts[(match.FullTimeHomeGoals > 0 && match.FullTimeAwayGoals > 0) ? "Y" : "N"]++;

            var first = match.Goals.OrderBy(goal => goal.Minute).FirstOrDefault();
            if (first is null)
            {
                firstScorer["NOGOAL"]++;
                anytimeScorer["NOGOAL"]++;
            }
            else
            {
                firstScorer[first.IsOtherPlayer ? "OTHER" : first.ScorerId]++;
                foreach (var scorerId in match.Goals.Where(goal => !goal.IsOtherPlayer).Select(goal => goal.ScorerId).Distinct())
                {
                    anytimeScorer[scorerId]++;
                }

                if (match.Goals.Any(goal => goal.IsOtherPlayer))
                {
                    anytimeScorer["OTHER"]++;
                }
            }
        }

        var markets = new List<MarketQuote>
        {
            BuildClosedBook(
                MarketKind.OneXTwo,
                "1X2",
                "1X2",
                oneXTwo.Select(kvp => (kvp.Key, kvp.Key switch
                {
                    "H" => "Home",
                    "D" => "Draw",
                    _ => "Away",
                }, kvp.Value / (double)simulationCount)).ToList()),
        };

        foreach (var line in _config.OverUnderLines)
        {
            var tally = ou[line];
            var lineLabel = line.ToString("0.0", CultureInfo.InvariantCulture);
            markets.Add(BuildClosedBook(
                MarketKind.OverUnder,
                $"OU:{lineLabel}",
                $"Over/Under {lineLabel}",
                new List<(string Id, string Label, double FairProbability)>
                {
                    ($"O:{lineLabel}", $"Over {lineLabel}", tally["O"] / (double)simulationCount),
                    ($"U:{lineLabel}", $"Under {lineLabel}", tally["U"] / (double)simulationCount),
                }));
        }

        foreach (var line in _config.HandicapLines)
        {
            var handicapBook = BuildHandicapBook(line, handicaps[line], simulationCount);
            // A real book only offers handicap lines where both sides are sensibly bettable.
            // Drop degenerate lines (e.g. giving the favourite a head start) whose either side pays below the floor.
            if (handicapBook.Outcomes.All(o => o.Odds >= _config.MinOfferedOdds))
            {
                markets.Add(handicapBook);
            }
        }

        markets.Add(BuildClosedBook(
            MarketKind.CorrectScore,
            "CS",
            "Correct Score",
            exactScores
                .OrderBy(kvp => kvp.Key == "AOS" ? int.MaxValue : 0)
                .ThenBy(kvp => kvp.Key)
                .Select(kvp => (kvp.Key, kvp.Key == "AOS" ? "Any Other Score" : kvp.Key, kvp.Value / (double)simulationCount))
                .ToList()));

        markets.Add(BuildClosedBook(
            MarketKind.HalfTimeFullTime,
            "HTFT",
            "Half-Time / Full-Time",
            htft.OrderBy(kvp => kvp.Key).Select(kvp => (kvp.Key, kvp.Key, kvp.Value / (double)simulationCount)).ToList()));

        markets.Add(BuildClosedBook(
            MarketKind.BothTeamsToScore,
            "BTTS",
            "Both Teams To Score",
            new List<(string Id, string Label, double FairProbability)>
            {
                ("Y", "Yes", btts["Y"] / (double)simulationCount),
                ("N", "No", btts["N"] / (double)simulationCount),
            }));

        markets.Add(BuildClosedBook(
            MarketKind.FirstGoalscorer,
            "FGS",
            "First Goalscorer",
            firstScorer.Select(kvp => (kvp.Key, LabelScorerOutcome(kvp.Key, fixture), kvp.Value / (double)simulationCount)).ToList()));

        markets.Add(BuildAnytimeBook(
            "AGS",
            "Anytime Goalscorer",
            anytimeScorer.Select(kvp => (kvp.Key, LabelScorerOutcome(kvp.Key, fixture), kvp.Value / (double)simulationCount)).ToList()));

        return new MarketCatalogue(markets);
    }

    private MarketQuote BuildAnytimeBook(string marketId, string displayName, IReadOnlyList<(string Id, string Label, double FairProbability)> outcomes)
    {
        var quotes = outcomes
            .Where(item => item.FairProbability > 0d)
            .Select(item => new OutcomeQuote(item.Id, item.Label, item.FairProbability, 1d / Math.Max(0.0001d, item.FairProbability * _config.BookmakerOverround), false))
            .ToList();
        return new MarketQuote(MarketKind.AnytimeGoalscorer, marketId, displayName, false, false, quotes);
    }

    private MarketCatalogue EstimateMarketsFast(ResolvedFixture fixture)
    {
        var firstHalfHome = BuildHalfState(fixture.Home, fixture.Away, isHome: true, 1d / (1d + _config.SecondHalfFactor));
        var firstHalfAway = BuildHalfState(fixture.Away, fixture.Home, isHome: false, 1d / (1d + _config.SecondHalfFactor));
        var secondHalfHome = BuildHalfState(fixture.Home, fixture.Away, isHome: true, _config.SecondHalfFactor / (1d + _config.SecondHalfFactor));
        var secondHalfAway = BuildHalfState(fixture.Away, fixture.Home, isHome: false, _config.SecondHalfFactor / (1d + _config.SecondHalfFactor));

        var homeFirstLambda = EstimateGoalLambda(fixture.Home, fixture.Away, firstHalfHome);
        var awayFirstLambda = EstimateGoalLambda(fixture.Away, fixture.Home, firstHalfAway);
        var homeSecondLambda = EstimateGoalLambda(fixture.Home, fixture.Away, secondHalfHome);
        var awaySecondLambda = EstimateGoalLambda(fixture.Away, fixture.Home, secondHalfAway);
        var homeFullLambda = homeFirstLambda + homeSecondLambda;
        var awayFullLambda = awayFirstLambda + awaySecondLambda;

        var maxGoals = Math.Max(_config.CorrectScoreCap + 4, 10);
        var fullMatrix = BuildScoreMatrix(homeFullLambda, awayFullLambda, maxGoals);
        var firstHalfMatrix = BuildScoreMatrix(homeFirstLambda, awayFirstLambda, maxGoals);
        var secondHalfMatrix = BuildScoreMatrix(homeSecondLambda, awaySecondLambda, maxGoals);

        var oneXTwo = new List<(string Id, string Label, double FairProbability)>
        {
            ("H", "Home", SumOutcomes(fullMatrix, static (h, a) => h > a)),
            ("D", "Draw", SumOutcomes(fullMatrix, static (h, a) => h == a)),
            ("A", "Away", SumOutcomes(fullMatrix, static (h, a) => h < a)),
        };

        var markets = new List<MarketQuote>
        {
            BuildClosedBook(MarketKind.OneXTwo, "1X2", "1X2", oneXTwo),
        };

        foreach (var line in _config.OverUnderLines)
        {
            var lineLabel = line.ToString("0.0", CultureInfo.InvariantCulture);
            markets.Add(BuildClosedBook(
                MarketKind.OverUnder,
                $"OU:{lineLabel}",
                $"Over/Under {lineLabel}",
                new List<(string Id, string Label, double FairProbability)>
                {
                    ($"O:{lineLabel}", $"Over {lineLabel}", SumOutcomes(fullMatrix, (h, a) => h + a > line)),
                    ($"U:{lineLabel}", $"Under {lineLabel}", SumOutcomes(fullMatrix, (h, a) => h + a < line)),
                }));
        }

        foreach (var line in _config.HandicapLines)
        {
            var targetReturn = 1d / _config.BookmakerOverround;
            var homeWin = SumOutcomes(fullMatrix, (h, a) => h + line > a);
            var push = SumOutcomes(fullMatrix, (h, a) => Math.Abs((h + line) - a) < 0.000001d);
            var awayWin = SumOutcomes(fullMatrix, (h, a) => h + line < a);
            var lineLabel = line.ToString("0.0", CultureInfo.InvariantCulture);
            markets.Add(new MarketQuote(
                MarketKind.AsianHandicap,
                $"AH:{lineLabel}",
                $"Asian Handicap {lineLabel}",
                true,
                false,
                new List<OutcomeQuote>
                {
                    new($"H:{lineLabel}", $"Home {lineLabel}", homeWin, (targetReturn - push) / Math.Max(0.0001d, homeWin), false, push),
                    new($"A:{(-line).ToString("0.0", CultureInfo.InvariantCulture)}", $"Away {(-line).ToString("0.0", CultureInfo.InvariantCulture)}", awayWin, (targetReturn - push) / Math.Max(0.0001d, awayWin), false, push),
                }));
        }

        var correctScores = new List<(string Id, string Label, double FairProbability)>();
        var enumeratedMass = 0d;
        for (var homeGoals = 0; homeGoals <= _config.CorrectScoreCap; homeGoals++)
        {
            for (var awayGoals = 0; awayGoals <= _config.CorrectScoreCap; awayGoals++)
            {
                var probability = fullMatrix[homeGoals, awayGoals];
                enumeratedMass += probability;
                correctScores.Add(($"{homeGoals}-{awayGoals}", $"{homeGoals}-{awayGoals}", probability));
            }
        }

        correctScores.Add(("AOS", "Any Other Score", Math.Max(0d, 1d - enumeratedMass)));
        markets.Add(BuildClosedBook(MarketKind.CorrectScore, "CS", "Correct Score", correctScores));

        var htft = new List<(string Id, string Label, double FairProbability)>();
        foreach (var ht in new[] { "H", "D", "A" })
        {
            foreach (var ft in new[] { "H", "D", "A" })
            {
                var probability = 0d;
                for (var hh = 0; hh <= maxGoals; hh++)
                {
                    for (var ha = 0; ha <= maxGoals; ha++)
                    {
                        if (EncodeMatchOutcome(hh, ha) != ht)
                        {
                            continue;
                        }

                        for (var sh = 0; sh <= maxGoals - hh; sh++)
                        {
                            for (var sa = 0; sa <= maxGoals - ha; sa++)
                            {
                                if (EncodeMatchOutcome(hh + sh, ha + sa) == ft)
                                {
                                    probability += firstHalfMatrix[hh, ha] * secondHalfMatrix[sh, sa];
                                }
                            }
                        }
                    }
                }

                htft.Add(($"{ht}/{ft}", $"{ht}/{ft}", probability));
            }
        }

        markets.Add(BuildClosedBook(MarketKind.HalfTimeFullTime, "HTFT", "Half-Time / Full-Time", htft));
        markets.Add(BuildClosedBook(
            MarketKind.BothTeamsToScore,
            "BTTS",
            "Both Teams To Score",
            new List<(string Id, string Label, double FairProbability)>
            {
                ("Y", "Yes", SumOutcomes(fullMatrix, static (h, a) => h > 0 && a > 0)),
                ("N", "No", SumOutcomes(fullMatrix, static (h, a) => h == 0 || a == 0)),
            }));

        var homeShares = BuildScorerShares(fixture.Home);
        var awayShares = BuildScorerShares(fixture.Away);
        var totalLambda = homeFullLambda + awayFullLambda;
        var noGoal = Math.Exp(-totalLambda);
        var firstScorer = new List<(string Id, string Label, double FairProbability)>();

        foreach (var share in homeShares)
        {
            firstScorer.Add((share.Id, share.Label, (1d - noGoal) * (homeFullLambda / Math.Max(0.0001d, totalLambda)) * share.Share));
        }

        foreach (var share in awayShares)
        {
            firstScorer.Add((share.Id, share.Label, (1d - noGoal) * (awayFullLambda / Math.Max(0.0001d, totalLambda)) * share.Share));
        }

        firstScorer.Add(("NOGOAL", "No Goalscorer", noGoal));
        markets.Add(BuildClosedBook(MarketKind.FirstGoalscorer, "FGS", "First Goalscorer", CollapseDuplicateOutcomes(firstScorer)));

        var anytime = new List<(string Id, string Label, double FairProbability)>();
        foreach (var share in homeShares)
        {
            anytime.Add((share.Id, share.Label, 1d - Math.Exp(-(homeFullLambda * share.Share))));
        }

        foreach (var share in awayShares)
        {
            anytime.Add((share.Id, share.Label, 1d - Math.Exp(-(awayFullLambda * share.Share))));
        }

        anytime.Add(("NOGOAL", "No Goalscorer", noGoal));
        markets.Add(BuildAnytimeBook("AGS", "Anytime Goalscorer", CollapseDuplicateOutcomes(anytime)));

        return new MarketCatalogue(markets);
    }

    private double EstimateGoalLambda(TeamRuntime attacking, TeamRuntime defending, HalfState state)
    {
        var shooterWeights = attacking.AvailableNamedScorers
            .Select(player => (weight: player.LiveInvolvement, finishing: player.EffectiveFinishing))
            .ToList();
        var genericWeight = attacking.GenericProfile.StarterInvolvement + attacking.NamedPlayers.Count(player => !player.Available && player.Role != PlayerRole.Keeper) * attacking.GenericProfile.SubstituteInvolvement;
        shooterWeights.Add((genericWeight, attacking.GenericProfile.StarterFinishing));
        var totalWeight = shooterWeights.Sum(item => item.weight);
        var weightedFinishing = shooterWeights.Sum(item => item.weight * item.finishing) / Math.Max(1d, totalWeight);
        var onTarget = Clamp(state.OnTargetBase * SoftMultiplier(weightedFinishing), 0.05d, 0.95d);
        var goalGivenOnTarget = Clamp(
            state.ConversionBase * SoftMultiplier(weightedFinishing) * (1.5d - SoftMultiplier(defending.GetEffectiveKeeperReliability()) / 2d),
            0.02d,
            0.98d);
        return state.ShotsExpectation * onTarget * goalGivenOnTarget;
    }

    private static double[,] BuildScoreMatrix(double homeLambda, double awayLambda, int maxGoals)
    {
        var matrix = new double[maxGoals + 1, maxGoals + 1];
        var home = Enumerable.Range(0, maxGoals + 1).Select(goals => PoissonProbability(homeLambda, goals)).ToArray();
        var away = Enumerable.Range(0, maxGoals + 1).Select(goals => PoissonProbability(awayLambda, goals)).ToArray();
        for (var h = 0; h <= maxGoals; h++)
        {
            for (var a = 0; a <= maxGoals; a++)
            {
                matrix[h, a] = home[h] * away[a];
            }
        }

        return matrix;
    }

    private static double SumOutcomes(double[,] matrix, Func<int, int, bool> predicate)
    {
        var total = 0d;
        for (var h = 0; h < matrix.GetLength(0); h++)
        {
            for (var a = 0; a < matrix.GetLength(1); a++)
            {
                if (predicate(h, a))
                {
                    total += matrix[h, a];
                }
            }
        }

        return total;
    }

    private static IReadOnlyList<(string Id, string Label, double Share)> BuildScorerShares(TeamRuntime team)
    {
        var shares = team.AvailableNamedScorers
            .Select(player => (player.Id, player.DisplayName, Weight: player.LiveInvolvement))
            .ToList();
        var genericWeight = team.GenericProfile.StarterInvolvement + team.NamedPlayers.Count(player => !player.Available && player.Role != PlayerRole.Keeper) * team.GenericProfile.SubstituteInvolvement;
        shares.Add(("OTHER", "Other Player", genericWeight));
        var total = shares.Sum(item => item.Weight);
        return shares.Select(item => (item.Id, item.DisplayName, item.Weight / Math.Max(1d, total))).ToList();
    }

    private static IReadOnlyList<(string Id, string Label, double FairProbability)> CollapseDuplicateOutcomes(IReadOnlyList<(string Id, string Label, double FairProbability)> outcomes)
    {
        return outcomes
            .GroupBy(item => $"{item.Id}|{item.Label}", StringComparer.Ordinal)
            .Select(group =>
            {
                var first = group.First();
                return (first.Id, first.Label, group.Sum(item => item.FairProbability));
            })
            .ToList();
    }

    private static double PoissonProbability(double lambda, int goals)
    {
        if (goals == 0)
        {
            return Math.Exp(-lambda);
        }

        return Math.Exp(-lambda) * Math.Pow(lambda, goals) / Factorial(goals);
    }

    private static double Factorial(int value)
    {
        var total = 1d;
        for (var i = 2; i <= value; i++)
        {
            total *= i;
        }

        return total;
    }

    private MarketQuote BuildHandicapBook(double line, HandicapTally tally, int simulationCount)
    {
        var targetReturn = 1d / _config.BookmakerOverround;
        var homeWin = tally.HomeWin / (double)simulationCount;
        var homePush = tally.HomePush / (double)simulationCount;
        var awayWin = tally.AwayWin / (double)simulationCount;
        var awayPush = tally.AwayPush / (double)simulationCount;
        var lineLabel = line.ToString("0.0", CultureInfo.InvariantCulture);
        return new MarketQuote(
            MarketKind.AsianHandicap,
            $"AH:{lineLabel}",
            $"Asian Handicap {lineLabel}",
            true,
            false,
            new List<OutcomeQuote>
            {
                new($"H:{lineLabel}", $"Home {lineLabel}", homeWin, (targetReturn - homePush) / Math.Max(0.0001d, homeWin), false, homePush),
                new($"A:{(-line).ToString("0.0", CultureInfo.InvariantCulture)}", $"Away {(-line).ToString("0.0", CultureInfo.InvariantCulture)}", awayWin, (targetReturn - awayPush) / Math.Max(0.0001d, awayWin), false, awayPush),
            });
    }

    private MarketQuote BuildClosedBook(MarketKind kind, string marketId, string displayName, IReadOnlyList<(string Id, string Label, double FairProbability)> outcomes)
    {
        var quotes = outcomes
            .Where(item => item.FairProbability > 0d || item.Id is "AOS" or "NOGOAL" or "OTHER")
            .Select(item =>
            {
                var implied = item.FairProbability * _config.BookmakerOverround;
                return new OutcomeQuote(item.Id, item.Label, item.FairProbability, implied <= 0d ? 0d : 1d / implied, true);
            })
            .ToList();
        return new MarketQuote(kind, marketId, displayName, false, true, quotes);
    }

    private static string LabelScorerOutcome(string id, ResolvedFixture fixture)
    {
        return id switch
        {
            "OTHER" => "Other Player",
            "NOGOAL" => "No Goalscorer",
            _ => fixture.Home.NamedPlayers.Concat(fixture.Away.NamedPlayers).Single(player => player.Id == id).DisplayName,
        };
    }

    private static void AccumulateHandicap(HandicapTally tally, double line, MatchResult result)
    {
        var homeAdjusted = result.FullTimeHomeGoals + line - result.FullTimeAwayGoals;
        if (homeAdjusted > 0d)
        {
            tally.HomeWin++;
        }
        else if (Math.Abs(homeAdjusted) < 0.000001d)
        {
            tally.HomePush++;
            tally.AwayPush++;
        }
        else
        {
            tally.AwayWin++;
        }
    }

    private static BetSelection? FindBestSelection(MarketQuote publicMarket, MarketQuote trueMarket)
    {
        BetSelection? best = null;
        foreach (var outcome in publicMarket.Outcomes)
        {
            var trueOutcome = trueMarket.Outcomes.SingleOrDefault(item => item.OutcomeId == outcome.OutcomeId);
            if (trueOutcome is null)
            {
                continue;
            }

            var edge = (trueOutcome.FairProbability * outcome.Odds) - 1d;
            var selection = new BetSelection(publicMarket.Kind, publicMarket.MarketId, outcome.OutcomeId, outcome.DisplayName, outcome.Odds, trueOutcome.FairProbability, edge);
            if (best is null || selection.Edge > best.Edge)
            {
                best = selection;
            }
        }

        return best;
    }

    private IReadOnlyList<string> GetEligibleScorerOutcomes(ResolvedFixture fixture)
    {
        var ids = fixture.Home.AvailableNamedScorers.Select(player => player.Id)
            .Concat(fixture.Away.AvailableNamedScorers.Select(player => player.Id))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        ids.Add("OTHER");
        ids.Add("NOGOAL");
        return ids;
    }

    private RoiMetric CalculateRoiMetric(IReadOnlyList<FixtureReturn> returns, int seed)
    {
        var profit = returns.Sum(item => item.Profit);
        var stakes = returns.Sum(item => item.Stakes);
        var roi = stakes == 0d ? 0d : profit / stakes;
        var interval = BootstrapInterval(returns, seed, static sample =>
        {
            var totalStake = sample.Sum(item => item.Stakes);
            return totalStake == 0d ? 0d : sample.Sum(item => item.Profit) / totalStake;
        });
        return new RoiMetric(profit, stakes, roi, interval);
    }

    private WinRateMetric CalculateWinRateMetric(IReadOnlyList<FixtureReturn> returns, int seed)
    {
        var wins = returns.Sum(item => item.Wins);
        var bets = returns.Sum(item => item.Bets);
        var winRate = bets == 0 ? 0d : wins / (double)bets;
        var interval = BootstrapInterval(returns, seed, static sample =>
        {
            var betCount = sample.Sum(item => item.Bets);
            return betCount == 0 ? 0d : sample.Sum(item => item.Wins) / (double)betCount;
        });
        return new WinRateMetric(wins, bets, winRate, interval, false);
    }

    private static ConfidenceInterval BootstrapInterval(IReadOnlyList<FixtureReturn> returns, int seed, Func<IReadOnlyList<FixtureReturn>, double> metric)
    {
        var rng = new Random(seed);
        var values = new double[250];
        for (var i = 0; i < values.Length; i++)
        {
            var sample = new List<FixtureReturn>(returns.Count);
            for (var j = 0; j < returns.Count; j++)
            {
                sample.Add(returns[rng.Next(returns.Count)]);
            }

            values[i] = metric(sample);
        }

        Array.Sort(values);
        return new ConfidenceInterval(values[(int)(values.Length * 0.025d)], values[(int)(values.Length * 0.975d)]);
    }

    private int DrawFactorCount(Random rng)
    {
        var roll = rng.NextDouble();
        var cumulative = 0d;
        for (var i = 0; i < _config.FactorCountWeights.Count; i++)
        {
            cumulative += _config.FactorCountWeights[i];
            if (roll <= cumulative)
            {
                return i;
            }
        }

        return _config.FactorCountWeights.Count - 1;
    }

    private bool TryApplyFactor(ResolvedFixture fixture, string factorName, Random rng, ISet<string> blockedTeams, ISet<string> blockedPlayers, out AppliedFactor factor)
    {
        factor = default!;
        if (factorName is "waterlogged pitch" or "high wind" or "derby" or "dead rubber")
        {
            ApplyMatchFactor(fixture, factorName);
            factor = BuildAppliedFactor(factorName, null, null);
            return true;
        }

        var candidateTeams = new[] { fixture.Home, fixture.Away }.Where(team => !blockedTeams.Contains(team.Source.Name)).ToList();
        if (candidateTeams.Count == 0)
        {
            return false;
        }

        var team = candidateTeams[rng.Next(candidateTeams.Count)];
        PlayerRuntime? player = null;

        switch (factorName)
        {
            case "striker knock":
                player = team.GetPrimaryStriker();
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                ApplyMinus(player, TeamRuntime.GetMagnitude(_config, FactorTier.Minor), modifyFinishing: true, modifyInvolvement: true);
                break;
            case "striker ruled out":
                player = team.GetPrimaryStriker();
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                team.MarkUnavailable(player);
                break;
            case "keeper hungover":
                player = team.GetKeeper();
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                ApplyMinus(player, TeamRuntime.GetMagnitude(_config, FactorTier.Moderate), modifyReliability: true);
                break;
            case "keeper elite form":
                player = team.GetKeeper();
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                ApplyPlus(player, TeamRuntime.GetMagnitude(_config, FactorTier.Moderate), modifyReliability: true);
                break;
            case "playmaker suspended":
                player = team.GetPlaymaker();
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                team.MarkUnavailable(player);
                break;
            case "hot streak":
                player = team.GetFactorTargetPlayer(rng);
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                player.ConditionMod *= 1d + TeamRuntime.GetMagnitude(_config, FactorTier.Minor);
                break;
            case "cold streak":
                player = team.GetFactorTargetPlayer(rng);
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                player.ConditionMod *= 1d - TeamRuntime.GetMagnitude(_config, FactorTier.Minor);
                break;
            case "training bust-up":
                player = team.GetFactorTargetPlayer(rng);
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                player.ConditionMod *= 1d - TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                break;
            case "model pro":
                player = team.GetFactorTargetPlayer(rng);
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                player.ConditionMod *= 1d + TeamRuntime.GetMagnitude(_config, FactorTier.Minor);
                break;
            case "winning morale":
                team.TeamAtkModifier *= 1d + TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                team.TeamDefModifier *= 1d + TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                break;
            case "losing crisis":
                team.TeamAtkModifier *= 1d - TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                team.TeamDefModifier *= 1d - TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                break;
            case "cup rotation":
                player = team.GetPrimaryStriker();
                if (player is null || blockedPlayers.Contains(player.Id))
                {
                    return false;
                }

                team.MarkUnavailable(player);
                break;
            case "surprise formation":
                team.Formation = Formations.Where(item => item != team.Source.BaseFormation).OrderBy(_ => rng.Next()).First();
                break;
            case "new-manager bounce":
                team.TeamAtkModifier *= 1d + TeamRuntime.GetMagnitude(_config, FactorTier.Minor);
                team.TeamDefModifier *= 1d + TeamRuntime.GetMagnitude(_config, FactorTier.Minor);
                break;
            case "pay dispute":
                team.TeamAtkModifier *= 1d - TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                team.TeamDefModifier *= 1d - TeamRuntime.GetMagnitude(_config, FactorTier.Moderate);
                break;
            default:
                return false;
        }

        blockedTeams.Add(team.Source.Name);
        if (player is not null)
        {
            blockedPlayers.Add(player.Id);
        }

        factor = BuildAppliedFactor(factorName, team, player);
        return true;
    }

    private static void ApplyMatchFactor(ResolvedFixture fixture, string factorName)
    {
        switch (factorName)
        {
            case "waterlogged pitch":
                fixture.Home.MatchShotModifier *= 0.78d;
                fixture.Away.MatchShotModifier *= 0.78d;
                fixture.Home.MatchConversionModifier *= 0.78d;
                fixture.Away.MatchConversionModifier *= 0.78d;
                break;
            case "high wind":
                fixture.Home.MatchOnTargetModifier *= 0.92d;
                fixture.Away.MatchOnTargetModifier *= 0.92d;
                break;
            case "derby":
            case "dead rubber":
                fixture.Home.MatchAttackModifier *= 0.9d;
                fixture.Away.MatchAttackModifier *= 0.9d;
                break;
        }
    }

    private AppliedFactor BuildAppliedFactor(string factorName, TeamRuntime? team, PlayerRuntime? player)
    {
        var tier = factorName switch
        {
            "striker knock" or "hot streak" or "cold streak" or "model pro" or "new-manager bounce" or "high wind" => FactorTier.Minor,
            "striker ruled out" or "cup rotation" or "waterlogged pitch" => FactorTier.Major,
            _ => FactorTier.Moderate,
        };
        var kind = factorName switch
        {
            "striker ruled out" or "playmaker suspended" or "cup rotation" => FactorKind.Availability,
            "waterlogged pitch" or "high wind" or "derby" or "dead rubber" => FactorKind.MatchCondition,
            _ => FactorKind.StatModifier,
        };
        var scope = kind == FactorKind.MatchCondition ? "match" : player is not null ? "player" : "team";
        return new AppliedFactor(factorName, tier, kind, scope, team?.Source.Name, player?.Id, factorName);
    }

    private static void ApplyMinus(PlayerRuntime player, double magnitude, bool modifyFinishing = false, bool modifyInvolvement = false, bool modifyReliability = false)
    {
        if (modifyFinishing)
        {
            player.Finishing *= 1d - magnitude;
        }

        if (modifyInvolvement)
        {
            player.Involvement *= 1d - magnitude;
        }

        if (modifyReliability)
        {
            player.Reliability *= 1d - magnitude;
        }
    }

    private static void ApplyPlus(PlayerRuntime player, double magnitude, bool modifyFinishing = false, bool modifyInvolvement = false, bool modifyReliability = false)
    {
        if (modifyFinishing)
        {
            player.Finishing *= 1d + magnitude;
        }

        if (modifyInvolvement)
        {
            player.Involvement *= 1d + magnitude;
        }

        if (modifyReliability)
        {
            player.Reliability *= 1d + magnitude;
        }
    }

    private static SettlementResult EvaluateOneXTwo(string outcomeId, MatchResult result)
    {
        return outcomeId switch
        {
            "H" when result.Outcome == MatchOutcome.HomeWin => SettlementResult.Win,
            "D" when result.Outcome == MatchOutcome.Draw => SettlementResult.Win,
            "A" when result.Outcome == MatchOutcome.AwayWin => SettlementResult.Win,
            _ => SettlementResult.Lose,
        };
    }

    private static SettlementResult EvaluateOverUnder(string outcomeId, MatchResult result)
    {
        var parts = outcomeId.Split(':');
        var line = double.Parse(parts[1], CultureInfo.InvariantCulture);
        var total = result.FullTimeHomeGoals + result.FullTimeAwayGoals;
        return parts[0] switch
        {
            "O" when total > line => SettlementResult.Win,
            "U" when total < line => SettlementResult.Win,
            _ => SettlementResult.Lose,
        };
    }

    private static SettlementResult EvaluateCorrectScore(string outcomeId, MatchResult result)
    {
        if (outcomeId == "AOS")
        {
            return result.FullTimeHomeGoals > 6 || result.FullTimeAwayGoals > 6 ? SettlementResult.Win : SettlementResult.Lose;
        }

        var parts = outcomeId.Split('-');
        return int.Parse(parts[0], CultureInfo.InvariantCulture) == result.FullTimeHomeGoals &&
               int.Parse(parts[1], CultureInfo.InvariantCulture) == result.FullTimeAwayGoals
            ? SettlementResult.Win
            : SettlementResult.Lose;
    }

    private static SettlementResult EvaluateHtFt(string outcomeId, MatchResult result)
    {
        var parts = outcomeId.Split('/');
        return EncodeMatchOutcome(result.HalfTimeHomeGoals, result.HalfTimeAwayGoals) == parts[0] &&
               EncodeMatchOutcome(result.FullTimeHomeGoals, result.FullTimeAwayGoals) == parts[1]
            ? SettlementResult.Win
            : SettlementResult.Lose;
    }

    private static SettlementResult EvaluateBtts(string outcomeId, MatchResult result)
    {
        var yes = result.FullTimeHomeGoals > 0 && result.FullTimeAwayGoals > 0;
        return (outcomeId == "Y" && yes) || (outcomeId == "N" && !yes) ? SettlementResult.Win : SettlementResult.Lose;
    }

    private static SettlementResult EvaluateHandicap(string outcomeId, MatchResult result)
    {
        var parts = outcomeId.Split(':');
        var line = double.Parse(parts[1], CultureInfo.InvariantCulture);
        var adjusted = parts[0] == "H"
            ? result.FullTimeHomeGoals + line - result.FullTimeAwayGoals
            : result.FullTimeAwayGoals + line - result.FullTimeHomeGoals;

        if (adjusted > 0d)
        {
            return SettlementResult.Win;
        }

        if (Math.Abs(adjusted) < 0.000001d)
        {
            return SettlementResult.Push;
        }

        return SettlementResult.Lose;
    }

    private static SettlementResult EvaluateFirstGoalscorer(string outcomeId, MatchResult result)
    {
        if (result.Goals.Count == 0)
        {
            return outcomeId == "NOGOAL" ? SettlementResult.Win : SettlementResult.Lose;
        }

        var first = result.Goals.OrderBy(goal => goal.Minute).First();
        if (outcomeId == "OTHER")
        {
            return first.IsOtherPlayer ? SettlementResult.Win : SettlementResult.Lose;
        }

        return first.ScorerId == outcomeId ? SettlementResult.Win : SettlementResult.Lose;
    }

    private static SettlementResult EvaluateAnytimeGoalscorer(string outcomeId, MatchResult result)
    {
        if (result.Goals.Count == 0)
        {
            return outcomeId == "NOGOAL" ? SettlementResult.Win : SettlementResult.Lose;
        }

        if (outcomeId == "OTHER")
        {
            return result.Goals.Any(goal => goal.IsOtherPlayer) ? SettlementResult.Win : SettlementResult.Lose;
        }

        return result.Goals.Any(goal => goal.ScorerId == outcomeId) ? SettlementResult.Win : SettlementResult.Lose;
    }

    private static string EncodeMatchOutcome(int homeGoals, int awayGoals)
    {
        if (homeGoals > awayGoals)
        {
            return "H";
        }

        if (homeGoals < awayGoals)
        {
            return "A";
        }

        return "D";
    }

    private static MatchOutcome GetOutcome(int homeGoals, int awayGoals)
    {
        if (homeGoals > awayGoals)
        {
            return MatchOutcome.HomeWin;
        }

        if (homeGoals < awayGoals)
        {
            return MatchOutcome.AwayWin;
        }

        return MatchOutcome.Draw;
    }
}

public sealed class ResolvedFixture
{
    public ResolvedFixture(TeamRuntime home, TeamRuntime away)
    {
        Home = home;
        Away = away;
    }

    public TeamRuntime Home { get; }

    public TeamRuntime Away { get; }

    public List<AppliedFactor> Factors { get; } = [];

    public string Signature => $"{Home.Signature}|{Away.Signature}|{string.Join(';', Factors.Select(f => $"{f.Name}:{f.TeamName}:{f.PlayerId}"))}";
}

internal sealed record HalfState(
    double ShotsExpectation,
    double CornersExpectation,
    double OnTargetBase,
    double ConversionBase,
    double AssistChance);

internal sealed record MatchSimulation(
    int HomeGoals,
    int AwayGoals,
    int HomeHalfTimeGoals,
    int AwayHalfTimeGoals,
    int HomeShots,
    int AwayShots,
    int HomeShotsOnTarget,
    int AwayShotsOnTarget,
    int HomeCorners,
    int AwayCorners,
    int HomeSaves,
    int AwaySaves,
    IReadOnlyList<GoalEvent> Goals,
    IReadOnlyList<CommentaryLine> Commentary);

internal sealed class HandicapTally
{
    public int HomeWin { get; set; }

    public int HomePush { get; set; }

    public int AwayWin { get; set; }

    public int AwayPush { get; set; }
}

internal sealed record FixtureReturn(double Profit, double Stakes)
{
    public FixtureReturn(double profit, double stakes, int wins, int bets)
        : this(profit, stakes)
    {
        Wins = wins;
        Bets = bets;
    }

    public int Wins { get; init; }

    public int Bets { get; init; }
}
