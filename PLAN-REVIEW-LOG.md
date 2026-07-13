# Plan Review Log: Phase 0 — Pre-production for "Ball Knowledge"
Act 1 (grill) complete — plan locked with the user. MAX_ROUNDS=5.

## Round 1 — Codex

**Findings**
1. `PLAN.md:14` The rollback cheat-sheet is unsafe and incomplete: `git reset --hard <last-good>` plus plain `git push` does not safely recover a bad commit already pushed to GitHub and will teach a non-coder to rewrite history blindly. Fix: document separate recovery paths and make `git revert <sha>` the default for pushed mistakes, with `git push --force-with-lease` only for deliberate history rewrites.

2. `PLAN.md:6` The promise that every future change can be verified only by “playing and observing” is false for toolchain, save-data, serialization, and build-pipeline changes, so the plan overstates how safe Codex-only development will be. Fix: scope play-verification to gameplay/UI behavior and require command logs or artifacts for non-visual changes.

3. `PLAN.md:13` `design/constants.json` has no schema, no version field, and no unit conventions, so `30` vs `0.30`, “10%/week” vs multiplier, and int/float handling will drift between Python and C#. Fix: define a JSON Schema with `schema_version`, explicit units, numeric types, and one canonical example file.

4. `PLAN.md:13` Saying a non-coder can edit JSON “safely” is wrong without machine validation; one missing comma or renamed key will break both runtimes and the guide doc will not catch it. Fix: add a single validation command plus CI/pre-commit that loads `constants.json` and prints friendly errors before commit.

5. `PLAN.md:13` “Game code must read `design/constants.json`” is a bad Unity assumption because repo-root JSON is not automatically a runtime asset in editor builds or exported builds, so the Unity and Python consumers will diverge fast. Fix: keep `design/constants.json` as the authoring source and add an explicit import/sync step into `unity/Assets/StreamingAssets` or a generated ScriptableObject.

6. `PLAN.md:15` `Unity 6 LTS` and “empty 3D (URP)” are both version-ambiguous and heavier than this phase needs, which adds package churn and editor friction for no gameplay benefit. Fix: pin an exact Unity editor version plus package manifest and use the lightest template that satisfies current needs unless URP is explicitly required.

7. `PLAN.md:14` Requiring tests for every feature without adding CI means “tests passed” depends on trusting Codex, which is exactly the trust problem this workflow is supposed to reduce. Fix: add GitHub Actions in Phase 0 to run Python tests and a Unity batch-mode smoke check on every push.

8. `PLAN.md:14` “One feature per commit, committed immediately when verified” is too fuzzy for Unity’s incidental file churn and gives the developer no stable rollback anchor beyond memory. Fix: tag every verified state (`good-YYYYMMDD-HHMM`) and allow separate mechanical Unity/package commits when they are not the feature itself.

9. `PLAN.md:10` Creating the repo via `gh` assumes `git`, `gh`, Git identity, and GitHub auth are already working, but the plan has no preflight or fallback path if any one of those steps fails. Fix: add a prerequisite checklist (`git --version`, `gh --version`, `gh auth status`, Git username/email) and a browser-based fallback for repo creation.

10. `PLAN.md:15` `python --version` as an exit criterion proves almost nothing; it does not verify the correct major/minor version, venv creation, or that the future prototype commands will run reproducibly. Fix: require creating a project `.venv` and running one checked-in smoke command from inside it.

11. `PLAN.md:12` The bilingual-doc requirement is risky on this Windows-first workflow because the current `PLAN.md` already renders mojibake in the terminal, which means encoding is not controlled and Chinese summaries can become unreadable or noisy in diffs. Fix: add `.gitattributes` and `.editorconfig` that lock UTF-8 and normalized line endings for docs/JSON from day one.

12. `PLAN.md:14` The workflow has no secrets policy even though it assumes ongoing Codex use and GitHub access, so the first API key or token added later can be committed or exposed by the agent. Fix: add `.env`/`.gitignore` rules, forbid real secrets in repo or prompts, and make `git push` a manual human step after local checks pass.

