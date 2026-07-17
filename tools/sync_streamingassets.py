from __future__ import annotations

"""Sync design JSON files into Unity StreamingAssets or check for drift."""

import argparse
import shutil
import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[1]
DESIGN_DIR = REPO_ROOT / "design"
STREAMING_ASSETS_DIR = REPO_ROOT / "unity" / "Assets" / "StreamingAssets"
SOURCE_FILES = [
    DESIGN_DIR / "constants.json",
    DESIGN_DIR / "teams.json",
    DESIGN_DIR / "greybox.json",
]


def display_path(path: Path) -> str:
    return str(path.relative_to(REPO_ROOT)).replace("\\", "/")


def ensure_source_files_exist() -> None:
    for source in SOURCE_FILES:
        if not source.is_file():
            raise ValueError(f"File not found: {source}")


def files_match(source: Path, dest: Path) -> bool:
    return dest.is_file() and source.read_bytes() == dest.read_bytes()


def sync_files() -> int:
    ensure_source_files_exist()
    STREAMING_ASSETS_DIR.mkdir(parents=True, exist_ok=True)

    for source in SOURCE_FILES:
        dest = STREAMING_ASSETS_DIR / source.name
        shutil.copyfile(source, dest)
        print(f"{display_path(source)} -> {display_path(dest)}")

    return 0


def check_files() -> int:
    ensure_source_files_exist()

    mismatches: list[str] = []
    for source in SOURCE_FILES:
        dest = STREAMING_ASSETS_DIR / source.name
        if not dest.exists():
            mismatches.append(f"{display_path(dest)}: missing")
        elif not files_match(source, dest):
            mismatches.append(f"{display_path(dest)}: differs from {display_path(source)}")

    if mismatches:
        print("StreamingAssets sync check failed:")
        for mismatch in mismatches:
            print(f"- {mismatch}")
        return 1

    print("StreamingAssets are in sync.")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description="Copy frozen design JSON files into unity/Assets/StreamingAssets or check for drift.")
    parser.add_argument(
        "--check",
        action="store_true",
        help="Verify that StreamingAssets files exist and match design/ byte-for-byte without copying.",
    )
    args = parser.parse_args()

    try:
        if args.check:
            return check_files()
        return sync_files()
    except ValueError as exc:
        print(f"Sync failed: {exc}")
        return 1


if __name__ == "__main__":
    sys.exit(main())
