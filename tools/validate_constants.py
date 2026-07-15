from __future__ import annotations

import argparse
import json
import math
import sys
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator


REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CONSTANTS_PATH = REPO_ROOT / "design" / "constants.json"
SCHEMA_PATH = REPO_ROOT / "design" / "constants.schema.json"
FORMATION_KEYS = [
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
]
FACTOR_KEYS = [
    "striker knock",
    "striker ruled out",
    "keeper hungover",
    "keeper elite form",
    "playmaker suspended",
    "hot streak",
    "cold streak",
    "training bust-up",
    "model pro",
    "winning morale",
    "losing crisis",
    "cup rotation",
    "surprise formation",
    "new-manager bounce",
    "pay dispute",
    "waterlogged pitch",
    "high wind",
    "derby",
    "dead rubber",
]


def load_json(path: Path) -> Any:
    try:
        with path.open("r", encoding="utf-8-sig") as handle:
            return json.load(handle)
    except FileNotFoundError as exc:
        raise ValueError(f"File not found: {path}") from exc
    except json.JSONDecodeError as exc:
        location = f"line {exc.lineno}, column {exc.colno}"
        raise ValueError(f"Invalid JSON in {path} at {location}: {exc.msg}") from exc


def format_path(parts: list[Any]) -> str:
    if not parts:
        return "<root>"

    path = ""
    for part in parts:
        if isinstance(part, int):
            path += f"[{part}]"
        elif path:
            path += f".{part}"
        else:
            path = str(part)
    return path


def collect_schema_errors(instance: Any, schema: Any) -> list[str]:
    validator = Draft202012Validator(schema)
    errors = []
    for error in sorted(validator.iter_errors(instance), key=lambda item: list(item.path)):
        path = format_path(list(error.path))
        errors.append(f"{path}: {error.message}")
    return errors


def is_half_or_whole(value: float) -> bool:
    return math.isclose(value * 2, round(value * 2), abs_tol=1e-9)