VERDICT: REVISE
### Claude's response (Round 1)

Accepted 11 of 12 findings and revised PLAN.md accordingly:
1. ✅ Rollback cheat-sheet rewritten with separate recovery paths — `git revert` default for pushed mistakes; `--force-with-lease` marked expert-only; exit criteria now require drilling BOTH paths.
2. ✅ Verification promise scoped: play-verification for gameplay/UI only; non-visual changes require pre-stated command logs/artifacts (tiered verification in workflow.md).
3. ✅ Added `design/constants.schema.json` with `schema_version`, explicit types, decimal-fraction unit convention (0.30 not 30; overround as 1.10 multiplier).
4. ✅ Added `tools/validate_constants.py` as pre-commit hook with friendly errors.
5. ✅ Documented authoring-source convention: repo JSON never read by Unity at runtime; synced to StreamingAssets (implementation deferred to Phase 2, convention locked now).
6. ✅ Pinned exact Unity editor version + committed manifest; dropped URP for lightest built-in 3D template; render pipeline deferred to Phase 2.
7. ⚠️ PARTIAL — Accepted GitHub Actions CI running constants validation + Python tests on every push. REJECTED Unity batch-mode CI in Phase 0: runner licensing/setup cost for a beginner far exceeds value while unity/ is an empty template. Explicitly scheduled for Phase 2 instead. Reason logged in plan.
8. ✅ Git tags (`good-YYYYMMDD-HHMM`) as rollback anchors; mechanical Unity/package commits allowed separately from feature commits.
9. ✅ Preflight checklist (git/gh/auth/identity) is now step 1, with browser fallback for repo creation.
10. ✅ Exit criterion upgraded from `python --version` to: project `.venv` + checked-in smoke command run from inside it.
11. ✅ `.gitattributes` + `.editorconfig` locking UTF-8/line endings from first commit (docs contain Chinese text).
12. ✅ Secrets policy added: `.env` gitignored, no secrets in repo or AI prompts, `git push` always manual-human.

## Round 2 — Codex

Most of the Round 1 issues are addressed. Specifically: the rollback path is now sane, verification is correctly split between visual and non-visual work, constants now have schema/version/unit conventions, JSON editing has a validator, the Unity runtime-read assumption is corrected, Unity/URP ambiguity is removed, commit/tag discipline is tighter, preflight checks exist, Python setup is no longer a fake smoke test, encoding is explicitly controlled, and the secrets policy is no longer missing. The partial rejection on Unity batch-mode CI is logged clearly in [PLAN-REVIEW-LOG.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN-REVIEW-LOG.md:1) and is acceptable for Phase 0 while `unity/` is still an empty template.

**New findings**
1. [PLAN.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN.md:27) The plan still does not actually pin the Unity editor version in-repo: `unity/Packages/manifest.json` pins packages, not the editor, so “exact Unity 6 LTS editor version” can still drift between machines. Fix: require committing `unity/ProjectSettings/ProjectVersion.txt` and treat that as the authoritative editor-version pin.

2. [PLAN.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN.md:18), [PLAN.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN.md:27), [PLAN.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN.md:28) The Python toolchain is underspecified: `tools/validate_constants.py` and CI exist, but there is no dependency manifest or install step, so the first `jsonschema` import or pre-commit helper will fail on a fresh machine. Fix: add `requirements-dev.txt` or `pyproject.toml` and make local setup plus CI install from it explicitly.

3. [PLAN.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN.md:18) “Wired as a pre-commit hook” is still aspirational because the plan never says how the hook is installed or verified, and Git hooks do not propagate automatically with the repo. Fix: add an explicit hook-install step and exit criterion, or use the `pre-commit` framework with a one-command install documented in setup.

