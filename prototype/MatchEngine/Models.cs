using System.Text.Json.Serialization;

namespace BallKnowledge.MatchEngine;

public enum PlayerRole
{
    Attacker,
    Playmaker,
    Keeper,
}

public enum FactorTier
{
    Minor,
    Moderate,
    Major,
}

public enum FactorKind
{
    StatModifier,
    Availability,
    MatchCondition,
}

public enum MatchOutcome
{
    HomeWin,
    Draw,
    AwayWin,
}

public enum CommentaryEventType
{
    Kickoff,
    FactorLeak,
    Goal,
    Save,
    Chance,
    Corner,
    HalfTime,
    FullTime,
}

public enum MarketKind
{
    OneXTwo,
    OverUnder,
    CorrectScore,
    HalfTimeFullTime,
    BothTeamsToScore,
    AsianHandicap,
    FirstGoalscorer,
    AnytimeGoalscorer,
}

public enum SettlementResult
{
    Win,
    Lose,
    Push,
}

public sealed record FormationMods
{
    [JsonPropertyName("atkMult")]
    public required double AtkMult { get; init; }

    [JsonPropertyName("defMult")]
    public required double DefMult { get; init; }

    [JsonPropertyName("shotMult")]
    public required double ShotMult { get; init; }

    [JsonPropertyName("cornerMult")]
    public required double CornerMult { get; init; }

    [JsonPropertyName("passMult")]
    public required double PassMult { get; init; }
}

public sealed record FactorTierMagnitudes
{
    [JsonPropertyName("minor")]
    public required double Minor { get; init; }

    [JsonPropertyName("moderate")]
    public required double Moderate { get; init; }

    [JsonPropertyName("major")]
    public required double Major { get; init; }

    public double GetMagnitude(FactorTier tier) => tier switch
    {
        FactorTier.Minor => Minor,
        FactorTier.Moderate => Moderate,
        FactorTier.Major => Major,
        _ => Minor,
    };
}

public sealed record EngineConfig
{
    [JsonPropertyName("schema_version")]
    public required int SchemaVersion { get; init; }

    [JsonPropertyName("starting_debt")]
    public required int StartingDebt { get; init; }

    [JsonPropertyName("weekly_debt_interest")]
    public required double WeeklyDebtInterest { get; init; }

    [JsonPropertyName("catch_vig")]
    public required double CatchVig { get; init; }

    [JsonPropertyName("bookmaker_overround")]
    public required double BookmakerOverround { get; init; }

    [JsonPropertyName("shot_base")]
    public required double ShotBase { get; init; }

    [JsonPropertyName("on_target_base")]
    public required double OnTargetBase { get; init; }

    [JsonPropertyName("conversion_base")]
    public required double ConversionBase { get; init; }

    [JsonPropertyName("corner_base")]
    public required double CornerBase { get; init; }

    [JsonPropertyName("assist_rate")]
    public required double AssistRate { get; init; }

    [JsonPropertyName("second_half_factor")]
    public required double SecondHalfFactor { get; init; }

    [JsonPropertyName("home_advantage")]
    public required double HomeAdvantage { get; init; }

    [JsonPropertyName("formation_mods")]
    public required IReadOnlyDictionary<string, FormationMods> FormationMods { get; init; }

    [JsonPropertyName("factor_tier_magnitudes")]
    public required FactorTierMagnitudes FactorTierMagnitudes { get; init; }

    [JsonPropertyName("max_factors_per_match")]
    public required int MaxFactorsPerMatch { get; init; }

    [JsonPropertyName("factor_count_weights")]
    public required IReadOnlyList<double> FactorCountWeights { get; init; }

    [JsonPropertyName("factor_rarity")]
    public required IReadOnlyDictionary<string, double> FactorRarity { get; init; }

    [JsonPropertyName("pricing_sim_count")]
    public required int PricingSimCount { get; init; }

    [JsonPropertyName("correct_score_cap")]
    public required int CorrectScoreCap { get; init; }

    [JsonPropertyName("over_under_lines")]
    public required IReadOnlyList<double> OverUnderLines { get; init; }

    [JsonPropertyName("handicap_lines")]
    public required IReadOnlyList<double> HandicapLines { get; init; }

    [JsonPropertyName("validation_fixture_count")]
    public required int ValidationFixtureCount { get; init; }

    [JsonPropertyName("edge_threshold")]
    public required double EdgeThreshold { get; init; }

    [JsonPropertyName("blind_roi")]
    public required double BlindRoi { get; init; }

    [JsonPropertyName("blind_roi_band")]
    public required IReadOnlyList<double> BlindRoiBand { get; init; }

    [JsonPropertyName("informed_roi_min")]
    public required double InformedRoiMin { get; init; }

    [JsonPropertyName("even_money_odds_range")]
    public required IReadOnlyList<double> EvenMoneyOddsRange { get; init; }

