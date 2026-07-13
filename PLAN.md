# Plan: Phase 0 — Pre-production for "Ball Knowledge"
_Locked via grill — by Claude + Phil. Revised after Codex review Rounds 1–2._

## Goal

Stand up everything needed before any gameplay work on **Ball Knowledge** (working title; 中文《走數》), a 90s underground football-betting sim: a versioned project repo, the merged design doc v2, a single validated tuning-constants file, a working Unity shell, and — most critically — an AI-development workflow with guardrails, because the sole developer is a **beginner who cannot read code**, working **5–15 hrs/week**, with **OpenAI Codex as the coding agent**. Phase 0 succeeds when the developer can verify gameplay/UI changes by *playing and observing*, verify non-visual changes via required command logs and artifacts, and roll back any bad change using a tested, safe recovery procedure.

## Approach

1. **Preflight checklist (before anything else).** Verify `git --version`, `gh --version`, `gh auth status`, and Git identity (`git config user.name` / `user.email`); written fix-it steps for each failure, plus a browser-based fallback for creating the GitHub repo if `gh` cannot be made to work.
2. **Create the repo.** New folder `C:\Users\philb\Projects\ball-knowledge`; `git init`; create a **private** GitHub repository and push the first commit. From day one include: the **official GitHub Unity `.gitignore` template verbatim** (`github/gitignore` → `Unity.gitignore`, adjusted only for the `unity/` subfolder path) plus `.env` and secrets patterns, `.gitattributes` and `.editorconfig` locking **UTF-8 encoding and normalized line endings** for docs/JSON (the docs contain Chinese text; encoding must be controlled from the first commit).
3. **Repo skeleton.** Directories: `docs/` (design docs), `design/` (data tables & constants), `prototype/` (Phase 1 Python match engine — empty for now), `unity/` (Unity project, created in step 7), plus `README.md` naming the game and linking the docs.
4. **Design doc v2** at `docs/design-doc.md` — **English body with a short 中文 summary per section**. Merges every locked decision: three-act structure; design pillars (every bet is walked / information is currency / debt is the clock / nothing buys better luck); homeless-shelter start and housing-ladder trade-off (shelter = safe from collectors but curfew/no stash; own place = stash but address known); catch/vig rule (confiscate all cash, 70% to debt, 30% vig); energy-as-daily-budget with food folded in; far training ground vs pub-night intel collision; relationship NPCs (ball kid, shelter mate) carrying to Acts 2–3; in-fiction promo tokens (amplify reward, never probability; no real-money purchases ever); global event-broadcasting calendar; Poisson match engine summary with odds built from stale public info + ~110% overround.
5. **Constants: schema + file + validation.**
   - `design/constants.schema.json` — JSON Schema defining every key with explicit **types and unit conventions** (all rates as decimal fractions: vig `0.30` not `30`; weekly interest `0.10`; overround as multiplier `1.10`), plus a required `schema_version` field.
   - `design/constants.json` — the single authoring source. Initial values: `schema_version: 1`; starting debt `500000` (integer, in-game dollars); weekly debt interest `0.10`; catch vig `0.30`; bookmaker overround `1.10`; league average goals `1.35`; home advantage `0.25`; validation targets (blind ROI ≈ `-0.08`; informed win rate `0.55–0.60`).
   - `design/constants-guide.md` — one plain-language line per key (JSON cannot hold comments).
   - `tools/validate_constants.py` — a friendly validator (checks JSON parses, matches schema, values in sane ranges; prints human-readable errors).
   - **Python dependency manifest:** `requirements-dev.txt` (e.g. `jsonschema`, `pre-commit`) committed to the repo; local setup and CI both install from it explicitly — no bare imports on a fresh machine.
   - **Hook installation is explicit, not aspirational:** use the `pre-commit` framework with a committed `.pre-commit-config.yaml`; setup docs include the one-command install (`pre-commit install`), and "hook fires on a junk edit" is an exit criterion.
   - **Unity consumption convention (documented now, implemented in Phase 2):** `design/constants.json` is the authoring source only; a sync step copies it into `unity/Assets/StreamingAssets/` at build/import time. Repo-root JSON is never read directly by Unity at runtime.
6. **AI-workflow guardrails** at `docs/workflow.md` — the contract for every future Codex session:
   - (a) **Verification is tiered:** gameplay/UI changes are verified by playing and observing; non-visual changes (tooling, serialization, data, build) must end with a command the developer runs and a log/artifact whose expected output is stated in advance.
   - (b) Codex writes automated tests alongside every feature; the test run is part of "done".
   - (c) **Commit discipline:** one feature per commit; separate mechanical commits allowed for Unity/package churn when they are not the feature; every verified state gets a git tag (`good-YYYYMMDD-HHMM`) as a stable rollback anchor.
   - (d) **Safe rollback cheat-sheet with separate recovery paths:** unpushed local mistakes → `git restore` / `git reset --hard <tag>`; **pushed mistakes → `git revert <sha>` (default)**; history rewrites (`push --force-with-lease`) documented as expert-only, not part of the beginner path.
   - (e) **Secrets policy:** no real secrets in the repo or in prompts to any AI; `.env` is gitignored; `git push` is always a manual human step after local checks pass.
   - (f) Session sizing: tasks sized to 1–2 hour sessions to fit the 5–15 hr/week budget.
