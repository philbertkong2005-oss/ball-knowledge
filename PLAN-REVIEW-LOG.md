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

## Phase 0 Closure — 2026-07-14

All exit criteria met:
- Preflight passed after real fixes (installed Python 3.12.10 + GitHub CLI via winget; git identity set)
- Repo skeleton, docs, constants schema+file+guide, requirements-dev.txt, .pre-commit-config.yaml committed (21d7cd9)
- Pre-commit hook demonstrably blocked a junk constants edit (zero-commit drill during Act 3 verification)
- CI green on GitHub (run 29284359016, success)
- Private GitHub repo live: philbertkong2005-oss/ball-knowledge; main + good-* tags pushed
- Unity 6.3 LTS project committed; authoritative editor pin unity/ProjectSettings/ProjectVersion.txt = 6000.3.19f1 (04d927c)
- .venv smoke command passes (validator exit 0)
- Developer personally executed BOTH rollback drills: unpushed junk edit -> git reset --hard to tag (learned bare `git reset` != `--hard` on the way); pushed junk commit b37ef65 -> git revert faf509d, both pushed

Phase 0 CLOSED. Next: Phase 1 — text-only Python match engine in prototype/ (separate plan).

## Round 1 — Codex (Phase 1 spec)

**Findings**

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:92) lines 92-97 make Gate 1 statistically unmeasurable because they never define stake sizing, bettor selection policy, skip/no-bet rules, or a confidence interval, so `1,000+` bets can pass or fail by noise alone; fix: specify an exact staking rule, an exact bet-selection algorithm for blind and informed bettors, a fixed sample size, and numeric pass bands with confidence intervals.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:94) makes “55-60% informed win rate” a bad target because win rate is meaningless across mixed-price markets like correct score and accumulators, where a profitable strategy can win far below 50%; fix: replace the primary informed target with ROI/CLV by market and only use win rate on a narrowly defined even-money subset.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:44) lines 44-52 define the true match as a shots→SoT→goals chain, but [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:76) still prices from a separate Poisson score matrix, so blind ROI will reflect model mismatch as much as hidden info; fix: price odds from the same engine with hidden factors removed, either analytically from the chain or via Monte Carlo.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:80) includes HT/FT, but the plan has no first-half intensity model and only scatters events after full-match outcomes, so halftime prices will be arbitrary; fix: add an explicit first-half/second-half split model and derive HT/FT from a halftime score matrix.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:80) includes first-goalscorer and anytime-goalscorer, but there is no starting-XI model, no replacement rule when a player is ruled out, no player goal-intensity formula, and no `No Goalscorer` outcome for 0-0; fix: define lineup selection, absence replacement, player scoring intensities, and mandatory `No Goalscorer`/void settlement rules.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:80) says “Asian handicap” and “accumulator” without line-selection or settlement rules, and same-game leg multiplication is wrong under correlation; fix: enumerate supported handicap lines and settlement mechanics, and restrict accumulators to independent cross-match legs only.

- [design-doc.md](/C:/Users/philb/Projects/ball-knowledge/docs/design-doc.md:109) says matches have `0-3 hidden per-match factors`, but [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:56) turns this into ~18 independent factor rows with rarity and no cap, which can wildly change edge frequency and make Gate 1 tunable by brute force; fix: define the exact sampling process, including max active factors, rarity normalization, and whether factors can stack on the same team/player.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:58) treats all factor effects as standardized magnitudes, but factors like “striker ruled out” and “playmaker suspended” are lineup-state changes, not just mild multipliers; fix: split factors into stat modifiers versus availability modifiers, with availability removing the player from lineup and scorer markets.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:46) says SoT depends on the shooting players’ finishing and [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:48) assigns scorers only after goals are sampled, which breaks the stated causality; fix: sample shooter identity per shot before SoT/goal resolution, or explicitly aggregate player skill into team-level rates and use the same weights consistently for attribution.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:29) gives generic attackers no stats, but [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:48) includes them in scorer weights, so Codex will invent defaults; fix: either generate explicit generic-player stats from team baselines or exclude unnamed players from scorer markets entirely.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:107) says “odds sum to ~110% overround,” but that is undefined for truncated correct-score books, first-goalscorer without a no-scorer bucket, and handicap markets with push states; fix: define each closed book exactly, include residual outcomes like `Any Other Score`, and test margin per market with explicit settlement states.

