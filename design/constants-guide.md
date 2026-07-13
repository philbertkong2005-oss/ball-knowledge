# Constants Guide

- `schema_version`: File-format version for `design/constants.json`; change this only when the structure changes.
- `starting_debt`: Starting debt in in-game dollars; use a whole number such as `500000`.
- `weekly_debt_interest`: Weekly debt interest as a decimal fraction, so `0.10` means `10%`, not `10`.
- `catch_vig`: Collector vig as a decimal fraction, so `0.30` means `30%`, not `30`.
- `bookmaker_overround`: Bookmaker overround as a multiplier, so `1.10` means a `110%` book, not `10%`.
- `league_avg_goals`: League average goals **per team** per match (the Poisson baseline), so `1.35` means each team scores about 1.35 goals on average — roughly `2.7` total goals per match.
- `home_advantage`: Home advantage in expected-goals units, written as a decimal number, not a percentage.
- `validation_targets.blind_roi`: Expected blind betting ROI as a decimal fraction, so `-0.08` means `-8%`.
- `validation_targets.informed_win_rate_min`: Lower bound for informed win rate as a decimal fraction, so `0.55` means `55%`.
- `validation_targets.informed_win_rate_max`: Upper bound for informed win rate as a decimal fraction, so `0.60` means `60%`.
