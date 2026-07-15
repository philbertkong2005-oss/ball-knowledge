# Phase 1 Spec — Match Engine (standalone C#)
_Frozen build spec for "Ball Knowledge". Locked via grill by Claude + Phil, 2026-07-15._

## Goal

Build a standalone, text-only **C# match engine** that simulates fictional football matches, sets bookmaker odds, produces authentic 90s radio-style commentary, and — most importantly — **proves the information economy works mathematically** via a Monte-Carlo validation harness, before any Unity/graphics work exists. The engine is written as a **pure C# class library with zero Unity dependencies**, so the *same validated code* is later referenced by the Unity game (Phase 3) with no rewrite and no translation risk.

中文摘要：獨立、純文字的 C# 比賽引擎——模擬虛構足球賽、開賠率、產生90年代電台旁述，並用蒙地卡羅驗證證明情報經濟數學成立。純 C# 類別庫、零 Unity 依賴，之後 Unity（第三階段）直接引用同一份代碼，零重寫。

## Architecture

```
BallKnowledge.MatchEngine   (pure C# class library — NO Unity references, just logic + data)
   ├── referenced by → BallKnowledge.Console   (text playthrough + 1,000-bet validation harness)  ← Phase 1 deliverable
   └── referenced by → the Unity game (unity/)                                                     ← Phase 3, unchanged
```

- Target framework: **.NET 8** (LTS) console + classlib. Solution/projects live in `prototype/` (e.g. `prototype/BallKnowledge.sln`, `prototype/MatchEngine/`, `prototype/Console/`, `prototype/Tests/`).
- All tunable numbers load from the repo `design/constants.json` at runtime (via `System.Text.Json`). The engine must never hard-code a tunable value. New Phase-1 constants are added to `constants.json` + its schema + guide (validated by the existing pre-commit hook).
- No Unity, no graphics, no networking. Output is console text + a validation report.

中文摘要：純類別庫 + console + 測試三個專案，放 `prototype/`，用 .NET 8。所有可調數字從 `design/constants.json` 讀取，永不寫死。無 Unity、無圖像。

## Data model

### Team
`name`, `nameZh`, `ATK` (1–100), `DEF` (1–100), `height` (1–100), `baseFormation`, `teamForm` (0.7–1.3, drifts weekly), plus a squad of named players. The 8-team strawman league in `docs/design-doc.md` is the seed data (Harbour FC, Eastport Rovers, Victoria Athletic, Stonecutters FC, Tai Fung SC, Central United, Kowloon Quarry, Aberdeen Fishermen), authored into `design/teams.json`.

### Player (Tier B — role-based, 3–5 named per team; rest are flavour names with no stats)
- **Striker/attacker:** `finishing` (1–100), `involvement` (1–100).
- **Playmaker:** `passing` (1–100).
- **Keeper:** `reliability` (1–100).
- **Condition (all named players):** a single per-player weekly modifier channel with two news flavours — **form** (natural drift + streaks) and **discipline** (news events: training-ground bust-up / missed training → debuff; model professional → buff). Implemented as ONE multiplier on the player's effective stats. **Constraint: discipline modifies pre-match stats only — it must NOT trigger in-match red cards that alter the live simulation (deferred with the player-booking market).**

### Formations (10 preset modifier packages — linear, NO matchup matrix)
`4-4-2, 4-3-3, 5-3-2, 4-2-3-1, 3-4-2-1, 4-5-1, 4-1-4-1, 3-5-2, 4-2-2-2, 3-4-3`. Each is a package of multipliers on the team's numbers (ATK, DEF, shot volume, corner tendency, passing/assist tendency). Formations never interact with each other — each is a flat modifier set. Values live in `constants.json`.

中文摘要：球隊有攻/守/身高/陣式/狀態＋陣容。Tier B 球員按位置帶數值＋一條 form/discipline 狀態通道（紀律只改賽前數值，不觸發賽中紅牌）。10個陣式＝線性修正包，無相剋矩陣。

## Simulation — the causal output chain

One consistent match produces goals, shots, shots-on-target, saves, corners, scorers, and assisters — all at once, chained so the numbers stay honest (cannot have more goals than shots on target):

