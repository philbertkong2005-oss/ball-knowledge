from __future__ import annotations

"""Validate Ball Knowledge design/greybox.json against schema and sane ranges."""

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator


REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_GREYBOX_PATH = REPO_ROOT / "design" / "greybox.json"
SCHEMA_PATH = REPO_ROOT / "design" / "greybox.schema.json"


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


def collect_provenance_errors(data: dict[str, Any]) -> list[str]:
    """world-design.md §10: every value is doc-cited, TUNE, SCAFFOLD, or verified —
    enforced in BOTH directions so unmarked scaffolding cannot become the design."""
    errors: list[str] = []

    provenance = data.get("_provenance", {})
    leaf_paths = {
        f"{group}.{key}"
        for group, values in data.items()
        if group not in ("schema_version", "_provenance") and isinstance(values, dict)
        for key in values
    }

    for path in sorted(leaf_paths - set(provenance)):
        errors.append(f"_provenance: '{path}' has no provenance entry (doc citation, TUNE, SCAFFOLD, or verified).")
    for path in sorted(set(provenance) - leaf_paths):
        errors.append(f"_provenance: entry '{path}' does not match any value in the file.")

    return errors


def collect_sane_range_errors(data: dict[str, Any]) -> list[str]:
    errors: list[str] = []

    def ensure(condition: bool, key: str, message: str) -> None:
        if not condition:
            errors.append(f"{key}: {message}")

    player = data["player"]
    detection = data["detection"]
    cash_heat = data["cash_heat"]
    catch_stop = data["catch_stop"]
    economy = data["economy"]
    clock = data["clock"]

    ensure(player["walk_speed_ms"] > 0, "player.walk_speed_ms", "must be positive.")
    ensure(player["run_speed_ms"] > 0, "player.run_speed_ms", "must be positive.")
    ensure(player["crouch_speed_ms"] > 0, "player.crouch_speed_ms", "must be positive.")
    ensure(
        player["crouch_speed_ms"] < player["walk_speed_ms"] < player["run_speed_ms"],
        "player",
        "must satisfy crouch_speed_ms < walk_speed_ms < run_speed_ms.",
    )
    ensure(player["mouse_sensitivity"] > 0, "player.mouse_sensitivity", "must be positive.")

    ensure(
        0 < detection["vision_half_angle_deg"] < 90,
        "detection.vision_half_angle_deg",
        "must be between 0 and 90 degrees.",
    )
    ensure(detection["vision_range_m"] > 0, "detection.vision_range_m", "must be positive.")
    ensure(detection["base_detect_time_s"] > 0, "detection.base_detect_time_s", "must be positive.")
    ensure(
        0 < detection["min_detect_time_s"] <= detection["base_detect_time_s"],
        "detection.min_detect_time_s",
        "must be positive and less than or equal to base_detect_time_s.",
    )
    ensure(
        detection["detect_curve_exponent"] > 0,
        "detection.detect_curve_exponent",
        "must be positive (1.0 = linear).",
    )
    ensure(
        detection["chase_speed_mult"] >= 1,
        "detection.chase_speed_mult",
        "must be at least 1.",
    )
    ensure(
        detection["search_scan_duration_s"] > 0,
        "detection.search_scan_duration_s",
        "must be positive.",
    )

    ensure(cash_heat["carry_threshold"] >= 0, "cash_heat.carry_threshold", "must be at least 0.")
    ensure(
        cash_heat["heat_per_hour_over_threshold"] > 0,
        "cash_heat.heat_per_hour_over_threshold",
        "must be positive.",
    )
    ensure(
        cash_heat["heat_decay_per_hour_below_threshold"] > 0,
        "cash_heat.heat_decay_per_hour_below_threshold",
        "must be positive.",
    )
    ensure(cash_heat["max_heat"] > 0, "cash_heat.max_heat", "must be positive.")

    ensure(
        catch_stop["fine_per_heat_point"] >= 0,
        "catch_stop.fine_per_heat_point",
        "must be at least 0.",
    )
    ensure(
        catch_stop["bribe_fine_multiplier"] >= 1,
        "catch_stop.bribe_fine_multiplier",
        "must be at least 1 (wd:§6.4 prices the bribe well above the fine).",
    )

    ensure(economy["debt_start"] > 0, "economy.debt_start", "must be positive.")
    ensure(economy["canned_bet_stake"] > 0, "economy.canned_bet_stake", "must be positive.")
    ensure(
        economy["canned_bet_payout"] > economy["canned_bet_stake"],
        "economy.canned_bet_payout",
        "must be greater than canned_bet_stake.",
    )
    ensure(economy["canned_bet_home_team"] >= 0, "economy.canned_bet_home_team", "must be at least 0.")
    ensure(economy["canned_bet_away_team"] >= 0, "economy.canned_bet_away_team", "must be at least 0.")
    ensure(
        economy["canned_bet_home_team"] != economy["canned_bet_away_team"],
        "economy",
        "must use different canned_bet_home_team and canned_bet_away_team indices.",
    )

    ensure(
        clock["real_minutes_per_game_day"] > 0,
        "clock.real_minutes_per_game_day",
        "must be positive.",
    )
    ensure(0 <= clock["night_start_hour"] <= 23, "clock.night_start_hour", "must be in 0..23.")
    ensure(0 <= clock["night_end_hour"] <= 23, "clock.night_end_hour", "must be in 0..23.")

    return errors


def validate(greybox_path: Path) -> list[str]:
    schema = load_json(SCHEMA_PATH)
    data = load_json(greybox_path)

    schema_errors = collect_schema_errors(data, schema)
    if schema_errors:
        return schema_errors

    return collect_provenance_errors(data) + collect_sane_range_errors(data)


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Ball Knowledge design/greybox.json against schema v1 and sane ranges.")
    parser.add_argument(
        "greybox_path",
        nargs="?",
        default=str(DEFAULT_GREYBOX_PATH),
        help="Path to a greybox JSON file. Defaults to design/greybox.json.",
    )
    args = parser.parse_args()

    greybox_path = Path(args.greybox_path).resolve()

    try:
        errors = validate(greybox_path)
    except ValueError as exc:
        print(f"Validation failed: {exc}")
        return 1

    if errors:
        print(f"Validation failed for {greybox_path}:")
        for error in errors:
            print(f"- {error}")
        return 1

    print(f"Validation passed for {greybox_path}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
