# Phase 2 Spec — Greybox District (Unity, first-person)
_Build spec for "Ball Knowledge". Locked via grill by Claude + Phil, 2026-07-16. Pending Codex review._

## Goal

Build the smallest walkable Unity greybox that answers **Gate 2's one question: does walking cash through gang turf, at night, actually make you nervous?** Grey blocks only — no art. The Phase 1 match engine is wired in minimally so betting generates the cash that creates the tension, proving the pure-C#-library-into-Unity reuse works. Everything not needed to feel that one walk is deferred.

中文摘要：用最小嘅可行走 Unity 灰盒回答 Gate 2 唯一問題——夜晚帶現金穿過黑幫地盤緊唔緊張？只用灰方塊、無美術。最小接駁 Phase 1 引擎，令落注產生現金去製造張力，並證明純 C# 庫接入 Unity 可行。唔需要嗰段路嘅一切全部延後。

## Scope — IN vs DEFERRED (the discipline that keeps this shippable)

**IN (the tension loop only):**
- First-person walk / run, mouse-look, vision-based hiding (crouch behind / inside grey cover).
- A minimal district: **3 grey buildings — shelter, bet shop, and one gang office/landmark — connected by ONE patrolled corridor** (the docks turf), plus a couple of side alleys and 2–3 hiding spots. No full zones.
- **ONE gang patrol** (1–2 patrollers) with a vision cone: patrol waypoints → spot player → chase → stop.
- **Gang-heat cash-carry tick:** carrying > $500 cash outdoors raises spot-chance over time (`+1/hour` in-game, per world-design §4.2). This is the only heat source in the slice.
- **The catch stop as a choice:** bribe / hand over all on-body cash toward debt + vig / fight-flee. (Fight-flee resolves simply in the slice: a chance roll to escape; caught-anyway = pay+vig.)
- **Minimal betting:** an interact at the bet shop opens a simple bet UI backed by the **real Phase 1 MatchEngine**; place one bet, resolve it, receive cash on-body. This cash is what you then must carry home.
- **On-body cash HUD + a home stash** at the shelter (deposit = safe, carried = at risk). Stash vs carried is the core cash-safety state.
- A minimal in-game clock so "night" exists and the hourly heat tick advances.

**DEFERRED to Phase 3+ (explicitly NOT in this slice):**
- Police heat, crimes, weapons, jail, bounties (no crime verbs in this loop).
- Survival bars (Health/Energy/Hunger), food, cooking, sleep-as-recovery.
- Intel-gathering, the three-tier intel ladder, the day loop, NPC connections, dialogue.
- The full economy (bank/card, laundering, businesses, item system, driving).
- The other 3 zones, the full building roster, multiple patrols, stakeouts.
- Art, audio, animation beyond what greyboxing needs.

中文摘要：**入**——第一人稱行走/跑/匿藏；3座灰建築（宿舍、投注站、幫會據點）＋一條被巡邏走廊＋幾條橫巷＋2–3匿藏點；一隊有視野錐嘅黑幫巡邏；帶現金>$500嘅heat tick；被截三選一；用**真 Phase 1 引擎**嘅最小落注畀你現金；身上現金HUD＋宿舍收藏點；最小時鐘（有「夜晚」）。**延後**——警察heat/犯罪/武器/坐監、生存bar、收料/情報階梯/日循環/NPC關係、完整經濟/駕駛、其餘3區、美術音效。

## Architecture & engine hookup