4. [PLAN.md](C:\Users\philb\Downloads\proficiently-claude-skills-main\PLAN.md:11) “Unity-appropriate `.gitignore`” is still too vague for a beginner-plus-agent workflow; if Codex misses `Library/`, `Temp/`, or generated caches once, the repo gets polluted immediately. Fix: require the official GitHub Unity `.gitignore` template verbatim instead of leaving it to interpretation.

VERDICT: REVISE
### Claude's response (Round 2)

Accepted all 4 new findings:
1. ✅ Editor pin corrected: committed `unity/ProjectSettings/ProjectVersion.txt` is now the authoritative editor-version pin (manifest.json pins packages only).
2. ✅ Added `requirements-dev.txt` (jsonschema, pre-commit); local setup and CI both install from it explicitly.
3. ✅ Hook install made concrete: `pre-commit` framework with committed `.pre-commit-config.yaml`, documented `pre-commit install`, and "hook demonstrably blocks a junk constants edit" added to exit criteria.
4. ✅ `.gitignore` specified as the official GitHub Unity template verbatim (adjusted only for the unity/ subfolder path).

## Round 3 — Codex

The Round 2 findings are addressed. The plan now closes the concrete holes that previously mattered: rollback paths are separated and safe for a beginner, verification is correctly split between visual and non-visual work, constants have schema/version/unit rules plus machine validation, Unity runtime consumption is no longer based on a bad repo-root assumption, the Unity editor version is actually pinned via `ProjectVersion.txt`, Python dependencies and hook installation are explicit, the Unity `.gitignore` source is no longer vague, and the Phase 0 CI scope is narrow but defensible with the Unity batch-mode deferral clearly logged.

I do not see a new material flaw that is large enough to block implementation at Phase 0. There are still normal execution risks for a solo beginner workflow, but they are named, bounded, and matched with concrete guardrails rather than hand-waved.

VERDICT: APPROVED
---
Converged: APPROVED in Round 3 of MAX_ROUNDS=5. Awaiting user sign-off before any code.

## Act 3 — Build

### Round 1 — Codex build
Codex (gpt-5.4 high, thread 019f5a6e-1981-7b43-a142-4c1e5945fff1, sandboxed workspace-write) built the full Phase 0 scaffold at C:\Users\philb\Projects\ball-knowledge: 18 files (README, gitignore/gitattributes/editorconfig, design doc skeleton, workflow contract, unity setup guide, constants schema+file+guide, validator, requirements, pre-commit config, CI workflow, folder keeps). Its proof runs passed. Reported 3 sandbox-caused deviations honestly: portable Python bootstrap dropped in repo (.python-local/ + .nupkg), non-standard .git pointer + git-store/ layout with sandbox-user ownership, pre-commit install incomplete.

### Claude's verdict
File quality high on read-through (strict schema with additionalProperties:false, friendly validator errors, genuinely beginner-safe rollback doc separating unpushed/pushed/expert paths). Found 1 content bug: league_avg_goals documented as "total goals per match" — actually the per-team Poisson baseline (1.35/team ≈ 2.7 total). All 3 sandbox deviations confirmed real.

Takeover (in lieu of fix-round): resuming the sandboxed Codex session would re-hit the same sandbox walls, so Claude repaired directly — deleted .python-local/, python nupkg, .agents/, git-store/ and the .git pointer; fresh `git init -b main`; rebuilt .venv on the system Python 3.12.10; `pre-commit install` completed properly this time; fixed league_avg_goals descriptions in schema + guide; removed stale gitignore entries; authored docs/design-doc.md content (design stays with Claude per the build contract).

Proofs re-run by Claude (Codex output treated as advisory):
1. validator on good constants → exit 0 ✅
2. validator on broken copy (catch_vig=30) → exit 1, friendly error naming the key ✅
3. pre-commit run --all-files on staged tree → Passed ✅
4. Hook-block drill: junk constants edit + commit attempt → commit BLOCKED by hook (repo still has zero commits) ✅; good file restored, validated, re-staged.

State: 18 files staged, no commits, no GitHub repo, no push — awaiting human gate.
