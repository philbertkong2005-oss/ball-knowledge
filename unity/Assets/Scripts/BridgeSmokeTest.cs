using System;
using System.Globalization;
using BallKnowledge.MatchEngine;
using UnityEngine;

namespace BallKnowledge.Greybox
{
    public sealed class BridgeSmokeTest : MonoBehaviour
    {
        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private void Start()
        {
            try
            {
                var loaded = GreyboxConfigLoader.LoadFrom(Application.streamingAssetsPath);
                var engineConfig = loaded.EngineConfig;
                var teams = loaded.Teams;
                var greybox = loaded.Greybox;

                if (!NearlyEqual(engineConfig.ConversionBase, 0.205) ||
                    !NearlyEqual(engineConfig.HomeAdvantage, 1.24) ||
                    engineConfig.FormationMods == null ||
                    engineConfig.FormationMods.Count != 10)
                {
                    Debug.LogError(string.Format(
                        InvariantCulture,
                        "[BallKnowledge] Config mismatch — conversion_base={0} home_advantage={1} formations={2} expected=0.205/1.24/10",
                        FormatNumber(engineConfig.ConversionBase),
                        FormatNumber(engineConfig.HomeAdvantage),
                        engineConfig.FormationMods == null ? 0 : engineConfig.FormationMods.Count));
                    return;
                }

                Debug.Log(string.Format(
                    InvariantCulture,
                    "[BallKnowledge] Config OK — conversion_base={0} home_advantage={1} formations={2}",
                    FormatNumber(engineConfig.ConversionBase),
                    FormatNumber(engineConfig.HomeAdvantage),
                    engineConfig.FormationMods.Count));

                if (teams == null || teams.Count != 8 || !string.Equals(teams[0].Name, "Harbour FC", StringComparison.Ordinal))
                {
                    Debug.LogError(string.Format(
                        InvariantCulture,
                        "[BallKnowledge] Teams mismatch — count={0} first={1} expected=8/Harbour FC",
                        teams == null ? 0 : teams.Count,
                        teams != null && teams.Count > 0 ? teams[0].Name : "<none>"));
                    return;
                }

                Debug.Log(string.Format(
                    InvariantCulture,
                    "[BallKnowledge] Teams OK — {0} teams, first={1}",
                    teams.Count,
                    teams[0].Name));

                if (!NearlyEqual(greybox.Player.WalkSpeedMs, 2.5) ||
                    !NearlyEqual(greybox.Player.RunSpeedMs, 5.0) ||
                    greybox.CashHeat.CarryThreshold != 500 ||
                    greybox.Economy.DebtStart != 100000)
                {
                    Debug.LogError(string.Format(
                        InvariantCulture,
                        "[BallKnowledge] Greybox mismatch — walk={0} sprint={1} carry_threshold={2} debt={3} expected=2.5/5/500/100000",
                        FormatNumber(greybox.Player.WalkSpeedMs),
                        FormatNumber(greybox.Player.RunSpeedMs),
                        greybox.CashHeat.CarryThreshold,
                        greybox.Economy.DebtStart));
                    return;
                }

                Debug.Log(string.Format(
                    InvariantCulture,
                    "[BallKnowledge] Greybox OK — walk={0} sprint={1} carry_threshold={2} debt={3}",
                    FormatNumber(greybox.Player.WalkSpeedMs),
                    FormatNumber(greybox.Player.RunSpeedMs),
                    greybox.CashHeat.CarryThreshold,
                    greybox.Economy.DebtStart));

                var economy = greybox.Economy;
                ValidateTeamIndex(economy.CannedBetHomeTeam, teams.Count, "home");
                ValidateTeamIndex(economy.CannedBetAwayTeam, teams.Count, "away");

                if (!string.Equals(economy.CannedBetOutcome, "home", StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[BallKnowledge] Canned bet mismatch — greybox.json outcome must be 'home' for the Gate-2 fixture.");
                    return;
                }

                var homeTeam = teams[economy.CannedBetHomeTeam];
                var awayTeam = teams[economy.CannedBetAwayTeam];
                var engine = new BallKnowledge.MatchEngine.MatchEngine(engineConfig);
                var result = engine.SimulateMatch(homeTeam, awayTeam, economy.CannedBetSeed);

                if (result.FullTimeHomeGoals != 2 ||
                    result.FullTimeAwayGoals != 0 ||
                    result.Outcome != MatchOutcome.HomeWin)
                {
                    Debug.LogError(string.Format(
                        InvariantCulture,
                        "[BallKnowledge] Canned bet mismatch — seed={0} produced {1} {2}-{3} {4} ({5}), expected 2-0 home win",
                        economy.CannedBetSeed,
                        homeTeam.Name,
                        result.FullTimeHomeGoals,
                        result.FullTimeAwayGoals,
                        awayTeam.Name,
                        DescribeOutcome(result.Outcome)));
                    return;
                }

                Debug.Log(string.Format(
                    InvariantCulture,
                    "[BallKnowledge] Canned bet seed={0} -> {1} {2}-{3} {4}  [EXPECTED: 2-0 home win]",
                    economy.CannedBetSeed,
                    homeTeam.Name,
                    result.FullTimeHomeGoals,
                    result.FullTimeAwayGoals,
                    awayTeam.Name));
                Debug.Log("[BallKnowledge] BRIDGE SMOKE TEST PASSED");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        private static string DescribeOutcome(MatchOutcome outcome)
        {
            switch (outcome)
            {
                case MatchOutcome.HomeWin:
                    return "home win";
                case MatchOutcome.Draw:
                    return "draw";
                case MatchOutcome.AwayWin:
                    return "away win";
                default:
                    return outcome.ToString();
            }
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.###", InvariantCulture);
        }

        private static bool NearlyEqual(double left, double right)
        {
            return Math.Abs(left - right) < 0.0001d;
        }

        private static void ValidateTeamIndex(int teamIndex, int teamCount, string label)
        {
            if (teamIndex >= 0 && teamIndex < teamCount)
            {
                return;
            }

            throw new IndexOutOfRangeException(
                "greybox.json canned_bet_" + label + "_team index " + teamIndex + " is outside the loaded team list (count=" + teamCount + ").");
        }
    }
}
