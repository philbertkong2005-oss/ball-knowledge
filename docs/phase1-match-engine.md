# Phase 1 Spec — Match Engine (standalone C#)
_Frozen build spec for "Ball Knowledge". Locked via grill by Claude + Phil; hardened via Codex adversarial review, 2026-07-15._

## Goal

Build a standalone, text-only **C# match engine** that simulates fictional football matches, sets bookmaker odds **from its own simulation**, produces authentic 90s radio-style commentary, and **proves the information economy works mathematically** via a Monte-Carlo validation harness, before any Unity/graphics work exists. The engine is a **pure C# class library with zero Unity dependencies**, configured through an injected config object (not a file path), so the *same validated code* is later referenced by the Unity game (Phase 3) with no rewrite.

中文摘要：獨立純文字 C# 比賽引擎——模擬賽事、用**自己的模擬**開賠率、產生90年代旁述、用蒙地卡羅驗證證明情報經濟成立。純 C# 類別庫、零 Unity 依賴、透過注入的設定物件配置（非檔案路徑），Phase 3 的 Unity 直接引用同一份代碼。

## Architecture

```
BallKnowledge.MatchEngine   (pure C# class library — NO Unity refs, NO file I/O; configured via an EngineConfig object)
   ├── referenced by → BallKnowledge.Console   (reads design/constants.json → builds EngineConfig; runs playthrough + validation)  ← Phase 1
   └── referenced by → the Unity game          (builds the SAME EngineConfig from its own asset pipeline)                          ← Phase 3
```

- **Config injection (fixes the Unity-reuse bug):** `MatchEngine` never reads a file. It takes an immutable `EngineConfig` record (a plain DTO holding every tunable). The **Console** project reads `design/constants.json` + `design/teams.json` via `System.Text.Json` and constructs `EngineConfig`. Unity later constructs the identical `EngineConfig` from `StreamingAssets`. The engine is therefore genuinely reusable unchanged.
- Target framework: **.NET 8** (LTS). Solution in `prototype/` (`prototype/BallKnowledge.sln`, `MatchEngine/`, `Console/`, `Tests/`).
- **Deterministic:** the engine takes an injected RNG seed; the same seed → the same match. Required for reproducible tests and playtests.

中文摘要：引擎不讀檔，只收一個 `EngineConfig` DTO；Console 讀 JSON 建 config，Unity 之後用同樣方式建同一個 config——真正可重用。.NET 8，可注入亂數種子（同種子＝同一場）。

## Constants rewrite (BUILD TASK 0 — before any engine code)

The current `design/constants.json` holds `league_avg_goals`/`home_advantage` in **old direct-xG units** that do not fit the new shot-chain model. First task: **rewrite `constants.json` + its schema + guide (bump `schema_version` to 2)**, keeping the debt/vig/overround values, replacing the goal-model values, and adding every new parameter below with explicit units. The pre-commit validator must pass on the new file.

