# Ball Knowledge 《走數》— project context

A 90s (pre-mobile) underground **football-betting life-sim**: Bettor → Bookie → Fixer.
Solo project. **Phil designs + decides; Codex writes the code; Claude is PM / spec-writer / reviewer.**

## Read these first (authoritative — do NOT duplicate their content elsewhere)

| Doc | Authority |
|---|---|
| `docs/phase1-match-engine.md` | 🔒 **FROZEN** contract for the match engine (Gate 1 PASSED) |
| `docs/world-design.md` | Authoritative for **world / economy / items / crime / actions / intel** (Phase 3+) |
| `docs/phase2-greybox.md` | Phase 2 build spec (greybox district) |
| `docs/design-doc.md` | Thin **pointer layer** over the above + pillars/acts. Loses any conflict with them |
| `PLAN-REVIEW-LOG.md` | Full history: every Codex review round, verdict, and phase closure |

## Status

- **Phase 0** ✅ repo, CI, Unity 6.3 LTS pinned (`6000.3.19f1`), rollback drills done
- **Phase 1** ✅ **Gate 1 PASSED** (tag `phase1-complete`) — C# match engine; blind ROI −8.29%, informed +44%, even-money win rate 56.4%. Information economy mathematically proven
- **Phase 2** 🔄 spec **APPROVED** by Codex (3 rounds, see PLAN-REVIEW-LOG.md); build IN PROGRESS — step 1 done (MatchEngine multi-targets `net8.0;netstandard2.1`, DLL at `unity/Assets/Plugins/`, Phase 1 proof re-verified identical)
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
dotnet test BallKnowledge.sln                      # 9/9 must pass
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