7. **Tool installation (the developer's GUI hands).** Install Unity Hub and an exact Unity 6 LTS editor version; **the authoritative editor-version pin is the committed `unity/ProjectSettings/ProjectVersion.txt`** (README records it too; `unity/Packages/manifest.json` pins packages, not the editor). Create the project from the **lightest built-in 3D template** (no URP — render pipeline decided at Phase 2 when art needs are known). Install **Python 3.12+**, create a project `.venv`, install `requirements-dev.txt`, run `pre-commit install`, and run one checked-in smoke command (`python tools/validate_constants.py`) from inside the venv. Written step-by-step instructions for every GUI step, since Codex cannot click the Unity editor.
8. **CI (minimal but real).** GitHub Actions workflow on every push: install from `requirements-dev.txt`, then run `tools/validate_constants.py` against schema + any Python tests present. Unity batch-mode CI is **deferred to Phase 2** (first Unity code) — license/runner setup cost outweighs benefit while `unity/` contains an empty template.
9. **Verification & exit criteria.** Phase 0 is done when ALL of: preflight checklist passes; the private GitHub repo shows the pushed skeleton with `.gitattributes`/`.editorconfig`/`.gitignore`; design doc, workflow doc, constants schema + file + guide, `requirements-dev.txt`, and `.pre-commit-config.yaml` exist and are committed; the pre-commit hook demonstrably blocks a junk constants edit; CI runs green on a push; the Unity editor version pinned in committed `ProjectVersion.txt` opens the empty project and plays a blank scene without errors; the `.venv` smoke command succeeds; and the developer has personally executed one full **rollback drill for BOTH paths** (unpushed junk edit → reset to tag; pushed junk commit → revert). Target: **two weekends (~2 calendar weeks at 5–15 hrs/week).**

## Key decisions & tradeoffs

- **Solo beginner + Codex-as-coder** is the governing constraint. Guardrails (schema-validated data, pre-commit hook, tiered verification, tagged rollback anchors, CI) trade speed for safety, since the developer cannot review diffs.
- **Unity over Godot** — the developer's explicit choice, accepted with the named consequence: the developer performs GUI steps by instruction; mitigation is keeping game logic in code/data and scenes minimal. Revisit at the Phase 2 gate only if editor friction proves blocking. (Claude recommended Godot 4; user preference won.)
- **Pinned Unity version + lightest 3D template, no URP** — kills version ambiguity and package churn; render pipeline is a Phase 2 decision.
- **Private GitHub from day 1**, browser fallback if `gh` fails — protects a year of work from disk failure.
- **English-body docs with 中文 summaries** — single source of truth optimized for Codex, skimmable for the developer; UTF-8 locked via `.gitattributes`/`.editorconfig` to prevent mojibake on Windows.
- **JSON constants + JSON Schema + validator + guide doc** — machine validation replaces the false promise that a non-coder can edit JSON "safely" unaided; decimal-fraction unit convention prevents `30` vs `0.30` drift between Python and C#.
- **Authoring-source convention for Unity** — repo-root JSON never read at runtime; synced into `StreamingAssets` (implemented Phase 2, documented now).
- **CI scope: constants + Python only in Phase 0; Unity CI deferred** — accepted the reviewer's CI principle, rejected the Unity batch-mode half for now: setup cost for a beginner (Unity license on runners, build minutes) far exceeds its value while the Unity project is an empty template.
- **Working title locked: "Ball Knowledge"** — football slang for deep understanding of the game, matching the core pillar; repo named `ball-knowledge`.
- **Timeline honesty:** at 5–15 hrs/week, all original roadmap durations roughly double; Phase 0 itself is sized at ~2 weeks.

## Risks / open questions

- **Unity ↔ AI-CLI friction:** scene wiring and package installs need human GUI work; if this becomes a recurring blocker, revisit the deferred Godot option at the Phase 2 gate (before art/level investment makes switching expensive).
- **No human code review:** tests + tiered verification reduce but do not eliminate the risk of subtly wrong logic (especially betting math). Phase 1's 1,000-bet validation harness is the designed backstop; until then this risk is accepted and named.
- **Codex cost/availability:** the workflow assumes ongoing Codex access; no fallback coder exists.
- **Unity asset bloat:** large binary assets may later need Git LFS; deferred — noted in README, decide when the first big asset arrives (Phase 4).

## Out of scope

- All gameplay code, including the Phase 1 match engine (next phase, separate plan).
- Art, audio, Steam page, marketing, localization.
- Acts 2–3 design elaboration (stays on paper per the roadmap's PM rules).
- Any real-money monetization design (permanently out).
- Unity batch-mode CI (Phase 2), StreamingAssets sync implementation (Phase 2), Git LFS (Phase 4).
