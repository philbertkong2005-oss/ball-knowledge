# Ball Knowledge

Working title: **Ball Knowledge**. 中文《走數》.

This repository is the Phase 0 pre-production scaffold for the game: docs, validated tuning constants, local tooling, CI, and the starter folder layout for the future Unity project.

## Repo Documents

- [PLAN.md](./PLAN.md) - frozen implementation spec for this scaffold
- [docs/design-doc.md](./docs/design-doc.md) - design document heading skeleton only; design content stays authored separately
- [docs/workflow.md](./docs/workflow.md) - the future Codex working contract, verification rules, rollback paths, and secrets policy
- [docs/unity-setup.md](./docs/unity-setup.md) - beginner Unity Hub and Unity project setup instructions
- [design/constants.schema.json](./design/constants.schema.json) - JSON Schema for the tuning constants
- [design/constants.json](./design/constants.json) - authoring source for Phase 0 design constants
- [design/constants-guide.md](./design/constants-guide.md) - plain-language guide for each constants key

## Tool Versions

- Python used for the verified local `.venv`: `3.12.10`
- Expected host Python location from the Phase 0 spec: `C:\Users\philb\AppData\Local\Programs\Python\Python312\python.exe`
- Project virtual environment: `.venv`
- Python package pins used for verification: `jsonschema 4.26.0`, `pre-commit 4.6.0`
- Python requirement constraints in version control: `jsonschema >=4,<5`, `pre-commit >=4,<5`
- Unity Editor: `6000.3.19f1` (Unity 6.3 LTS); the authoritative editor pin is the committed `unity/ProjectSettings/ProjectVersion.txt`
- Unity template requirement: built-in `3D` template only, not `Universal 3D` or any URP template

## Constants Workflow

`design/constants.json` is the single authoring source in Phase 0. Unity must not read the repo-root file directly at runtime. The planned convention is to sync it into `unity/Assets/StreamingAssets/` in Phase 2.

## Next Steps (Human)

- Run `gh auth login`, create a private GitHub repository named `ball-knowledge`, then add the remote. Browser fallback: create a private repo named `ball-knowledge` on `github.com`, then run `git remote add origin <repo-url>`.
- Install Unity Hub and Unity 6 LTS by following [docs/unity-setup.md](./docs/unity-setup.md).
- Make the first commit and push manually after local checks pass. This stays a human-only step under the repo secrets policy.
