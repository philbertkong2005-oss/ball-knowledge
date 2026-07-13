from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator


REPO_ROOT = Path(__file__).resolve().parents[1]
DEFAULT_CONSTANTS_PATH = REPO_ROOT / "design" / "constants.json"
SCHEMA_PATH = REPO_ROOT / "design" / "constants.schema.json"


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

    formatted: list[str] = []
    for part in parts:
        if isinstance(part, int):
            formatted.append(f"[{part}]")
        else:
            formatted.append(str(part))

    dotted = ".".join(piece for piece in formatted if not piece.startswith("["))
    indexed = "".join(piece for piece in formatted if piece.startswith("["))
    return f"{dotted}{indexed}" if dotted else indexed


def collect_schema_errors(instance: Any, schema: Any) -> list[str]:
    validator = Draft202012Validator(schema)
    errors = []
    for error in sorted(validator.iter_errors(instance), key=lambda item: list(item.path)):
        path = format_path(list(error.path))
        errors.append(f"{path}: {error.message}")
    return errors


def collect_sane_range_errors(data: dict[str, Any]) -> list[str]:
    errors: list[str] = []

    def ensure(condition: bool, key: str, message: str) -> None:
        if not condition:
            errors.append(f"{key}: {message}")

    ensure(0 < data["weekly_debt_interest"] < 1, "weekly_debt_interest", "must be a decimal fraction between 0 and 1.")
    ensure(0 <= data["catch_vig"] <= 1, "catch_vig", "must be a decimal fraction between 0 and 1.")
    ensure(1 <= data["bookmaker_overround"] <= 2, "bookmaker_overround", "must be a multiplier between 1 and 2.")
    ensure(0 < data["league_avg_goals"] < 10, "league_avg_goals", "must be a positive goals value less than 10.")
    ensure(-5 < data["home_advantage"] < 5, "home_advantage", "must stay within a sane expected-goals range.")

    targets = data["validation_targets"]
    ensure(-1 <= targets["blind_roi"] <= 1, "validation_targets.blind_roi", "must be a decimal fraction between -1 and 1.")
    ensure(0 <= targets["informed_win_rate_min"] <= 1, "validation_targets.informed_win_rate_min", "must be a decimal fraction between 0 and 1.")
    ensure(0 <= targets["informed_win_rate_max"] <= 1, "validation_targets.informed_win_rate_max", "must be a decimal fraction between 0 and 1.")
    ensure(
        targets["informed_win_rate_min"] <= targets["informed_win_rate_max"],
        "validation_targets",
        "informed_win_rate_min must be less than or equal to informed_win_rate_max.",
    )

    return errors


def validate(constants_path: Path) -> list[str]:
    schema = load_json(SCHEMA_PATH)
    data = load_json(constants_path)

    schema_errors = collect_schema_errors(data, schema)
    if schema_errors:
        return schema_errors

    return collect_sane_range_errors(data)


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate Ball Knowledge design/constants.json against schema and sane ranges.")
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
