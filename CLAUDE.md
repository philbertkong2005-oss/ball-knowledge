# Ball Knowledge 《走數》— project context

A 90s (pre-mobile) underground **football-betting life-sim**: Bettor → Bookie → Fixer.
Solo project. **Phil designs + decides; Codex writes the code; Claude is PM / spec-writer / reviewer.**

## Read these first (authoritative — do NOT duplicate their content elsewhere)

| Doc | Authority |
|---|---|
| `docs/phase1-match-engine.md` | 🔒 **FROZEN** contract for the match engine (Gate 1 PASSED) |
| `docs/world-design.md` | Authoritative for **world / economy / items / crime / actions / intel / bookmaking** (Phase 3+). Rev. 2026-07-17 — see the change table at the top. **§8.1 lists 8 open decisions that need Phil**; **§10 is a `constants.json` build handoff** |
| `docs/phase2-greybox.md` | Phase 2 build spec (greybox district) |
| `docs/design-doc.md` | Thin **pointer layer** over the above + pillars/acts. Loses any conflict with them |
| `PLAN-REVIEW-LOG.md` | Full history: every Codex review round, verdict, and phase closure |

## Status

- **Phase 0** ✅ repo, CI, Unity 6.3 LTS pinned (`6000.3.19f1`), rollback drills done
- **Phase 1** ✅ **Gate 1 PASSED** (tag `phase1-complete`) — C# match engine; blind ROI −8.29%, informed +44%, even-money win rate 56.4%. Information economy mathematically proven
- **Phase 2** 🔄 spec **APPROVED** by Codex (3 rounds, see PLAN-REVIEW-LOG.md); build IN PROGRESS — **step 1 done + verified loadable**: MatchEngine multi-targets `net8.0;netstandard2.1`, DLL at `unity/Assets/Plugins/`, Phase 1 proof re-run identical. **The Unity DLL is dependency-free and MUST stay that way** — if it ever grows a dependency that's a bug: fix with a shim in `NetstandardCompat.cs`, never a package (a `System.Text.Json` ref would drag in 8 DLLs Unity conflicts with). Unity-side config loading = `com.unity.nuget.newtonsoft-json` (built-in `JsonUtility` can't do the dictionaries in `constants.json`) — **and it MUST go through `EngineJsonContractResolver`** (reference impl in `prototype/BridgeTests/`): the DLL's `[JsonPropertyName]` shims are invisible to JSON readers, so default Newtonsoft binds every value as **silent zeros**, and snake_case naming is also wrong (`constants.json` mixes snake_case + camelCase; `teams.json` has `"ATK"`). The Phase 1 proof CANNOT catch this (it runs net8.0/real BCL) — `prototype/BridgeTests/` (6 tests, in the sln) guards it against the actual netstandard2.1 assembly. **Steps 2+3 done + verified** (commit `0a39fcb`): `tools/sync_streamingassets.py` (syncs all THREE design files incl. greybox; `--check` = CI drift guard; the ONLY allowed copy path) + `design/greybox.json` (spec detection numbers locked; design-weighted values are placeholders pending Phil) + schema/validator/pre-commit hook. **Task 2 (engine into Unity) BUILT + verified outside the editor** (commit `2627a78`): asmdef, `EngineJsonContractResolver` copy, pure-C# `GreyboxConfigLoader` (no UnityEngine — that is what makes it provable pre-editor), `BridgeSmokeTest` MonoBehaviour, and `docs/phase2-unity-checklist.md`. **The C#11-vs-Unity risk is RETIRED** — a netstandard2.1+C#9 probe compiles all engine usage; root cause is that the shim attributes are `internal`, so the C#11 gate is invisible to consumers (structural, not compiler-version luck). ⚠️ **Consequence: `required` gives Unity code ZERO compile-time protection — always load config through the resolver, never hand-build an `EngineConfig`.** ✅ **VERIFIED IN THE EDITOR by Phil, 2026-07-28** (tag `gate-phase2-bridge-verified`): all five `[BallKnowledge]` Console lines, **5 messages / 0 warnings / 0 errors**, canned bet seed 8 → `Harbour FC 2-0 Eastport Rovers` — identical to the Console and the pre-editor harness. **The engine-into-Unity bridge is PROVEN end-to-end; the C#9 编译 question is answered by a real editor run, not inference.** Gotcha for future sessions: editing `Packages/manifest.json` while the editor is open does NOT install a package — Unity only resolves on project load; use Package Manager `+` → *Install package by name* (checklist step 4a). **Next: the greybox itself** — real-scale 700m zoned map + 6 shop-shaped buildings (see the phase2-greybox.md amendment), FPS controller, then the patroller
- Phase 3+ = day loop, intel ladder, economy (see `world-design.md`)