New engine parameters (all live in `constants.json`, all tuned to hit Gate 1):
| Param | Role / unit |
|---|---|
| `shot_base` | league baseline shots per team per match (count) |
| `on_target_base` | baseline P(a shot is on target), 0–1 |
| `conversion_base` | baseline P(an on-target shot is a goal before keeper adjust), 0–1 |
| `corner_base` | baseline corners per team per match (count) |
| `assist_rate` | P(a goal has a named assister), 0–1 |
| `second_half_factor` | multiplier on 2nd-half scoring vs 1st (≈1.0–1.2) |
| `home_advantage` | multiplier on home attacking strength (e.g. 1.10) |
| `formation_mods` | table: per formation → {atkMult, defMult, shotMult, cornerMult, passMult} |
| `factor_tier_magnitudes` | {minor, moderate, major} standardized effect sizes |
| `max_factors_per_match` | integer cap (default 3, per design doc's "0–3") |
| `bookmaker_overround` | already present (1.10) |

## Data model

### Team (`design/teams.json`, seeded from the 8-team strawman in design-doc.md)
`name`, `nameZh`, `ATK` (1–100), `DEF` (1–100), `height` (1–100), `baseFormation`, `teamForm` (0.7–1.3, weekly drift), squad.

### Player (Tier B — role-based; each team fields a fixed starting XI)
- **Attacker:** `finishing` (1–100), `involvement` (1–100).
- **Playmaker:** `passing` (1–100).
- **Keeper:** `reliability` (1–100).
- **Named vs generic:** 3–5 **named** players carry stats. Remaining XI slots are **generic** players who get **stats derived deterministically from the team's ATK/DEF baseline** (so nothing is invented ad hoc). Generic outfielders are eligible to score but pool into an **"Other player"** bucket for scorer markets (see Scorer markets).
- **Condition channel:** one per-named-player weekly multiplier with two news flavours (form / discipline). Multiplies effective stats. **Discipline modifies pre-match stats ONLY — never triggers in-match red cards (deferred with the player-booking market).**

### Formations (10 flat modifier packages — linear, NO matchup matrix)
`4-4-2, 4-3-3, 5-3-2, 4-2-3-1, 3-4-2-1, 4-5-1, 4-1-4-1, 3-5-2, 4-2-2-2, 3-4-3`. Each = a `formation_mods` row. Formations never interact.

中文摘要：球隊有攻/守/身高/陣式/狀態。3–5名具名球員帶數值，其餘 generic 由球隊基準推導（不憑空捏造），generic 入球歸入「其他球員」桶。狀態通道只改賽前數值。10陣式＝線性修正包。

## Simulation — the causal output chain (per half, then summed)

To price HT/FT correctly, each match is simulated as **two halves** (2nd half scaled by `second_half_factor`); halftime score = 1st-half result, full-time = sum. Per half, per team:

1. **Effective strengths:** computed by the exact formulas in "Rating formulas & aggregation" below — `atk_eff` via the attacking-pool `attackScale` (which subsumes condition + availability), `def_eff` via `factorDefMods`; the home team applies `home_advantage`. Availability factors alter the lineup **before** this step. (This line is a summary; the formula section below is authoritative.)
2. **Shots:** `shotsExp = shot_base_half × (atk_eff / opp_def_eff) × formation.shotMult`; `shots ~ Poisson(shotsExp)`.
3. **Shooter per shot (fixes causality):** for **each** shot, first draw the shooter from the team's eligible attackers weighted by `involvement × conditionMod` (named players + the generic pool). This shooter's `finishing` then drives that shot's on-target and goal resolution — so attribution is causally upstream, not bolted on after.
4. **On target:** shot is on-target with `p = on_target_base × f(shooter finishing)`.
5. **Goal vs save:** an on-target shot is a **goal** with `p = conversion_base × g(shooter finishing) × (1 − h(opp keeper reliability × keeper conditionMod))`; otherwise a **save** credited to the keeper. (Goals ≤ SoT ≤ shots is a tested invariant.)
6. **Assist:** each goal has a named assister with probability `assist_rate`, drawn by `passing`; else no assist / generic.
7. **Corners (parallel):** `cornersExp = corner_base_half × (atk_eff/opp_def_eff) × heightFactor × formation.cornerMult`; `corners ~ Poisson(cornersExp)`.
8. **Timeline:** all events across 90 minutes (goals with scorer/assister, notable saves, near-misses, corners, factor leaks, kickoff/HT/FT) → the ordered event list that **is** the radio script.

All coefficients live in `constants.json`; the spec fixes the *structure*, tuning fixes the *numbers*.

中文摘要：每場分兩半模擬（次半乘 `second_half_factor`），半場＝上半結果，全場＝相加。修正因果：**每一腳射門先抽射手**（按參與度加權），該射手的射術驅動射正與入球，歸屬在上游而非事後補。入球≤射正≤射門為受測不變量。

## Rating formulas & aggregation (concrete defaults — coefficients tunable in constants.json)

The spec fixes these exact forms so Codex does not guess; the numeric coefficients are `constants.json` values tuned to hit Gate 1. All ratings are 1–100 with **50 = league-neutral (multiplier 1.0)**.

- **Soft stat multiplier:** `sm(S) = clamp(0.5 + S/100, 0.2, 2.0)` → S=50→1.0, S=100→1.5, S=0→0.5.
- **On-target:** `pOnTarget = clamp(on_target_base × sm(shooter.finishing), 0.05, 0.95)`.
- **Goal vs save:** `pGoal = clamp(conversion_base × sm(shooter.finishing) × (1.5 − sm(keeper.reliability × keeperConditionMod)/2), 0.02, 0.98)`; the complement of an on-target shot is a save credited to the keeper. (Neutral point: finishing=reliability=50 → `pGoal = conversion_base`.)
- **Player → team aggregation (unifies condition AND availability via one "attacking pool"):**
  - `basePool = Σ(named attacker.involvement) + genericInvolvement`
  - `livePool = Σ(available named attacker.involvement × conditionMod) + genericInvolvement`
  - `attackScale = livePool / basePool`
  - `atk_eff = ATK × formation.atkMult × teamForm × attackScale × factorAtkMods × (home_advantage if home)`
  - An availability factor **replaces** the named player with a generic substitute: the named player's `involvement × conditionMod` leaves `livePool` and the generic sub's (baseline-derived, weaker) `involvement` enters it, so the XI stays 11 and the effect is a downgrade — not a man-short team. `def_eff = DEF × formation.defMult × teamForm × factorDefMods`.
- **Shooter draw:** per shot, shooter ∝ `involvement × conditionMod` over available named attackers + the generic pool.
- **Half allocation:** `firstHalfWeight = 1/(1+second_half_factor)`, `secondHalfWeight = second_half_factor/(1+second_half_factor)`; `shot_base_half = shot_base × (weight for that half)`; identically for `corner_base_half`. Weights sum to 1, so full-match totals are unchanged.
- **Height factor:** `heightFactor = clamp(0.5 + team.height/100, 0.5, 1.5)`, applied to `cornersExp` only (Phase 1).
- **Pass/assist:** effective assist rate `= clamp(assist_rate × formation.passMult, 0, 1)`; assister drawn ∝ `passing` over the team's playmakers/named attackers.

中文摘要：明確定死公式（係數放 constants.json 調校），50＝中性。射正/入球用 `sm(S)=0.5+S/100`。球員→球隊用單一「攻擊池」統一處理狀態與缺陣（缺陣＝退出 livePool）。半場配重、身高係數、傳球乘數全部給出明確式子。

## Hidden factors — ~18 rows, 3 tiers, with exact sampling + two kinds

**Sampling (fixes the "brute-force tunable" gap):** per match, draw the number of active factors from an explicit PMF `factor_count_weights` in `constants.json` — default `P(0)=0.30, P(1)=0.40, P(2)=0.22, P(3)=0.08` (must sum to 1; length = `max_factors_per_match`+1). Then select that many distinct factors by normalized rarity weights **without replacement**. **At most one factor per player and one per team** (no stacking). Match-condition factors apply to the whole fixture.

**Magnitude application rule:** each stat-modifier factor uses its tier magnitude `m` from `factor_tier_magnitudes`; a `−` factor multiplies its target field by `(1 − m)`, a `+` factor by `(1 + m)`. Availability factors ignore magnitude (they swap a player, below).

**Two kinds (fixes "ruled out ≠ multiplier"):**
- **Stat-modifier factors** — adjust the exact field(s) in the table below.
- **Availability factors** — **remove the named player from scorer/assist market eligibility AND promote a generic substitute into the XI so the side stays 11-a-side** (not man-short). The named starter's contribution leaves `livePool`; the generic sub's (weaker, baseline-derived) contribution enters it — net effect is a *downgrade*, correctly. The removed player is void in scorer/assist books.

**Exact factor table** (target field, sign, kind):
| Factor | Tier | Kind | Target field · sign |
|---|---|---|---|
| striker knock | min | stat | that striker `finishing` & `involvement` · − |
| striker ruled out | maj | avail | remove striker; promote generic sub |
| keeper hungover | mod | stat | keeper `reliability` · − |
| keeper elite form | mod | stat | keeper `reliability` · + |
| playmaker suspended | mod | avail | remove playmaker; promote generic sub |
| hot streak | min | stat | player `conditionMod` · + |
| cold streak | min | stat | player `conditionMod` · − |
| training bust-up | mod | stat | player `conditionMod` · − |
| model pro | min | stat | player `conditionMod` · + |
| winning morale | mod | stat | team `ATK` & `DEF` · + |
| losing crisis | mod | stat | team `ATK` & `DEF` · − |
| cup rotation | maj | avail | remove one named starter; promote generic sub |
| surprise formation | mod | stat | team plays a non-baseline formation (swap `formation_mods` row) |
| new-manager bounce | min | stat | team `ATK` & `DEF` · + |
| pay dispute | mod | stat | team `ATK` & `DEF` · − |
| waterlogged pitch | maj | match | both teams `shotsExp` & `conversion_base` · − |
| high wind | min | match | both teams `on_target_base` · − |
| derby | mod | match | both teams `atk_eff` · − (tighter, lower-scoring) |
| dead rubber | mod | match | both teams `atk_eff` · − (sloppy, lower-scoring) |

**Selection rules (deterministic given the match seed):** `cup rotation` removes the team's highest-`involvement` named attacker (most impactful and most newsworthy) and promotes a generic sub; `surprise formation` picks a seeded-pseudorandom formation from the 10 that differs from the team's `baseFormation`.

**Intel source → factor mapping** (this IS the information economy; `major` factors sit behind the far/expensive sources): training ground → fitness/form/injuries/bust-ups; groundskeeper → pitch/rotation; café → morale/streaks/rumours; insider → the `major` availability secrets.

**Engine principle (Phase 3 sabotage hook, free):** a factor's *source* may be a player action, applied identically to a random roll. Phase 1 rolls factors randomly only.

中文摘要：每場抽 0–3 個因素，按稀有度不重複抽，每名球員/每隊最多一個（不疊加）。兩類：數值修正 vs 可用性（後者把球員移出陣容與射手市場，強度重分配）。情報源對應＝經濟本身。

## Odds generation (priced from the SAME engine — fixes model mismatch)

Odds are computed by running the **same simulation, Monte-Carlo, with all hidden factors removed** (public-information view): simulate the fixture N times (e.g. 50k) sans factors, tally outcome frequencies → fair probabilities, then apply `bookmaker_overround` (1.10) to each **closed book**. Blind ROI then reflects only the vig, not model mismatch. (An analytic derivation may replace Monte-Carlo only if it provably matches the same distribution.)

**Each market is a closed book with residual outcomes and explicit settlement:**
- **1X2:** home/draw/away.
- **Over/Under goals:** at defined lines (e.g. 1.5/2.5/3.5).
- **Correct score:** enumerated scores up to a cap **plus `Any Other Score`** to close the book.
- **HT/FT:** the 9 combinations, from the halftime + full-time matrices.
- **Both teams to score:** yes/no.
- **Asian handicap:** only **half and whole lines** (e.g. −2, −1.5, −1, −0.5, 0, +0.5, …); whole-line exact ties **push/void** (stake returned); no quarter lines in Phase 1. **Push-capable markets are priced in expected-return terms, NOT by a naive implied-probability sum:** compute fair win/push/lose probabilities, then set each side's odds so a bettor staking at true probabilities has expected return exactly `1/overround` per unit (push returns stake). The margin test for these markets checks that EV, not a probability sum.
- **First goalscorer / anytime goalscorer:** named eligible players **plus an `Other Player` outcome** (covers goals scored by the generic pool) **plus `No Goalscorer`** (reserved for 0–0 only); ruled-out (availability) players are void/removed and their book renormalized.
- **Accumulator:** **independent cross-match legs only** (never same-match, to avoid correlation); odds = product of leg odds; settles all-win.

Margin (overround) is verified **per market**, each as its own closed set including residuals.

中文摘要：賠率用**同一引擎、去除隱藏因素、蒙地卡羅**模擬得出公平機率，再乘110%抽水——盲賭ROI只反映抽水，非模型不符。每個市場是含殘餘結果（Any Other Score、No Goalscorer、讓球push）的封閉盤，逐市場核對抽水。串關只限跨場獨立腿。

## The validation harness (Gate 1 — exact, measurable)

A console command runs a fixed Monte-Carlo over a **fixed 100,000 simulated fixtures** and prints a report with **fixture-clustered bootstrap 95% confidence intervals** (resample fixtures, not individual bets — because multiple bets on one fixture are correlated, so naive per-bet CIs would be wrong). Two bettors, **flat 1-unit stakes**, **at most one bet per market per fixture** (the single highest-edge selection in each market) to keep correlation bounded:

- **Blind bettor policy:** each fixture, pick a **uniformly random selection** in the 1X2 market, stake 1 unit at board odds, settle. Under proportional margining, EV per unit = `1/overround`, so **target blind ROI = 1/overround − 1 = 1/1.10 − 1 ≈ −0.0909 (−9.09%)**, derived exactly from the margin (not an eyeballed −8%). Pass band: **blind ROI ∈ [−0.10, −0.08]**, 95% CI within band. This exact target is stored as `blind_roi` in `constants.json` (= −0.0909).
- **Informed bettor policy:** knows the fixture's hidden factors. In each market computes true probability (factors included) vs board-implied (factors excluded) and, if the best selection's **edge = trueProb × oddsDecimal − 1 > `edge_threshold`**, stakes 1 unit on that one selection. Report **ROI (primary metric)** — pass band **informed ROI ≥ +0.05**.
- **Win-rate metric (secondary, correct usage):** reported **only on the near-even-money subset** (decimal odds ∈ [1.8, 2.2]); target **55–60%**. NOT applied to correct-score/accumulator books. **This metric only gates when the qualifying subset has ≥ `min_even_money_bets` (default 2,000) bets** — below that the bootstrap CI is too wide, so the metric is reported but declared "not gateable" and Gate 1 rests on the ROI bands.

**Gate 1 passes iff:** blind ROI in band **AND** informed ROI ≥ +0.05, both with fixture-clustered bootstrap 95% CIs inside their bands. The even-money win-rate 55–60% condition is applied **only when** the qualifying subset has ≥ `min_even_money_bets` bets; below that threshold it is reported but NOT required, and Gate 1 rests on the two ROI criteria alone. If a required criterion is missed, tune `constants.json`; do not proceed to Phase 2. (`edge_threshold`, fixture count, bands, `min_even_money_bets`, and the derived `blind_roi` all live in `constants.json`.)

中文摘要：定量驗證——平注1單位、固定 N=100,000、報95%信賴區間。盲賭策略：1X2隨機選，ROI應∈[−0.10,−0.06]。有情報策略：按 edge>門檻下注，**主指標ROI≥+0.05**；勝率只在接近均注（賠率1.8–2.2）子集報，目標55–60%。三者連CI都達標才過Gate 1。

## Radio commentary + a SEPARATE tone check (not part of numeric Gate 1)

Authentic 90s local sportscaster voice, generated from the event timeline via ~50–60 templates tagged by event type. Hidden factors must **leak** into commentary (hungover keeper flaps at a cross) so informed players hear their intel confirmed. Commentary is text (TTS out of scope).

**Tone acceptance (concrete, seeded — replaces the unmeasurable "feels tense"):** with fixed seeds, generate 5 transcripts and check a reproducible checklist: (a) every active factor produces at least one leak line; (b) a goal in the 80th minute or later produces an escalation/late-drama line; (c) HT and FT summary lines are present with correct scores; (d) no template placeholder leaks unrendered. This checklist is a required test but is **separate from the statistical Gate 1**.

## League & season

8 teams (strawman), double round-robin (14 rounds); the console can run a single match, a full 10-week slice season, or the validation harness. Names are placeholder — reskinning does not block the build.

## Testing & verification (per workflow.md)

Automated tests (`prototype/Tests/`, xUnit): distribution sanity (Poisson/Binomial means), chain invariants (goals ≤ SoT ≤ shots; every goal has a scorer; availability factors remove named players from scorer books while keeping the XI at 11 via a generic sub), **margin checks split by book type** — a **probability-sum** check (implied probs sum to ≈ `overround`) for closed non-push books (1X2, O/U, correct score incl. `Any Other Score`, HT/FT, BTTS, scorer incl. `Other Player`/`No Goalscorer`), and an **EV-margin** check (bettor at true probs returns ≈ `1/overround` per unit) for push-capable handicap books — factor sampling respects the PMF/cap/no-stack rules, HT/FT consistency (HT ≤ FT componentwise in the sampled realization), and the seeded tone checklist. Non-visual proof artifacts: (1) one seeded readable match with commentary; (2) the 100k-bet validation report with CIs.

## Key decisions & tradeoffs

- **Odds priced from the same engine (factors removed)** — the single most important fix: blind ROI must reflect vig only, not a second model's mismatch.
- **ROI is the primary Gate-1 metric; win-rate only on an even-money subset** — win rate is meaningless across mixed-price books.
- **Config injected as an `EngineConfig` DTO, engine does no file I/O** — makes the "unchanged in Unity" claim actually true.
- **Constants rewritten for the shot-chain model (schema v2) as build task 0** — the old xG-unit constants don't fit; do this before engine code.
- **Shooter drawn per shot, before resolution** — attribution is causally correct, not post-hoc.
- **Factors split into stat-modifier vs availability** with an explicit capped, no-stack sampling process — prevents brute-forcing Gate 1 and models "ruled out" correctly.
- **Closed books with residual outcomes** (`Any Other Score`, `No Goalscorer`, handicap push) and half/whole handicap lines only — makes every market's overround well-defined and settleable.
- **Two-half simulation** — required for non-arbitrary HT/FT pricing.
- **Tone check is a seeded checklist, separate from statistical Gate 1** — keeps Gate 1 reproducible per workflow.md.
- **Anti-repetition systems, sabotage world, promo tokens, Wave-2 markets: deferred** (see Out of scope).

## Risks / open questions

- **Tuning to hit the bands may take several iterations** — the harness + `constants.json` fast-iteration loop is the backstop.
- **Monte-Carlo odds cost** — pricing every fixture by 50k sims × a full season × the 100k-bet harness must stay fast; cache per-fixture fair odds, keep the engine allocation-light. Acceptable on a dev machine; revisit if slow.
- **No human code review** — mitigated by tests, invariants, deterministic seeds, and the statistical harness.

## Out of scope (Phase 1)

- Unity, graphics, the walkable world, day loop, debt clock, intel-gathering UI (Phase 2–3).
- **Promo tokens** — explicitly deferred here AND to be marked deferred in `design-doc.md` (they are a Phase 3 shop feature; no token validation in Phase 1).
- Wave-2 bet markets (corners/shots/player-shots/assists/saves) — engine produces the data; markets added post-Gate-1.
- Anti-repetition systems (context-dependent factors, unreliable intel, market-prices-public-info) — post-Gate-1.
- Sabotage/fixing world system (design logged; the engine hook is free, the world is Phase 3).
- In-match cards / player-booking market; quarter handicap lines; TTS/audio; commentary localization; Acts 2–3.
