# Phase 2 Spec — Greybox District (Unity, first-person)
_Build spec for "Ball Knowledge". Locked via grill by Claude + Phil; hardened via Codex review round 1, 2026-07-16._

## Goal

Build the smallest walkable Unity greybox that answers **Gate 2's one question: does walking cash through gang turf, at night, actually make you nervous?** Grey blocks only — no art. The Phase 1 match engine is wired in minimally so betting generates the cash that creates the tension, proving the pure-C#-library-into-Unity reuse works. Everything not needed to feel that one walk is deferred.

中文摘要：用最小嘅可行走 Unity 灰盒回答 Gate 2 唯一問題——夜晚帶現金穿過黑幫地盤緊唔緊張？只用灰方塊、無美術。最小接駁 Phase 1 引擎，令落注產生現金去製造張力，並證明純 C# 庫接入 Unity 可行。唔需要嗰段路嘅一切全部延後。

## Scope — IN vs DEFERRED

**IN (the tension loop only):**
- First-person walk / run, mouse-look, crouch, vision-based hiding (behind / inside grey cover).
- A minimal district: **3 grey buildings — shelter, bet shop, one gang landmark — connected by ONE patrolled corridor** (the docks turf), plus a couple of side alleys and 2–3 hiding spots.
- **Exactly ONE gang patroller** (not 1–2) with a vision cone: waypoints → spot → chase → stop.
- **Gang-heat cash-carry tick:** carrying > carry-threshold cash outdoors raises the patroller's spot-chance over time. The only heat source in the slice.
- **The catch stop as a choice:** bribe / hand over all on-body cash toward debt + vig / fight-flee (flee = a seeded escape roll; caught-anyway = pay+vig).
- **Deterministic cash source (see §"Gate-2 cash"):** a canned, seeded bet at the bet shop that ALWAYS pays out on the first try, so every session reliably reaches the walk. Betting skill is not what Phase 2 tests.
- **Minimal VISIBLE debt ledger:** a single on-screen debt number (starting value in `greybox.json`) that visibly decreases when cash is handed over (pay+vig), plus a before/after stop summary — so the catch is a concrete state change, not arbitrary money deletion.
- **On-body cash HUD + a home stash** at the shelter (deposit = safe, carried = at risk).
- A minimal in-game clock so "night" exists and the hourly heat tick advances.

**DEFERRED to Phase 3+ (explicitly NOT in this slice):**
- Police heat, crimes, weapons, jail, bounties. Survival bars, food, sleep-as-recovery. Intel-gathering, the intel ladder, the day loop, NPC connections, dialogue. The full economy (bank/card, laundering, businesses, items, driving). The other 3 zones, the full building roster, multiple patrols, stakeouts, the full monthly-debt system. Art, audio, animation beyond greyboxing. **Unity CI (see §Verification — it becomes a follow-up task, not part of Gate 2).**

中文摘要：**入**——第一人稱行走/跑/蹲/匿藏；3座灰建築＋一條被巡邏走廊＋幾條橫巷＋2–3匿藏點；**剛好一隻**巡邏（唔係1–2）；帶現金heat tick；被截三選一；**確定性現金來源**（一注固定會中，令每次都到得到嗰段路）；**可見債務數字**（交錢時見到減少＋前後對比）；身上現金HUD＋宿舍收藏；最小時鐘。**延後**——警察/犯罪/武器、生存bar、收料/日循環/NPC、完整經濟/駕駛、其餘區、多巡邏、完整月供債務、美術音效、**Unity CI（變後續任務，唔屬 Gate 2）**。

## Architecture & the MatchEngine bridge (Critical — settle before any code)

The Unity project cannot reference the `prototype/` class library directly (Unity has its own compilation; a `net8.0` `.csproj` outside `Assets/` is not ingestible). **Bridge mechanism (locked):**