## Design pillars (every feature must pass all four)

1. **Every bet is walked** — no menu-only actions
2. **Information is currency** — skill = knowing what the odds board doesn't
3. **Debt is the clock** — monthly minimum vs ~$100k principal
4. **Nothing buys better luck** — progression buys safety/info/reward size, never win probability

**Stamped guardrail:** the player's underworld role caps forever at *"the enforcement arm of the betting business."* No territory/racket-management sim, ever.

## Hard rules

- 🔒 **Phase 1 freeze:** never change engine semantics, public API, or math. Build/packaging plumbing is allowed **only** if the Phase 1 proof re-runs with **identical numbers** (9/9 tests + `validate`). If a number moves, revert.
- 🚦 **Gates are one-way doors.** Never build the next phase to escape a failed gate.
- **No hard-coded tunables** — all numbers live in `design/*.json`, schema-validated by the pre-commit hook.
- **Cheap vs expensive:** "one more row of data" = cheap, add freely. "Everything interacts with everything" = expensive, resist.
- **No real-money monetization. Ever.**
- **Scope is the #1 risk** (solo + beginner). Push back on anything that balloons a slice.

## Working agreement

- **Phil cannot read code.** Verify by *playing/observing* for gameplay; for non-visual work, end with a command + expected-vs-actual output. Never say "it works" without running it.
- **Always run the FULL proof after market/engine-structure changes** (`validate`, not just unit tests). A handicap filter once shipped a harness crash because only unit tests were run.
- Codex builds sandboxed → **Claude verifies in the real env** (Codex's numbers are advisory until re-run). Expect sandbox artifacts to clean up.
- One feature per commit; tag verified states `good-YYYYMMDD-HHMM`; `git push` is a manual human step.
- Pipeline that has worked 3×: **grill Phil → write spec → Codex adversarial review (loop to APPROVED) → Codex build → Claude verify → Phil signs off.**

## Commands

```bash
# from prototype/  (add --design-root <repo>/design if run elsewhere)
dotnet test BallKnowledge.sln                      # 9/9 engine (Gate-1 landmark) + 6/6 bridge must pass
dotnet run --project Console -- match --seed 42 [--home N --away N]
dotnet run --project Console -- board [--home N --away N]   # full odds board (American odds)
dotnet run --project Console -- stats --n 3000              # realism dashboard (~2.7 goals/match)
dotnet run --project Console -- validate                    # Gate 1 harness (~5 min)
.venv/Scripts/python tools/validate_constants.py            # constants schema check
```

.NET 8 SDK: `C:\Program Files\dotnet`. Unity editor pin: `unity/ProjectSettings/ProjectVersion.txt`.

## Communication

- **Bilingual: English body + 繁體中文 for decisions, verdicts, and summaries** (full bilingual on request — the selective form is deliberate to save context).
- Be a PM, not a yes-man: flag scope traps, name the real risk, give a recommendation.
- Prefer subagents for mechanical work (review loops, seed scans, verification sweeps) — they keep big outputs out of the main context.