- [constants.json](/C:/Users/philb/Projects/ball-knowledge/design/constants.json:7) and [constants-guide.md](/C:/Users/philb/Projects/ball-knowledge/design/constants-guide.md:6) still define `league_avg_goals` and `home_advantage` in direct expected-goals units from the older Poisson model, while [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:44) repurposes the engine into a shot-chain with many new coefficients; fix: rewrite the constants schema/guide around the new model and define the units and role of every new parameter before implementation.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:19) promises runtime loading from repo `design/constants.json`, which breaks the “same validated code, unchanged” Unity claim because Unity builds will not have that repo-relative file path; fix: make the engine consume a config/data object interface and let the console read JSON while Unity supplies the same DTO from its own asset pipeline.

- [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:97) makes transcript quality part of pass/fail, but [workflow.md](/C:/Users/philb/Projects/ball-knowledge/docs/workflow.md:10) requires a written expected result for non-visual proof and “feels tense” is not reproducible; fix: move tone review out of Gate 1 or convert it to a concrete checklist tied to seeded transcripts.

- [design-doc.md](/C:/Users/philb/Projects/ball-knowledge/docs/design-doc.md:89) says promo tokens must pass the Phase 1 validation harness, but [phase1-match-engine.md](/C:/Users/philb/Projects/ball-knowledge/docs/phase1-match-engine.md:128) omits them entirely, leaving a design-doc contradiction that Codex will ignore; fix: explicitly defer tokens in the design doc too, or add a minimal token model and blind-with-token validation target now.

VERDICT: REVISE
### Claude's response (Phase 1, Round 1)