def collect_sane_range_errors(data: dict[str, Any]) -> list[str]:
    errors: list[str] = []

    def ensure(condition: bool, key: str, message: str) -> None:
        if not condition:
            errors.append(f"{key}: {message}")

    ensure(0 < data["weekly_debt_interest"] < 1, "weekly_debt_interest", "must be a decimal fraction between 0 and 1.")
    ensure(0 <= data["catch_vig"] <= 1, "catch_vig", "must be a decimal fraction between 0 and 1.")
    ensure(1 <= data["bookmaker_overround"] <= 2, "bookmaker_overround", "must be a multiplier between 1 and 2.")
    ensure(1 <= data["shot_base"] <= 40, "shot_base", "must be a sensible shots-per-team baseline.")
    ensure(0 < data["on_target_base"] < 1, "on_target_base", "must be a decimal fraction between 0 and 1.")
    ensure(0 < data["conversion_base"] < 1, "conversion_base", "must be a decimal fraction between 0 and 1.")
    ensure(0 < data["corner_base"] < 20, "corner_base", "must be a sensible corners-per-team baseline.")
    ensure(0 <= data["assist_rate"] <= 1, "assist_rate", "must be a decimal fraction between 0 and 1.")
    ensure(0.5 <= data["second_half_factor"] <= 2, "second_half_factor", "must stay within a sane half-weighting range.")
    ensure(0.5 <= data["home_advantage"] <= 1.5, "home_advantage", "must be a multiplier in a sane attacking range.")

    formation_mods = data["formation_mods"]
    ensure(sorted(formation_mods.keys()) == sorted(FORMATION_KEYS), "formation_mods", "must define exactly the 10 supported formations.")
    for formation, row in formation_mods.items():
        for key in ("atkMult", "defMult", "shotMult", "cornerMult", "passMult"):
            ensure(0.5 <= row[key] <= 1.5, f"formation_mods.{formation}.{key}", "must stay within a sane multiplier range.")

    tiers = data["factor_tier_magnitudes"]
    ensure(tiers["minor"] < tiers["moderate"] < tiers["major"], "factor_tier_magnitudes", "must increase from minor to moderate to major.")
    ensure(tiers["major"] < 0.5, "factor_tier_magnitudes.major", "must stay below 0.5.")

    weights = data["factor_count_weights"]
    ensure(len(weights) == data["max_factors_per_match"] + 1, "factor_count_weights", "length must equal max_factors_per_match + 1.")
    ensure(abs(sum(weights) - 1) < 1e-9, "factor_count_weights", "must sum to 1.")

    factor_rarity = data["factor_rarity"]
    ensure(sorted(factor_rarity.keys()) == sorted(FACTOR_KEYS), "factor_rarity", "must define every frozen hidden-factor key exactly once.")

    ensure(data["pricing_sim_count"] >= 300, "pricing_sim_count", "should be at least 300 for usable Monte-Carlo pricing.")
    ensure(data["correct_score_cap"] >= 1, "correct_score_cap", "must be at least 1.")
    ensure(data["validation_fixture_count"] >= 1000, "validation_fixture_count", "should be at least 1000.")
    ensure(-1 <= data["edge_threshold"] <= 1, "edge_threshold", "must stay within a sane EV range.")

    blind_roi_expected = round(1 / data["bookmaker_overround"] - 1, 4)
    ensure(abs(data["blind_roi"] - blind_roi_expected) <= 0.0001, "blind_roi", f"must match 1/bookmaker_overround - 1 (expected about {blind_roi_expected}).")

    blind_band = data["blind_roi_band"]
    ensure(blind_band[0] <= blind_band[1], "blind_roi_band", "must be ordered [lower, upper].")
    ensure(blind_band[0] <= data["blind_roi"] <= blind_band[1], "blind_roi_band", "must contain blind_roi.")

    even_money_range = data["even_money_odds_range"]
    ensure(even_money_range[0] <= even_money_range[1], "even_money_odds_range", "must be ordered [lower, upper].")
    ensure(even_money_range[0] > 1, "even_money_odds_range", "decimal odds must be greater than 1.")

    ensure(-1 <= data["informed_roi_min"] <= 1, "informed_roi_min", "must stay within a sane ROI range.")
    ensure(0 <= data["informed_win_rate_min"] <= 1, "informed_win_rate_min", "must be a decimal fraction between 0 and 1.")
    ensure(0 <= data["informed_win_rate_max"] <= 1, "informed_win_rate_max", "must be a decimal fraction between 0 and 1.")
    ensure(data["informed_win_rate_min"] <= data["informed_win_rate_max"], "informed_win_rate", "min must be less than or equal to max.")

    for index, line in enumerate(data["over_under_lines"]):
        ensure(is_half_or_whole(line), f"over_under_lines[{index}]", "must be a whole or half line.")

    for index, line in enumerate(data["handicap_lines"]):
        ensure(is_half_or_whole(line), f"handicap_lines[{index}]", "must be a whole or half line.")

    return errors


def validate(constants_path: Path) -> list[str]:
    schema = load_json(SCHEMA_PATH)
    data = load_json(constants_path)

    schema_errors = collect_schema_errors(data, schema)
    if schema_errors:
      return schema_errors

    return collect_sane_range_errors(data)


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Ball Knowledge design/constants.json against schema v2 and sane ranges.")
    parser.add_argument(
        "constants_path",
        nargs="?",
        default=str(DEFAULT_CONSTANTS_PATH),
        help="Path to a constants JSON file. Defaults to design/constants.json.",
    )
    args = parser.parse_args()

    constants_path = Path(args.constants_path).resolve()

    try:
        errors = validate(constants_path)
    except ValueError as exc:
        print(f"Validation failed: {exc}")
        return 1

    if errors:
        print(f"Validation failed for {constants_path}:")
        for error in errors:
            print(f"- {error}")
        return 1

    print(f"Validation passed for {constants_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
