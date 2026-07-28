using Newtonsoft.Json;

namespace BallKnowledge.Greybox
{
    public sealed class GreyboxSettings
    {
        [JsonProperty("schema_version")]
        public int SchemaVersion { get; set; }

        [JsonProperty("player")]
        public GreyboxPlayerSettings Player { get; set; } = new GreyboxPlayerSettings();

        [JsonProperty("detection")]
        public GreyboxDetectionSettings Detection { get; set; } = new GreyboxDetectionSettings();

        [JsonProperty("cash_heat")]
        public GreyboxCashHeatSettings CashHeat { get; set; } = new GreyboxCashHeatSettings();

        [JsonProperty("catch_stop")]
        public GreyboxCatchStopSettings CatchStop { get; set; } = new GreyboxCatchStopSettings();

        [JsonProperty("economy")]
        public GreyboxEconomySettings Economy { get; set; } = new GreyboxEconomySettings();

        [JsonProperty("clock")]
        public GreyboxClockSettings Clock { get; set; } = new GreyboxClockSettings();
    }

    public sealed class GreyboxPlayerSettings
    {
        [JsonProperty("walk_speed_ms")]
        public double WalkSpeedMs { get; set; }

        [JsonProperty("run_speed_ms")]
        public double RunSpeedMs { get; set; }

        [JsonProperty("crouch_speed_ms")]
        public double CrouchSpeedMs { get; set; }

        [JsonProperty("mouse_sensitivity")]
        public double MouseSensitivity { get; set; }
    }

    public sealed class GreyboxDetectionSettings
    {
        [JsonProperty("vision_half_angle_deg")]
        public double VisionHalfAngleDeg { get; set; }

        [JsonProperty("vision_range_m")]
        public double VisionRangeM { get; set; }

        [JsonProperty("base_detect_time_s")]
        public double BaseDetectTimeS { get; set; }

        [JsonProperty("min_detect_time_s")]
        public double MinDetectTimeS { get; set; }

        [JsonProperty("detect_curve_exponent")]
        public double DetectCurveExponent { get; set; }

        [JsonProperty("chase_speed_mult")]
        public double ChaseSpeedMult { get; set; }

        [JsonProperty("search_scan_duration_s")]
        public double SearchScanDurationS { get; set; }
    }

    public sealed class GreyboxCashHeatSettings
    {
        [JsonProperty("carry_threshold")]
        public int CarryThreshold { get; set; }

        [JsonProperty("heat_per_hour_over_threshold")]
        public double HeatPerHourOverThreshold { get; set; }

        [JsonProperty("heat_decay_per_hour_below_threshold")]
        public double HeatDecayPerHourBelowThreshold { get; set; }

        [JsonProperty("max_heat")]
        public double MaxHeat { get; set; }
    }

    public sealed class GreyboxCatchStopSettings
    {
        [JsonProperty("fine_per_heat_point")]
        public double FinePerHeatPoint { get; set; }

        [JsonProperty("bribe_fine_multiplier")]
        public double BribeFineMultiplier { get; set; }

        [JsonProperty("bribe_always_available")]
        public bool BribeAlwaysAvailable { get; set; }
    }

    public sealed class GreyboxEconomySettings
    {
        [JsonProperty("debt_start")]
        public int DebtStart { get; set; }

        [JsonProperty("canned_bet_stake")]
        public int CannedBetStake { get; set; }

        [JsonProperty("canned_bet_payout")]
        public int CannedBetPayout { get; set; }

        [JsonProperty("canned_bet_seed")]
        public int CannedBetSeed { get; set; }

        [JsonProperty("canned_bet_outcome")]
        public string CannedBetOutcome { get; set; } = string.Empty;

        [JsonProperty("canned_bet_home_team")]
        public int CannedBetHomeTeam { get; set; }

        [JsonProperty("canned_bet_away_team")]
        public int CannedBetAwayTeam { get; set; }
    }

    public sealed class GreyboxClockSettings
    {
        [JsonProperty("real_minutes_per_game_day")]
        public double RealMinutesPerGameDay { get; set; }

        [JsonProperty("night_start_hour")]
        public int NightStartHour { get; set; }

        [JsonProperty("night_end_hour")]
        public int NightEndHour { get; set; }
    }
}