1. **Effective strengths:** `atk_eff = ATK × formation.atkMult × teamForm × conditionMults`; `def_eff = DEF × formation.defMult × teamForm`. Home team gets `home_advantage`. Hidden factors modify the relevant inputs at this step.
2. **Shots:** `shotsExp = shotBase × (atk_eff / opp_def_eff) × formation.shotMult`; sample `shots ~ Poisson(shotsExp)`.
3. **Shots on target:** each shot is on-target with a rate driven by the shooting players' `finishing`; `SoT ~ Binomial(shots, onTargetRate)`.
4. **Goals vs saves:** each SoT resolves to a **goal** with probability driven by shooter `finishing` vs opponent keeper `reliability` (× keeper condition); otherwise it's a **save** credited to the keeper.
5. **Attribution:** each goal is assigned a **scorer** (weighted by `finishing × involvement` across the team's named + generic attackers) and, with an assist rate, an **assister** (weighted by `passing`).
6. **Corners (parallel track):** `cornersExp = cornerBase × (atk_eff / opp_def_eff) × heightFactor × formation.cornerMult`; sample `corners ~ Poisson(cornersExp)`.
7. **Timeline:** all events (goals, notable saves, near-misses, corners, factor "leaks") are scattered across 90 simulated minutes → this ordered event list **is** the radio commentary script.

All coefficients (`shotBase`, `onTargetRate` baseline, conversion base, `cornerBase`, assist rate, home_advantage, etc.) live in `constants.json` and are **tuned to hit the Gate 1 validation target** — the spec fixes the *structure*, tuning fixes the *numbers*.

中文摘要：因果鏈：有效攻防→射門(Poisson)→射正(Binomial)→入球或撲救→歸屬射手/助攻→角球(平行)。事件散落90分鐘＝旁述稿。所有係數放 constants.json，調校去達到 Gate 1 目標。

## Hidden factors (the intel core) — ~18 factors, 3 severity tiers

Each factor is a data row: what it **attaches to** (player / team / match), its **effect** (modifies a chain input), a **severity tier** (`minor` / `moderate` / `major` — standardized magnitudes, so balancing 18 factors = tuning 3 magnitudes + assigning tiers), a **rarity %**, and which **intel source** can reveal it.

- **Player:** striker knock `minor`, striker ruled out `major`, keeper hungover `moderate`, keeper elite form `moderate`, playmaker suspended `moderate`, hot streak `minor`, cold streak `minor`, training-ground bust-up `moderate`, model professional `minor`.
- **Team:** winning-streak morale `moderate`, losing crisis `moderate`, squad rotation for cup `major`, surprise formation `moderate`, new-manager bounce `minor`, pay-dispute unrest `moderate`.
- **Match conditions:** waterlogged pitch `major`, high wind `minor`, derby/high-stakes `moderate`, dead rubber `moderate`.

**Intel source → factor mapping** (this IS the information economy; `major` factors sit behind the expensive/far sources so the training-ground trek pays off):
- **Training ground** (far, reliable): player fitness, form, injuries, bust-ups.
- **Groundskeeper**: pitch condition, rotation hints.
- **Café gossip** (cheap, unreliable): morale, streaks, rumours.
- **Insider** (expensive, limited uses): the `major` secrets (confirmed suspensions, cup rotation).

**Engine principle (enables the Phase 3 fixing/sabotage system for free):** a factor's *source* may be a player action, not only a random roll — the engine applies a factor identically regardless of source. Phase 1 only rolls factors randomly; the hook exists for later.

中文摘要：約18個因素，分3級（輕微/中等/重大，只調3個標準幅度）。情報源→因素對應＝情報經濟本身，重大因素鎖在昂貴/遠情報源後。引擎原則：因素來源可以是玩家行動（為第三幕造馬預留，零成本）。

## Odds generation

The shop prices every market from the Poisson score matrix computed using **yesterday's public information only** — i.e. team/player base stats WITHOUT the hidden factors (the factors are exactly the private edge the player hunts). Implied probabilities are then inflated by `bookmaker_overround` (1.10) so a blind bettor slowly loses. The player's entire edge is knowing today's factor modifiers before the board does.

## Bet markets — Wave 1 (built + validated in Phase 1)

1X2 (home/draw/away), Asian handicap, over/under total goals, correct score (jackpot), HT/FT, both-teams-to-score, **first goalscorer**, **anytime goalscorer**, and **accumulator** (combine legs, multiply odds). Each market computes its odds from the same score/scorer matrices.

**Wave 2 (logged, NOT built in Phase 1 — added one at a time post-Gate-1):** over/under corners, over/under team shots & shots-on-target, player shots, assists, keeper saves. The engine already *produces* these outputs; Wave 2 is mostly adding each market's odds line + running the harness.

中文摘要：賠率用「昨日公開資訊」（無隱藏因素）＋110%抽水計算。Wave 1 市場：主客和、讓球、大小球、波膽、半全場、雙方入球、首名/任何射手、串關。Wave 2（記低不建）：角球、射門、射正、球員射門、助攻、撲救。

## Radio commentary

Authentic 90s local sportscaster voice — grounded, period-accurate, warm-not-cartoonish, tense at goals. Commentary is generated from the match event timeline via templates. Crucially, **hidden factors must "leak" into the commentary** (e.g. a hungover keeper visibly flaps at a cross), so an informed player hears their intel confirmed live while a blind bettor just hears "a save." Provide ~50–60 template lines tagged by event type (goal, miss, save, corner, momentum, factor-reveal, kickoff, half-time, full-time). Commentary is text (a future TTS-through-radio-EQ pass is out of scope).

## The validation harness (Gate 1 — pass/fail)

A console command runs a Monte-Carlo simulation and prints an ROI/win-rate report:
- **Blind bettor** (bets off the odds board with no factor knowledge): target ROI ≈ `blind_roi` (−0.08 / −8%) over 1,000+ simulated bets.
- **Fully-informed bettor** (knows all of today's hidden factors): target win rate **55–60%** (`informed_win_rate_min/max`) with clearly positive ROI.
- Report both, per market where practical, so tuning can target the gap.

**Gate 1 passes only if:** blind ≈ −8%, informed lands 55–60% with positive returns, AND a human read-through of ~5 match transcripts feels tense (a late equalizer should land emotionally even in plain text). If the numbers miss, tune `constants.json` — do not proceed to Phase 2 until they hold.

中文摘要：Monte-Carlo 驗證：盲賭 ROI≈−8%、全情報 55–60% 勝率且正回報，且人手讀5份旁述稿覺得緊張。不達標就調 constants.json，未過不入第二階段。

## League & season

8 teams (the strawman), double round-robin = 14 rounds; the console can simulate a full 10-week slice season and/or single matches on demand. Team/player names are placeholders the designer will reskin — names do not block the build.

## Testing & verification (per repo workflow.md)

- Automated tests (`prototype/Tests/`, xUnit or similar) for: Poisson/Binomial sampling sanity, the causal-chain invariants (goals ≤ SoT ≤ shots; every goal has a scorer), odds sum to ~110% overround, factor application, and the validation-harness math.
- Non-visual verification: the console prints (a) a single readable match with commentary, and (b) the 1,000-bet validation report. Both are the Phase 1 proof artifacts.
- Deterministic seed option (pass a seed → reproducible match) so tests and playtests are repeatable.

## Key decisions & tradeoffs

- **C# standalone (pure class library) over Python** — eliminates the Phase-3 rewrite/translation step; same validated engine runs in the console harness and later in Unity. Small cost: install the .NET 8 SDK.
- **Tier B players + one condition channel** — concrete human intel (the core pillar) without the Tier C full-squad simulation swamp. Discipline is pre-match-modifier only; in-match cards deferred.
- **10 formations as flat modifier packages** — linear cost; a matchup matrix (quadratic) was explicitly rejected.
- **Full causal output chain now (goals/shots/SoT/saves/corners/scorers/assists)** — biggest single chunk of Phase 1, accepted because it's the load-bearing system AND it powers concrete intel; Wave-2 markets then come almost free.
- **Anti-repetition systems (context-dependent factors, unreliable intel, market-prices-public-info) DEFERRED** to post-Gate-1 — ship the simplest provable engine first.
- **Odds priced off stale public info only** — the hidden factors ARE the edge; this is fixed by the design pillars, not a tunable.
- **Structure fixed in spec, numbers tuned to the target** — coefficients live in `constants.json` and are tuned to hit Gate 1 rather than guessed up front.

## Risks / open questions

- **Tuning to hit 55–60% may take several iterations** — the harness is the backstop; budget tuning time, and keep coefficients in `constants.json` for fast iteration.
- **Correlation realism** — the causal chain must keep outputs consistent (goals ≤ SoT ≤ shots); tested as an invariant.
- **No human code review** (beginner + Codex) — mitigated by automated tests + the validation harness + deterministic seeds.
- **Radio tone is subjective** — the read-aloud check is the acceptance test; iterate templates if flat.

## Out of scope (Phase 1)

- Unity, graphics, the walkable world, the day loop, debt clock, intel-gathering UI (all Phase 2–3).
- Wave-2 bet markets (corners/shots/player-shots/assists/saves) — engine produces the data, markets added post-Gate-1.
- The anti-repetition systems and the sabotage/fixing world system (design logged; built later).
- In-match cards / player-booking market.
- TTS/audio, localization of commentary, Acts 2–3.