- **The freeze is defined by OUTCOMES, not file-immutability:** what stays frozen is the **public API, the engine behaviour, and the Gate-1 outputs** (9/9 tests + identical `validate` numbers). **Project-level compatibility plumbing needed to compile for `netstandard2.1` is explicitly ALLOWED** — because the engine's `required`/`init` records and `System.Text.Json` usage will need it. Permitted: multi-target `<TargetFrameworks>net8.0;netstandard2.1</TargetFrameworks>`, adding package references, and adding polyfill shims (`IsExternalInit` for `init`, a `RequiredMemberAttribute`/`CompilerFeatureRequired` shim for `required`) under a conditional compile so net8.0 is untouched. **BANNED without separate re-review:** any change to the engine's semantic logic, public method/property signatures, or math. **Mandatory gate:** after the bridge change, re-run the Phase 1 proof (9/9 tests + `validate`) and confirm **identical Gate-1 numbers**; if any number moves, the change was semantic — revert and re-review.
- **Deliver the engine to Unity as a prebuilt DLL:** build the `netstandard2.1` output and place `BallKnowledge.MatchEngine.dll` under `unity/Assets/Plugins/`. Unity loads it as a managed plugin. Gameplay scripts reference it via an `asmdef`.
- **RESOLVED in step 1 — the DLL must stay dependency-free (`System.Text.Json` package reference REJECTED).** The engine never calls `JsonSerializer`; it only carries `[JsonPropertyName]` as **inert metadata** (all real (de)serialisation lives in the host — `Console/Program.cs`, or Unity). So the package reference bought nothing while stamping a hard assembly reference into the DLL, which dragged **8 transitive BCL DLLs** into `Plugins/` — several of which Unity already ships (duplicate-assembly conflicts), with IL2CPP reflection-stripping risk on top. Instead, a `JsonPropertyNameAttribute` shim in `NetstandardCompat.cs` under `#if NETSTANDARD2_1` removes the reference entirely. **Verified:** `publish` emits **1 file, not 9**; `deps.json` dependency list is **empty**; `Models.cs` unchanged; net8.0 still binds the real BCL type. Phase 1 proof re-run — 9/9 tests; blind ROI **−8.29%**, informed **+44.09%**, even-money **56.36% on 2014 bets** — **identical to Gate 1**. **Rule going forward: if the Unity DLL ever grows a dependency, that is a bug — fix it with a shim, not a package.**
- **The Phase 1 *behaviour* stays identical; the engine `.cs` semantic sources stay unchanged** (only build metadata, package refs, and compile-time shims may be added). `phase1-match-engine.md`, `constants.json`, `constants.schema.json`, and `teams.json` stay byte-for-byte unchanged.
- **Config for the engine:** the runtime copies of `design/constants.json` + `design/teams.json` in `unity/Assets/StreamingAssets/` are produced by **ONE repeatable sync — a single command (`tools/sync_streamingassets.py`) or a Unity editor menu action — that copies both files and logs success.** **No manual copy step** (that is the silent-drift footgun). The synced files may be committed, but they must only ever be regenerated by the sync, never hand-edited; the sync is also runnable in CI to assert the copies match `design/`.
- **Unity-side config loading (RESOLVED — supersedes the original "via `System.Text.Json`"):** that route is dead, because the engine DLL is now dependency-free and must stay that way. Unity's built-in `JsonUtility` is **also ruled out**: `formation_mods`, `factor_tier_magnitudes`, and `factor_rarity` are **dictionaries** (`"4-4-2": {atkMult…}`), which `JsonUtility` cannot deserialize at all. **Use `com.unity.nuget.newtonsoft-json`** — Unity's own UPM package, built for exactly this, no assembly conflicts. **Key mapping is NOT a naming-strategy problem:** the files mix conventions (`snake_case` top level, `atkMult`/`nameZh` camelCase nested, `ATK`/`DEF` uppercase), so no single resolver strategy works. **Preferred:** a small Newtonsoft `ContractResolver` that reads the engine's existing `[JsonPropertyName]` metadata via `GetCustomAttributesData()` (works despite the shim being `internal`, and needs no type instantiation) — ~15 lines, single source of truth, zero drift. **Fallback:** a hand-written mirror DTO (rejected by default: 30+ fields duplicated = silent-drift risk).
- **Bridge parity check (mandatory, Gate-2 supporting):** running the canned Gate-2 seeded fixture **inside Unity** must produce the **identical scoreline and settlement** the Console produces for that same seed. This is the cheap end-to-end proof that the DLL loaded, config deserialised correctly, and no field silently defaulted to `0` — a wrong config cannot survive it.
- **Greybox tunables live in a SEPARATE `design/greybox.json` with its OWN schema + validator** (`tools/validate_greybox.py`, added to the pre-commit hook). **`constants.json`/`schema`/`teams.json` are NOT edited in Phase 2** (banned — they belong to the frozen Phase 1 contract).

