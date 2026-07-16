using BallKnowledge.MatchEngine;
using Xunit;
using Engine = BallKnowledge.MatchEngine.MatchEngine;

namespace BallKnowledge.Tests;

public sealed class EngineTests
{
    private readonly EngineConfig _config = TestData.LoadConfig();
    private readonly IReadOnlyList<TeamDefinition> _teams = TestData.LoadTeams();

    [Fact]
    public void ValidationHarnessRunsWithoutCrashing()
    {
        // Regression: min_offered_odds curation can leave a public-board market with no
        // true-board counterpart (factors shift which handicap lines are offered). The
        // harness must reconcile the two boards without throwing.
        var fastConfig = _config with { ValidationFixtureCount = 120, PricingSimCount = 60 };
        var engine = new Engine(fastConfig);

        var report = engine.RunValidation(_teams, seed: 999);

        Assert.Equal(120, report.FixtureCount);
    }

    [Fact]
    public void PoissonSamplerTracksMean()
    {
        var rng = new Random(7);
        const double lambda = 3.4;
        var total = 0;
        const int draws = 20000;
        for (var i = 0; i < draws; i++)
        {
            total += Engine.SamplePoisson(lambda, rng);
        }

        var mean = total / (double)draws;
        Assert.InRange(mean, 3.2, 3.6);
    }

    [Fact]
    public void SeededMatchRespectsChainInvariants()
    {
        var engine = new Engine(_config);
        var match = engine.SimulateMatch(_teams[0], _teams[1], 12345);

        Assert.True(match.HomeStats.Goals <= match.HomeStats.ShotsOnTarget);
        Assert.True(match.HomeStats.ShotsOnTarget <= match.HomeStats.Shots);
        Assert.True(match.AwayStats.Goals <= match.AwayStats.ShotsOnTarget);
        Assert.True(match.AwayStats.ShotsOnTarget <= match.AwayStats.Shots);
        Assert.All(match.Goals, goal => Assert.False(string.IsNullOrWhiteSpace(goal.ScorerId)));
    }

    [Fact]
    public void AvailabilityKeepsXiAtElevenAndVoidsPlayerFromScorerBook()
    {
        var engine = new Engine(_config);
        var resolved = engine.ResolveFixture(_teams[0], _teams[1], 2233, includeHiddenFactors: true);
        Assert.Equal(11, resolved.Home.ToDebugState().StartingXiCount);
        Assert.Equal(11, resolved.Away.ToDebugState().StartingXiCount);

        var unavailable = resolved.Home.NamedPlayers.Concat(resolved.Away.NamedPlayers).Where(player => !player.Available).ToList();
        if (unavailable.Count > 0)
        {
            var board = engine.PriceTrueMarkets(resolved);
            var firstScorer = board.Markets.Single(m => m.Kind == MarketKind.FirstGoalscorer);
            foreach (var player in unavailable)
            {
                Assert.DoesNotContain(firstScorer.Outcomes, outcome => outcome.OutcomeId == player.Id);
            }
        }
    }

    [Fact]
    public void ClosedBooksSumToOverround()
    {
        var engine = new Engine(_config);
        var board = engine.PricePublicMarkets(_teams[0], _teams[1]);
        foreach (var market in board.Markets.Where(m => m.IsClosedBook && !m.IsPushCapable))
        {
            var sum = market.Outcomes.Sum(outcome => outcome.Odds <= 0d ? 0d : 1d / outcome.Odds);
            Assert.InRange(sum, _config.BookmakerOverround - 0.02d, _config.BookmakerOverround + 0.02d);
        }
    }

    [Fact]
    public void HandicapBookMatchesEvMargin()
    {
        var engine = new Engine(_config);
        var board = engine.PricePublicMarkets(_teams[0], _teams[1]);
        foreach (var handicap in board.Markets.Where(m => m.Kind == MarketKind.AsianHandicap))
        {
            foreach (var outcome in handicap.Outcomes)
            {
                var target = 1d / _config.BookmakerOverround;
                Assert.InRange((outcome.FairProbability * outcome.Odds) + outcome.PushProbability, target - 0.02d, target + 0.02d);
            }
        }
    }

    [Fact]
    public void FactorSamplingRespectsCapAndNoStack()
    {
        var engine = new Engine(_config);
        for (var seed = 0; seed < 200; seed++)
        {
            var factors = engine.SampleFactors(_teams[0], _teams[1], seed);
            Assert.True(factors.Count <= _config.MaxFactorsPerMatch);
            Assert.Equal(factors.Count, factors.Select(item => item.Name).Distinct().Count());
            Assert.Equal(factors.Where(item => item.TeamName is not null).Count(), factors.Where(item => item.TeamName is not null).Select(item => item.TeamName).Distinct().Count());
            Assert.Equal(factors.Where(item => item.PlayerId is not null).Count(), factors.Where(item => item.PlayerId is not null).Select(item => item.PlayerId).Distinct().Count());
        }
    }

    [Fact]
    public void HtFtStateIsComponentwiseConsistent()
    {
        var engine = new Engine(_config);
        var match = engine.SimulateMatch(_teams[2], _teams[3], 4040);
        Assert.True(match.HalfTimeHomeGoals <= match.FullTimeHomeGoals);
        Assert.True(match.HalfTimeAwayGoals <= match.FullTimeAwayGoals);
    }

    [Fact]
    public void CommentaryChecklistPassesForSeededMatch()
    {
        var engine = new Engine(_config);
        MatchResult? match = null;
        for (var seed = 1; seed < 2000; seed++)
        {
            var candidate = engine.SimulateMatch(_teams[0], _teams[1], seed);
            if (candidate.ActiveFactors.Count > 0 && candidate.Goals.Any(goal => goal.Minute >= 80))
            {
                match = candidate;
                break;
            }
        }

        Assert.NotNull(match);
        Assert.All(match!.ActiveFactors, factor => Assert.Contains(match.Commentary, line => line.EventType == CommentaryEventType.FactorLeak && line.Text.Contains(factor.TeamName ?? string.Empty, StringComparison.OrdinalIgnoreCase) || line.Text.Contains(factor.LeakToken, StringComparison.OrdinalIgnoreCase)));
        Assert.Contains(match.Commentary, line => line.EventType == CommentaryEventType.Goal && line.Minute >= 80 && line.Text.Contains("late", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(match.Commentary, line => line.EventType == CommentaryEventType.HalfTime && line.Text.Contains(match.HalfTimeHomeGoals.ToString()) && line.Text.Contains(match.HalfTimeAwayGoals.ToString()));
        Assert.Contains(match.Commentary, line => line.EventType == CommentaryEventType.FullTime && line.Text.Contains(match.FullTimeHomeGoals.ToString()) && line.Text.Contains(match.FullTimeAwayGoals.ToString()));
        Assert.DoesNotContain(match.Commentary, line => line.Text.Contains('{') || line.Text.Contains('}'));
    }
}