    [JsonPropertyName("informed_win_rate_min")]
    public required double InformedWinRateMin { get; init; }

    [JsonPropertyName("informed_win_rate_max")]
    public required double InformedWinRateMax { get; init; }

    [JsonPropertyName("min_even_money_bets")]
    public required int MinEvenMoneyBets { get; init; }
}

public sealed record TeamDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("nameZh")]
    public required string NameZh { get; init; }

    [JsonPropertyName("ATK")]
    public required int Atk { get; init; }

    [JsonPropertyName("DEF")]
    public required int Def { get; init; }

    [JsonPropertyName("height")]
    public required int Height { get; init; }

    [JsonPropertyName("baseFormation")]
    public required string BaseFormation { get; init; }

    [JsonPropertyName("teamForm")]
    public required double TeamForm { get; init; }

    [JsonPropertyName("namedPlayers")]
    public required IReadOnlyList<PlayerDefinition> NamedPlayers { get; init; }
}

public sealed record PlayerDefinition
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("nameZh")]
    public required string NameZh { get; init; }

    [JsonPropertyName("role")]
    public required PlayerRole Role { get; init; }

    [JsonPropertyName("finishing")]
    public int? Finishing { get; init; }

    [JsonPropertyName("involvement")]
    public int? Involvement { get; init; }

    [JsonPropertyName("passing")]
    public int? Passing { get; init; }

    [JsonPropertyName("reliability")]
    public int? Reliability { get; init; }

    [JsonPropertyName("conditionFlavor")]
    public required string ConditionFlavor { get; init; }
}

public sealed record CommentaryLine(int Minute, CommentaryEventType EventType, string Text);

public sealed record GoalEvent(
    int Minute,
    bool IsHomeTeam,
    string ScorerId,
    string ScorerName,
    bool IsOtherPlayer,
    string? AssistId,
    string? AssistName);

public sealed record TeamMatchStats(
    string TeamName,
    int Goals,
    int HalfTimeGoals,
    int Shots,
    int ShotsOnTarget,
    int Corners,
    int Saves);

public sealed record TeamDebugState(
    string TeamName,
    string Formation,
    int StartingXiCount,
    IReadOnlyList<string> AvailableNamedPlayerIds,
    IReadOnlyList<string> UnavailableNamedPlayerIds,
    IReadOnlyList<string> EligibleScorerIds,
    bool HasOtherPlayerOutcome);

public sealed record AppliedFactor(
    string Name,
    FactorTier Tier,
    FactorKind Kind,
    string Scope,
    string? TeamName,
    string? PlayerId,
    string LeakToken);

public sealed record MatchResult(
    int Seed,
    TeamDefinition HomeTeam,
    TeamDefinition AwayTeam,
    TeamMatchStats HomeStats,
    TeamMatchStats AwayStats,
    MatchOutcome Outcome,
    IReadOnlyList<GoalEvent> Goals,
    IReadOnlyList<AppliedFactor> ActiveFactors,
    IReadOnlyList<CommentaryLine> Commentary,
    TeamDebugState HomeDebug,
    TeamDebugState AwayDebug)
{
    public int HalfTimeHomeGoals => HomeStats.HalfTimeGoals;

    public int HalfTimeAwayGoals => AwayStats.HalfTimeGoals;

    public int FullTimeHomeGoals => HomeStats.Goals;

    public int FullTimeAwayGoals => AwayStats.Goals;
}

public sealed record OutcomeQuote(
    string OutcomeId,
    string DisplayName,
    double FairProbability,
    double Odds,
    bool IsClosedBookOutcome,
    double PushProbability = 0d);

public sealed record MarketQuote(
    MarketKind Kind,
    string MarketId,
    string DisplayName,
    bool IsPushCapable,
    bool IsClosedBook,
    IReadOnlyList<OutcomeQuote> Outcomes);

public sealed record MarketCatalogue(IReadOnlyList<MarketQuote> Markets);

public sealed record BetSelection(
    MarketKind Kind,
    string MarketId,
    string OutcomeId,
    string DisplayName,
    double Odds,
    double TrueProbability,
    double Edge);

public sealed record ConfidenceInterval(double Lower, double Upper);

public sealed record RoiMetric(double Profit, double Stakes, double Roi, ConfidenceInterval Interval);

public sealed record WinRateMetric(int Wins, int Bets, double WinRate, ConfidenceInterval Interval, bool IsGateable);

public sealed record ValidationReport(
    int FixtureCount,
    RoiMetric Blind,
    RoiMetric Informed,
    WinRateMetric EvenMoneySubset,
    bool BlindGatePassed,
    bool InformedGatePassed,
    bool EvenMoneyGatePassed,
    bool Gate1Passed,
    IReadOnlyList<string> Notes);

public sealed record AccumulatorLeg(string Label, double Odds, bool Won);

public sealed record AccumulatorQuote(double Odds, bool Won);