中文摘要：Unity 唔可以直接引用 `prototype/` 個庫。**橋接（鎖定）**：MatchEngine 加 build 用嘅 `netstandard2.1` 目標（**只係打包改動，公開API同數學凍結**，改完要重跑 Phase 1 的 9/9 測試＋validate 確認數字一樣，唔一樣就還原）；build 出 DLL 放 `Assets/Plugins/`，用 asmdef 引用。Phase 1 合約檔逐字節不變。引擎 config 用**有腳本嘅同步**由 `design/` 複製入 `StreamingAssets/`。灰盒可調數值放**獨立 `greybox.json`＋自己嘅 schema／validator**；**Phase 2 唔准改 constants.json/schema/teams.json**。

**步驟一已解決（更新原本嘅計劃）**：原本打算加 `System.Text.Json` package，**否決咗**。引擎其實**從來冇 call 過 JsonSerializer**，啲 `[JsonPropertyName]` 淨係做標籤，但個 reference 會拉埋 **8 個附屬 DLL** 入 `Plugins/`，其中幾個 Unity 本身已經有 → 撞版；再加 IL2CPP 剝碼風險。改為喺 `NetstandardCompat.cs` 用 `#if NETSTANDARD2_1` 自己寫個 shim，**個 DLL 而家零依賴**（publish 由 9 個檔案變 1 個）。已重跑 Phase 1：9/9 測試、blind −8.29%、informed +44.09%、even-money 56.36%（2014 注）——**數字完全一樣**。**往後規矩：Unity DLL 一有依賴就係 bug，用 shim 解決，唔好加 package。**

**Unity 讀 config（已定案）**：唔可以用 `System.Text.Json`（要保持零依賴），亦**唔可以用 Unity 內置 `JsonUtility`**——因為 `formation_mods` 等係**字典**，`JsonUtility` 根本做唔到。**用 `com.unity.nuget.newtonsoft-json`**（Unity 官方 package，唔會撞版）。啲 key 命名混合（snake_case／camelCase／大寫），冇單一 naming strategy 得，**建議**寫個細 resolver 讀返引擎現有嘅 `[JsonPropertyName]`（唯一真相來源，唔會走樣）。**橋接對數檢查（Gate 2 必做）**：喺 Unity 跑嗰場固定種子 fixture，比分同派彩要同 Console **一模一樣**——config 讀錯就一定過唔到呢關。

## Gate-2 cash (deterministic — so every session reaches the walk)

The walk, not betting luck, is what Phase 2 tests, so the cash source is deterministic:
- The bet shop offers **one canned Gate-2 fixture** resolved by the real MatchEngine with a **fixed seed chosen so the offered bet wins**, paying a fixed amount (≈$2,000, in `greybox.json`). Placing it always yields the carry-cash on the first try.
- This still exercises the real engine (proving the library bridge) but removes variance from the Gate-2 test. Full skill-based betting returns in Phase 3.

中文摘要：Phase 2 測嘅係嗰段路唔係賭運，所以現金來源確定：投注站有**一場固定 fixture**，用真引擎＋**選定種子令嗰注一定中**，固定派彩（約$2000，放 greybox.json）。照樣行真引擎（證明橋接），但去除變數。真正靠技術嘅落注留返 Phase 3。

## Detection & chase model (concrete numbers — all in `greybox.json`)