Accepted ALL 15 findings — the review found real statistical/architecture gaps. Key revisions to docs/phase1-match-engine.md:
1. Gate 1 made measurable: flat 1-unit stakes, fixed N=100,000, exact blind (random 1X2) + informed (edge>threshold) policies, 95% CIs, numeric pass bands.
2. Win rate demoted to secondary metric on even-money subset (odds 1.8-2.2) only; ROI is now the primary Gate-1 metric.
3. Odds now priced from the SAME engine with factors removed (Monte-Carlo), killing model mismatch — blind ROI reflects vig only.
4. Added two-half simulation so HT/FT prices are non-arbitrary.
5. Scorer markets fully specified: starting XI, availability removal, per-shot shooter draw, No Goalscorer residual, named-eligible + generic "Other" bucket.
6. Asian handicap restricted to half/whole lines with push settlement; accumulators cross-match independent legs only.
7. Factor sampling defined exactly: 0-max cap (default 3), rarity-weighted without replacement, one-per-player/team no-stack.
8. Factors split into stat-modifier vs availability (availability removes player from lineup + scorer books, redistributes intensity).
9. Causality fixed: shooter drawn per shot BEFORE on-target/goal resolution; attribution upstream.
10. Generic players get stats derived from team baseline; pooled into "Other player" scorer bucket (not invented ad hoc).
11. Each market defined as a closed book with residual outcomes (Any Other Score, No Goalscorer, handicap push); overround verified per market.
12. Constants rewritten for the shot-chain model (schema_version 2) as BUILD TASK 0, with a full new-parameter table (units/roles); old xG-unit params replaced.
13. Architecture fixed: MatchEngine does NO file I/O — takes an injected EngineConfig DTO; Console reads JSON, Unity supplies the same DTO. "Unchanged in Unity" now actually holds.
14. Tone review converted to a seeded, reproducible checklist SEPARATE from the numeric Gate 1 (satisfies workflow.md's written-expected-result rule).
15. Promo-token contradiction resolved: deferred in BOTH the spec and design-doc.md (Phase 3 shop feature; no Phase 1 token validation).

## Round 2 — Codex (Phase 1 spec)

Codex confirmed the 15 Round-1 findings were addressed, then raised 7 new material findings:
1. Core math (f/g/h rating→prob, player→team aggregation) still unspecified → Codex guesswork.
2. shot_base_half/corner_base_half/heightFactor/passMult referenced but undefined.
3. "Other player" bucket omitted from scorer books; No Goalscorer wrongly covering generic-only games.
4. Overround math undefined for whole-line handicap push/void (implied-prob sum invalid).
5. Informed bettor staking every positive-edge selection → correlated same-fixture bets → 95% CIs wrong.
6. Blind target "-8%" arithmetically wrong for a 1.10 book (should be 1/1.10-1 = -9.09%).
7. design-doc.md still described the obsolete direct-Poisson engine + 1,000-bet win-rate gate (conflicting spec).
VERDICT: REVISE

### Claude's response (Phase 1, Round 2)

Accepted all 7 new findings:
1. Added a "Rating formulas & aggregation" section with exact forms: sm(S)=clamp(0.5+S/100,...), pOnTarget/pGoal formulas, and a unified "attacking pool" (livePool/basePool) that handles condition AND availability by one mechanism. No more f/g/h guesswork.
2. Defined shot_base_half/corner_base_half (half-weight split summing to 1), heightFactor (0.5+height/100 → corners only), passMult (multiplies effective assist_rate + assister draw). No undefined/unused params.
3. First/anytime goalscorer books now have explicit "Other Player" (generic pool) outcome; "No Goalscorer" reserved for 0-0 only.
4. Push-capable handicaps priced in expected-return terms (EV = 1/overround per unit), NOT naive implied-prob sum; margin test is EV-based.
5. CI bug fixed: at most one bet per market per fixture (highest-edge selection); CIs are fixture-clustered bootstrap, not per-bet (bets on one fixture are correlated).
6. Blind ROI target corrected from eyeballed -8% to exact 1/1.10-1 = -0.0909 (-9.09%); updated constants.json blind_roi + guide; band [-0.10,-0.08].
7. design-doc.md "Match Engine" section rewritten to point at docs/phase1-match-engine.md as the authoritative contract (removed obsolete direct-Poisson + 1,000-bet win-rate gate).

## Round 3 — Codex (Phase 1 spec)

5 new findings:
1. Factor-count draw distribution P(0..max) unspecified.
2. Each factor's exact target field/sign/magnitude unspecified (wind/pitch/derby/etc. still guesses).
3. Availability drops player from livePool → team plays a man short instead of subbing a generic.
4. Even-money win-rate subset has no minimum count → bootstrap CI can be arbitrarily unstable.
5. Testing section says "per-market overround ≈ 1.10" for ALL markets, conflicting with EV-based push margining.
VERDICT: REVISE

### Claude's response (Phase 1, Round 3)

Accepted all 5:
1. Added explicit factor_count_weights PMF (default 0.30/0.40/0.22/0.08) to spec + constants.
2. Added a full factor effect table: every factor → exact target field(s), sign, kind (stat/avail/match), plus the magnitude application rule ((1±m) from tier).
3. Availability now promotes a generic substitute (XI stays 11, downgrade not man-short); named player still void in scorer/assist books.
4. Win-rate gate now requires ≥ min_even_money_bets (default 2,000) or it's reported-but-not-gateable; Gate 1 rests on ROI bands otherwise.
5. Split margin tests: probability-sum check for closed non-push books, EV-margin check for push-capable handicap books.

## Round 4 — Codex (Phase 1 spec)

3 findings (consistency/specification):
1. Gate 1 pass rule still required the win-rate metric unconditionally despite the "not gateable below min" clause.
2. Two divergent atk_eff/def_eff definitions (chain summary vs exact formula section).
3. cup rotation / surprise formation didn't define WHICH starter is rotated or WHICH alternate formation is chosen.
VERDICT: REVISE

### Claude's response (Phase 1, Round 4)

Accepted all 3:
1. Pass rule rewritten: Gate 1 = two ROI criteria; win-rate 55-60% applies ONLY when subset ≥ min_even_money_bets, else reported-not-required.
2. Earlier chain-summary strength line replaced with a pointer to the authoritative formula section (no divergent duplicate).
3. Added deterministic selection rules: cup rotation removes highest-involvement named attacker; surprise formation picks a seeded formation ≠ baseline.

## Round 5 — Codex (Phase 1 spec) — FINAL ROUND (MAX_ROUNDS=5)

2 findings (completeness):
1. Exact non-push margining formula left implicit (odds_i formula never stated).
2. Build Task 0 constants list incomplete vs params referenced later (factor_count_weights, factor_rarity, pricing_sim_count, correct_score_cap, O/U & handicap lines, edge_threshold, fixture count, ROI bands, min_even_money_bets).
VERDICT: REVISE

### Claude's response (Phase 1, Round 5)

Accepted both:
1. Added explicit proportional margining `impliedProb_i = fairProb_i × bookmaker_overround`, `odds_i = 1/impliedProb_i` for closed non-push books; push books explicitly use the EV rule.
2. Expanded Build Task 0 into a complete config inventory grouped (simulation / factors / odds & markets / validation) covering every parameter the spec references.

Both were pure completeness fixes with no design disagreement. Running one final confirmation resume (the convergence has been monotone: 15 → 7 → 5 → 3 → 2 findings, each round strictly smaller and non-controversial).

## Round 6 — Codex confirmation

No new material implementation blockers remain. Spec defines causal model, factor sampling/effects, market settlement/margining, validation gate, Unity-reuse boundary, and required config surface tightly enough that Codex should not need to invent core behavior. Minor note: some Chinese summary lines look stale vs final English gate details, but the authoritative contract is clear and measurable.

VERDICT: APPROVED

---
Converged: Phase 1 spec APPROVED after 5 review rounds (findings 15 -> 7 -> 5 -> 3 -> 2 -> 0). docs/phase1-match-engine.md is the frozen build contract. Awaiting user sign-off before build.