- Work in the existing `unity/` project (Unity 6.3 LTS, built-in 3D, pinned in `ProjectVersion.txt`).
- **MatchEngine reuse (the load-bearing proof):** reference the existing `BallKnowledge.MatchEngine` class library from the Unity project **unchanged** — it is pure C# with an injected `EngineConfig` and no file I/O, exactly so this works. Unity builds the SAME `EngineConfig` by reading `design/constants.json` + `design/teams.json` copied into `unity/Assets/StreamingAssets/` (the sync convention locked in Phase 0). **The MatchEngine library and its Phase 1 contract are NOT modified.**
- **StreamingAssets sync:** implement the Phase-0-documented step — a copy of `constants.json` + `teams.json` into `StreamingAssets/`, loaded at runtime via `System.Text.Json`. Document the sync so it can't silently drift from the authoring source in `design/`.
- Gameplay code lives in new `unity/Assets/Scripts/` (C#, authored by Codex); scene wiring (placing grey blocks, cameras, colliders) is GUI work done by Phil following written step-by-step instructions.
- All slice tunables (spot-chance curve, patrol speeds, cash-carry threshold reusing `catch_vig`, bribe cost, stash cap) live in a config the same way — either extend `constants.json` (schema v3, validator updated) or a sibling `greybox.json`. **Do not hard-code tunables.**

中文摘要：喺現有 `unity/` 做（Unity 6.3 LTS，已釘版本）。**引擎重用（承重證明）**：Unity 原封不動引用 `BallKnowledge.MatchEngine` 純 C# 庫，靠注入 `EngineConfig`（讀 `StreamingAssets` 內 `constants.json`＋`teams.json`）。**唔改動 MatchEngine 同其 Phase 1 合約。** 實作 Phase 0 已寫嘅 StreamingAssets 同步步驟。玩法碼喺新 `Scripts/`（Codex 寫），場景擺位係 Phil 跟指示做嘅 GUI 工作。切片可調數值放 config，唔寫死。

## The tension loop (one playable minute)

1. Start at the shelter. Interact → walk to the bet shop through the corridor (safe: little/no cash yet).
2. At the bet shop, place a bet on a real MatchEngine fixture; win → now carrying, say, $2,000+ cash on-body.
3. The cash-carry heat begins ticking. The gang patrol's spot-chance rises the longer you're outdoors over $500.
4. Walk home to stash the cash. The patrol is between you and the shelter. Choose route / use hiding spots / time your movement past the patrol's vision cone.
5. If spotted → chase → stop → the three-way choice. If you make it home → stash → safe → the relief beat.
6. **That round-trip, felt, IS Gate 2.**

## Gate 2 — pass/fail (deliberately human, not statistical)

Unlike Gate 1 (a math harness), Gate 2 is a **felt-experience gate**, judged by playtest:
- **Primary:** hand the build to 3–5 people (not-polite testers). **Do they voluntarily attempt the cash-carry walk a second time?** Do they describe a moment of tension unprompted ("I waited for the guy to turn around")?
- **Supporting checks (objective, so it's not pure vibes):** the patrol demonstrably spots the player through its vision cone and not through walls; hiding demonstrably breaks the chase; the cash-carry tick demonstrably raises stop-frequency (carrying $2,000 gets you stopped more than carrying $0 over the same walk); the catch stop presents all three choices and each resolves correctly (bribe deducts cash; pay+vig moves cash to debt with the 30% cut; fight-flee rolls escape).
- **Fail response:** if the walk is boring even with cash at stake, fix the tension (patrol pressure, route design, spot-chance curve) BEFORE adding anything new. Do not build Phase 3 to escape a failed Gate 2.

中文摘要：Gate 2 係**體感關卡**（唔似 Gate 1 係數學）：交畀3–5個唔會客氣嘅人試——**佢哋會唔會自願再行一次帶現金嗰段路？會唔會未提示就講出緊張時刻？** 客觀輔助檢查：巡邏靠視野錐（唔穿牆）發現玩家；匿藏可斷追逐；帶$2000比帶$0被截多；被截三選一各自結算正確。**唔達標＝先修張力，唔准為咗逃避而起 Phase 3。**

## Verification (per workflow.md — this is previewable Unity, so DRIVE it)

- **Automated (Codex writes):** EditMode/PlayMode tests for the pure-logic pieces — spot-chance curve given cash & time, catch-stop resolution math (bribe/pay+vig/flee), stash vs carried state, EngineConfig loads from StreamingAssets and a bet resolves. (Scene/vision-cone behaviour is validated by play, not unit tests.)
- **Play verification (the real proof):** run the build; screenshot/observe the patrol spotting via cone, a successful hide breaking a chase, and the three catch-stop choices resolving. Non-visual pieces (config load, bet resolution, cash math) end with a logged expected-vs-actual.
- **Commit discipline:** one feature per commit; `good-YYYYMMDD-HHMM` tags; scene-file (`.unity`) churn committed separately from script features. **CI:** extend to build the Unity project in batch mode + run EditMode tests (the Phase-0-deferred Unity CI now becomes worthwhile since real Unity code exists).

中文摘要：可預覽嘅 Unity，要**親自駕駛驗證**。自動測試（Codex 寫）：spot-chance 曲線、被截結算數學、收藏/攜帶狀態、由 StreamingAssets 載入 config 並結算一注（視野錐行為靠試玩，唔靠單元測試）。試玩證明：巡邏靠錐發現、匿藏斷追、三選一結算——截圖／觀察。CI 擴展到 batch 模式 build Unity＋跑 EditMode 測試（Phase 0 延後嘅 Unity CI 而家值得做）。

## Key decisions & tradeoffs

- **Minimal greybox (shelter → turf → bet shop)** over full zones — scope is the #1 solo-Unity risk; the walk only needs 3 buildings + one corridor to be felt. The other zones are greyboxed later once the walk is proven fun.
- **Real MatchEngine hookup** over a betting stub — proves the pure-library-into-Unity reuse (the whole reason Phase 1 was built as a no-file-IO class library) AND organically generates the carry-cash. Kept minimal (one bet, one fixture) so it doesn't expand scope.
- **First-person** — maximizes over-the-shoulder tension and sightline-hiding, and is the cheapest to greybox (no character model/animation needed).
- **Gate 2 is a felt-experience gate with objective supporting checks** — the tension is subjective, so playtest is primary, but the mechanics (cone spotting, hide-breaks-chase, cash-raises-heat, catch resolves) get objective checks so "it feels fine" can't paper over a broken system.
- **Config-driven tunables, MatchEngine untouched** — the frozen Phase 1 contract stays frozen; Phase 2 only *consumes* it.

## Risks / open questions

- **Unity GUI ↔ Codex-can't-click:** scene wiring (blocks, colliders, nav, camera) is Phil's hands via written steps; if this proves a recurring blocker, that's the moment to reconsider (the deferred Godot option was flagged at this gate in Phase 0). First real test of the Unity workflow.
- **StreamingAssets sync drift:** the authoring source is `design/`; the runtime copy is in `StreamingAssets/`. Needs a documented (ideally scripted) sync so they can't silently diverge.
- **Vision-cone AI in greybox:** must spot via line-of-sight, not through walls; the classic greybox bug. Called out as a supporting Gate-2 check.
- **First-person motion sickness / feel:** walk/run speeds and mouse-look need tuning to feel good; config-driven so it's fast to iterate.
- **Scene work is not unit-testable:** accepted — that's why Gate 2 is play-driven with objective checks on the underlying logic.

## Out of scope (Phase 2)

- Everything under "DEFERRED" above.
- Any change to the Phase 1 match-engine contract (`phase1-match-engine.md`), the engine library, or the validated betting math.
- Art, audio, animation, narrative integration, the full world-design.md world (this slice draws only the three buildings + corridor it needs).