One detection model, no guessing (starting values, tunable):
- **Vision cone:** half-angle `35°`, range `18 m`, requires **unbroken line-of-sight** (raycast; must NOT see through walls — a Gate-2 supporting check).
- **Time-to-detect:** player must be continuously in the cone + LOS for `0.8 s` at $0 heat; this fill-time **shrinks as cash-carry heat rises** (that's how carrying money makes you more likely to be caught). Partial fill decays when LOS breaks.
- **Chase:** on full detection the patroller pursues at `run-speed × 1.05`.
- **Chase-break rule:** if the patroller has no LOS to the player for `4.0 s` (e.g. player is inside a hiding spot / around a corner), it loses the target and returns to patrol. Re-acquire if LOS regained before the timer expires.

中文摘要：一個偵測模型（數值放 greybox.json、可調）：視野錐半角35°、射程18m、要**無阻擋視線**（raycast，唔可以穿牆——Gate 2 檢查項）；連續喺錐內+視線`0.8秒`先鎖定，帶現金heat越高呢個時間越短（＝帶錢越易被捉），斷視線會衰減。鎖定後以跑速×1.05追。**斷追規則**：patroller 連續`4秒`睇唔到玩家就甩目標返去巡邏，計時內重見則重新鎖定。

## The tension loop (one playable minute)

1. Start at the shelter; walk to the bet shop through the corridor (safe — little cash yet).
2. Place the canned bet → win → now carrying ≈$2,000 on-body.
3. The cash-carry heat begins ticking; the patroller's detect-time shrinks the longer you're outdoors over the threshold.
4. Walk home to stash the cash. The patroller is between you and the shelter. Choose route / hide / time your movement past the cone.
5. If detected → chase → stop → the three-way choice (with the visible debt ledger updating on pay+vig).
6. Make it home → stash → safe → the relief beat. **That round-trip, felt, IS Gate 2.**

## Gate 2 — pass/fail (felt-experience gate + objective supporting checks)

**Primary (playtest — with a fixed script + rubric, so it isn't gameable):**
- Give 3–5 not-polite testers a **fixed script**: "collect the payout, get the cash home to the stash." No other instruction.
- After **the first attempt**, collect a **forced 1–5 tension rating** ("how tense was carrying the cash home?") and one free-text moment.
- **Pass rubric:** median first-run tension ≥ 4/5 AND ≥ half the testers unprompted describe a specific tension moment (waiting out the patrol, picking a route, a near-miss). Voluntary replay is a *secondary* signal only (it can be caused by confusion/loss, so it does not gate).

**Objective supporting checks (automated + observed — so "it feels fine" can't hide a broken system):**
- **Automated seeded corridor simulation:** a headless/PlayMode test runs **many** corridor traversals at $0 and at $2,000 cash and asserts a **statistically higher mean stop-rate at $2,000** (not a couple of manual walks).
- **Observed (screenshot/clip):** the patroller detects via cone + LOS and **not through walls**; a completed hide **breaks the chase** (the 4 s rule); the catch stop presents all three choices and each resolves correctly (bribe deducts cash; pay+vig moves cash to debt with the vig cut and the ledger visibly drops; fight-flee rolls escape).

**Fail response:** if the walk is boring even with cash at stake, fix the tension (patrol pressure, route design, detect-time curve) BEFORE building anything new. Do not build Phase 3 to escape a failed Gate 2.

中文摘要：Gate 2＝體感關卡＋客觀輔助檢查。**主要（試玩，有固定腳本＋評分準則，防走數）**：3–5個唔客氣測試者，固定指示「攞彩金、帶現金返宿舍收好」；**第一次之後**收集強制1–5緊張評分＋一句自由描述。**過關準則**：首次緊張中位數≥4/5 且 至少一半人未提示就講出具體緊張時刻；自願重玩只作次要信號（可能因為蒙查查／輸咗，唔用��嚟過關）。**客觀檢查**：自動化種子模擬跑**大量**走廊來回，$0 vs $2000 斷言$2000平均被截率**統計上更高**；觀察（截圖／片）——巡邏靠錐＋視線（唔穿牆）、匿藏斷追（4秒規則）、三選一各自結算正確（賄賂扣現金；pay+vig 入債＋抽水且債數可見下降；打逃 roll 脫身）。

## Verification (per workflow.md — previewable Unity, so DRIVE it)

- **Automated (Codex writes, run locally):** EditMode/PlayMode tests for pure logic — detect-time-vs-cash curve, catch-stop resolution math (bribe/pay+vig/flee), stash-vs-carried state, `EngineConfig` loads from StreamingAssets and the canned bet resolves to the expected payout, and the seeded corridor stop-rate simulation.
- **Play verification (the real proof):** run the build; screenshot/observe the three observed checks above. Non-visual pieces end with a logged expected-vs-actual.
- **Editor setup checklist (mandatory Codex deliverable — because a non-coder does the GUI and Codex can't click):** for every setup step, Codex produces a literal checklist naming each **GameObject, component, tag, layer, serialized inspector field + value, and the expected screenshot** after that step. Editor mistakes must be catchable by comparison, not mistaken for code bugs.
- **Commit discipline:** one feature per commit; `good-YYYYMMDD-HHMM` tags; `.unity` scene / prefab churn committed separately from script features.
- **CI:** **out of scope for Gate 2.** Unity batch-mode CI (runner, editor license, `-runTests` invocation) is a **separate follow-up task after the walk is proven locally** — it is not required to reach or pass Gate 2. The Python constants/greybox validators keep running in the existing CI.

中文摘要：可預覽 Unity，要**親自駕駛驗證**。自動測試（Codex 寫、本地跑）：detect-time vs 現金曲線、被截結算數學、收藏/攜帶狀態、由 StreamingAssets 載 config 並結算固定注、種子走廊被截率模擬。試玩證明：三項觀察檢查截圖。**編輯器設定清單（Codex 必交，因為非程式員做 GUI、Codex 撳唔到）**：每步列出每個 GameObject／component／tag／layer／inspector 欄位值＋預期截圖。**CI 唔屬 Gate 2**：Unity batch CI 係後續任務；Python validator 照跑。

## Key decisions & tradeoffs

- **Minimal greybox (3 buildings + one corridor), exactly one patroller** — scope is the #1 solo-Unity risk; the walk needs no more to be felt.
- **DLL-in-Plugins bridge + build-only netstandard2.1 multi-target, API/behaviour frozen and re-verified** — the only viable way to reuse the engine in Unity; the Phase 1 contract is protected by re-running its proof after the packaging change.
- **Separate `greybox.json`; `constants.json`/schema/teams banned from edits** — closes the frozen-contract-violation door the draft accidentally opened.
- **Deterministic canned Gate-2 bet** — the walk is the test, not betting luck; still exercises the real engine.
- **Visible debt ledger in the slice** — makes pay+vig a concrete state change, not money deletion.
- **Concrete detection/chase numbers in config** — removes Codex guesswork and makes the stealth tunable.
- **Gate 2 = scripted playtest + forced rating + automated stop-rate sim** — subjective tension judged rigorously; objective mechanics can't hide behind vibes.
- **Unity CI deferred out of Gate 2** — first prove the walk locally; CI is its own task.

## Risks / open questions

- **First real Unity workflow test:** scene wiring is Phil's hands via the mandatory checklist; if it proves a recurring blocker, this is the gate at which the deferred Godot option was to be reconsidered (Phase 0).
- **netstandard2.1 compatibility:** the engine must not use net8.0-only APIs; if it does, they must be swapped for netstandard2.1-safe equivalents **without changing behaviour** (re-verified by the Phase 1 proof). Flag any such change explicitly.
- **StreamingAssets sync drift:** mitigated by the scripted sync + documentation; still the classic footgun.
- **Vision-cone-through-walls:** the classic greybox bug; an explicit Gate-2 observed check.
- **First-person feel:** walk/run/look speeds are config-driven for fast iteration.

## Out of scope (Phase 2)

- Everything under "DEFERRED" above, including Unity CI (a follow-up task).
- **Any change to the Phase 1 match-engine public API, behaviour, or betting math.** Permitted engine-project changes are limited to build metadata / package refs / compile-time shims needed to compile `netstandard2.1`, verified behaviour-identical by re-running the Phase 1 proof (identical Gate-1 numbers). Any semantic change requires separate re-review.
- Art, audio, animation, narrative integration, and the full world-design.md world (this slice draws only the three buildings + corridor it needs).

## Amendment — 2026-07-17 (world-design.md revision; see its change table + §10)

The design session revised `docs/world-design.md` (251 → 532 lines). Four changes land on this spec; where they conflict with the body above, **this amendment wins**.

**1. Flee is fully manual — the seeded escape roll is DEAD (wd:§4.1.2).**
The catch-stop's third choice ("fight-flee (flee = a seeded escape roll)") is replaced: choosing flee starts a **real chase**. Chase model: while the patroller has line of sight it pursues (run-speed × `chase_speed_mult`); on losing sight it walks to the **last point it saw the player** and scans; **`search_scan_duration_s` (30 s) without a sighting ends the chase** and it returns to patrol; re-sighting during the scan re-acquires. This **replaces the 4 s LOS chase-break rule** in §Detection. A probability roll here would break wd:§6.3 (stealth is full-manual vision-based) and pillar 1 (every bet is walked). The Gate-2 observed check "fight-flee rolls escape" becomes: **fight-flee starts a chase; a completed hide (scan expires unseen) ends pursuit; the catch consequences apply only if physically caught.**

**2. Catch-stop pricing is now formula, not flat (wd:§4.1.1 via greybox.json v2).**
`fine = heat × fine_per_heat_point ($20)`, `bribe = fine × bribe_fine_multiplier (2)`, **cash only** (wd:§3.3). The bribe's designed gate — that collector's connection/corruptibility (wd:§6.4) — does not exist in the greybox, so the bribe is offered unconditionally: **marked scaffolding** (`bribe_always_available: true`, provenance `SCAFFOLD wd:§6.4`). The pay-toward-debt **vig** remains `constants.json catch_vig 0.3`, which world-design **never ratified** — flagged to Phil (wd:§10 item 4); treat as provisional.

**3. Map requirements (wd:§2.2.1–2.2.2) — geometry grows, mechanics do not.**
- **Real scale from day one: ~700 m corner-to-corner, buildings at real size.** Walk **2.5 m/s**, sprint **5 m/s** (game speed; both in greybox.json, doc-locked).
- **Six shop-shaped buildings must stand in the map from day one:** Fat Keung's 3 trading bet shops + **3 empty shells ("for lease"), one per non-rural zone** — non-deferrable; retrofitting shells into a finished map is expensive. Shells are grey boxes with a marker, nothing more.
- **Zone boundaries are gameplay-critical** (district-scoped income, wd:§3.12; the risk mirror): greybox = **flat colour per district, hard edges at boundary streets**, 4 zones per wd:§2.2.
- **Scope guard:** the greybox's *systemic* content is unchanged — one patroller, one patrolled corridor (docks), shelter + bet shop + gang landmark, the tension loop. The other buildings are inert grey boxes; the map is simply built at its real footprint so nothing is retrofitted later. §Scope's "3 grey buildings" becomes "the 3 *systemic* buildings inside the real-scale zoned map".

**4. greybox.json process rule (wd:§10).**
Every value carries a `_provenance` entry — doc citation (`wd:§x` / `p2:x`), `TUNE`, `SCAFFOLD <missing gate>`, or `verified:<date>` — enforced both directions by `tools/validate_greybox.py`. Unmarked scaffolding becomes the design; that is what the marker prevents.

中文摘要（2026-07-17修訂）：①**flee冇骰仔**——揀「打逃」＝真追逐：見到你就追,追失咗行去最後見到你嗰點掃描30秒,冇再見到就放棄（wd:§4.1.2）;取代原本4秒斷追規則。②罰款/賄賂改公式:fine=heat×$20,bribe=fine×2,現金only;賄賂應有嘅人脈閘greybox未有→**明標鷹架**;vig 0.3未獲設計文件批准→已上報Phil。③地圖:一開始就起真尺寸（700m對角、行2.5跑5 m/s）、**六座舖形建築**（肥強3間營業+3間吉舖每非郊區1間,唔可以遲啲先加）、四區平色硬邊界;**系統內容不變**——照樣一隻巡邏、一條走廊、嗰個緊張循環。④greybox.json每個值必須標出處/TUNE/鷹架/驗證,validator雙向強制。
