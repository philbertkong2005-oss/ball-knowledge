# Ball Knowledge Design Doc v2

**Working title:** Ball Knowledge (中文《走數》) · 90s fictional city, pre-mobile-phone era · Football (soccer) only · Single-player immersive sim.

**High concept:** You owe the wrong people a lot of money. The only way out is the football betting shop across town — but the men you owe are on the streets between you and it, and every dollar in your pocket is theirs if they catch you. A survival sim about debt, information, and walking home the long way.

中文摘要：90年代虛構城市，還未有手機。你欠下巨債，唯一出路是城另一頭的足球投注站，但收數佬就在路上。一個關於債務、情報、同繞遠路回家的生存模擬。

> ## 📌 Document map — read this first
>
> - **[`docs/world-design.md`](world-design.md) is the authoritative source for all world, economy, item, crime/risk, action, and intel systems** (Phase 3+ life-sim). Where this document conflicts with it, **world-design.md wins.** The sections below are kept only as short summaries that point at it.
> - **[`docs/phase1-match-engine.md`](phase1-match-engine.md) is the authoritative, frozen contract for the match engine** (Phase 1, Gate 1 passed). **`world-design.md` does NOT alter it** in any way — the two documents are deliberately disjoint.
>
> 中文摘要：世界／經濟／物品／犯罪／行動／情報系統以 [`world-design.md`](world-design.md) 為準（衝突時它勝出）；比賽引擎以 [`phase1-match-engine.md`](phase1-match-engine.md) 為準，且 world-design 完全不改動它。

## Three-Act Structure

The game escalates through three acts. **Identity evolution is the selling point** — each act the player's primary identity genuinely changes. **Betting is retained in all three acts** (secondary verb in Acts 2–3; in Act 3 it is the payoff step of every fix). See [`world-design.md` §1](world-design.md) for the authoritative statement.

1. **Act 1 — The Bettor.** Deep in debt, you gather information the odds board doesn't reflect yet, walk your bets to the shop, and dodge the collectors hunting you. You *consume* odds. *(Fun: detective info-edge + survival tension — you are prey.)*
2. **Act 2 — The Bookie.** You move behind the counter: setting lines, balancing the book against sharps, employing people you know. You *set* odds. *(Fun: the "house always wins" power flip — you are predator.)* **Act 2 is a PURE bookie act — footballers and core team members are untouchable; "changing the truth" is saved whole for Act 3.**
3. **Act 3 — The Fixer.** With connections you reach the sport itself. You *control* the outcomes behind the odds. *(Fun: heist-like fixing operations.)*

**Act 2 entry = triple gate** (supersedes the older "debt cleared and trust earned"): ① debt principal cleared **+** ② connection levels with key NPCs reached **+** ③ player XP level reached. "Trust" is not a separate concept — it is unified into the NPC connection system ([`world-design.md` §1, §3.8](world-design.md)). Money alone can never open Act 2.

Acts 2 and 3 remain paper designs until Act 1 is proven (see the roadmap's PM rules). Systems are designed to invert across acts rather than be discarded — the loan-shark pressure you fled in Act 1 becomes the collection network you operate in Act 2; the intel sources you paid become the sharps you fear.

中文摘要：三幕式——賭客（獵物）→ 莊家（獵人）→ 操盤者。落注三幕保留。**第2幕係純莊家幕，球員不可掂**。入第2幕＝三閘（債清＋關鍵NPC關係＋等級），取代舊有嘅「債清＋信任」；「信任」已統一入NPC關係系統。詳見 world-design §1。

## Design Pillars

Every feature must pass all four:

1. **Every bet is walked.** Nothing happens through menus alone — betting, intel, repayment all require physically going somewhere and carrying something.
2. **Information is currency.** Match outcomes are partly knowable. Player skill = collecting what the odds board doesn't know yet.
3. **Debt is the clock.** All tension flows from one number and one recurring deadline — a **monthly** minimum payment against a principal of ≈$100,000, paid in cash ([`world-design.md` §3.5](world-design.md)). No artificial timers.
4. **Nothing buys better luck.** Progression buys safety, information, and reward size — never win probability.

中文摘要：四大支柱——每一注都用腳走出來；情報就是貨幣；債務就是時鐘；什麼都買不到更好的運氣。

## Shelter Start and Housing Ladder

The player starts in a homeless shelter (shelter #1, residential fringe). The core trade-off — **success raises exposure exactly as cash grows** — is retained. Authoritative detail: **[`world-design.md` §2.2 (zones), §4.2 (stakeouts), §5 (furniture/stash)](world-design.md)**.

- **Sleeping is now rotation gameplay (supersedes "the shelter hides you"):** gang patrols concentrate near your **fixed address and sleep spots**, and **sleeping in the same spot 2+ nights = stakeout.** With **two shelters**, hotels, and camping available, rotating where you sleep is the actual mechanic.
- **Housing ladder:** rentable rooms in the residential fringe → owned apartment/house (**card-only** — you must launder to buy durable assets, [§3.3](world-design.md)). Tiers add stash capacity (drawers), sleep quality (bed), cooking, and food storage via **furniture** ([§5](world-design.md)).
- **On-body vs stash are distinct states** ([§5](world-design.md)): carried = searchable/robbable; stashed = safe but must be fetched. This, not the shelter, is the real cash-safety system.

中文摘要：露宿者中心（#1，住宅區）開局，「越有錢越暴露」核心取捨保留。**瞓覺已變輪換玩法**（取代「中心令你隱形」）：黑幫巡邏集中喺你固定地址同瞓覺點，**同一點瞓2晚＝被伏**；兩間中心＋酒店＋露營＝輪換。住屋階梯：租房→自置（**只能刷卡**，要洗錢先買到）。身上vs收藏係兩種狀態。詳見 world-design §2.2／§4.2／§5。

## Catch and Vig Rule (now part of the dual-heat system)

The catch/vig rule survives, but its surrounding systems are **superseded by [`world-design.md` §4](world-design.md)**, which replaces the old single-heat/weekly-payment ladder with **two independent predators: police heat (cares about your crimes) and gang heat (cares about your money).**

- **The rule itself (unchanged):** cash confiscated toward the debt **+ vig** (`catch_vig` = 0.30 in `design/constants.json`). Failure = *bad progress*, never game over; the vig keeps "getting caught on purpose" a legal but clearly inferior banking strategy.
- **Gang heat fuel ([§4.2](world-design.md)):** ① days overdue on the **monthly** payment, ② carrying **>$500 cash outdoors, +1/hour**. Independent components.
- **A gang stop is now a choice, not an automatic seizure:** **bribe** / **hand over all on-body cash toward debt + vig** / **fight back & flee.** Losing the fight = all cash to debt + vig, health loss, out cold half a day, wake outside the hospital.
- **Superseded:** the old "miss one weekly payment → permanent heat; miss two → hospital; debt > 2× → game over" ladder. Overdue now adds **+10% to that missed installment** ([§3.5](world-design.md)); gang heat clears instantly on clearing the overdue amount, and the cash component decays −0.25/hour below $500.

中文摘要：抽水規則保留（現金入債＋抽水，`catch_vig`=0.30），但周邊系統以 world-design §4 為準：**雙heat（警察管你犯罪、黑幫管你錢）**。黑幫heat燃料＝逾期日數＋戶外帶現金>$500每鐘+1。被截係**三選一**（賄賂／全數入債＋抽水／反抗逃走），唔再係自動沒收。舊有「每週走數階梯／債超2倍gameover」已取代。

## Survival Bars (was "Energy as Daily Budget")

⚠️ **Superseded.** An earlier draft specified a *single* energy meter with "food folded into energy — no separate hunger meter." **[`world-design.md` §5 and §4.3](world-design.md) supersede this: there are THREE bars — Health, Energy, Hunger.**

- **Energy** — still the daily action budget; *when* you sleep sets your schedule. Sleep restores energy **and** health. Coffee / energy drinks / cigarettes = temporary boosts.
- **Hunger** — its own bar, fed by food (incl. cooking: 1 veg + 1 meat + 1 carb at ~half market price; food expires). **Hunger > 80 grants slow health regen.**
- **Health** — restored by sleep and medicine.
- **Failure states** ([§4.3](world-design.md)): Energy = 0 → forced sleep on the spot, 3h skip, 50% chance of losing 10% of on-body cash. Health = 0 → out half a day, wake at hospital, pay the bill.
- **Design intent retained:** punish with opportunity cost, not death spirals.

中文摘要：⚠️已被取代。舊稿寫「單一精力條、食物併入精力、無飢餓條」，現以 world-design §5／§4.3 為準：**三條bar——健康、精力、肚餓**。肚餓>80慢回血；瞓覺回精力兼回血；精力=0強制瞓（跳3小時、五成機會失一成身上現金）；健康=0昏半日入醫院付帳。

## Intel System (three-tier ladder)

The pillar system. **Authoritative spec: [`world-design.md` §7](world-design.md)** — this replaces the older flat "café 50% / groundskeeper 75% / insider 90%" source list with a **three-tier ladder by information lifespan**:

| Tier | Content | Source | Lifespan |
|---|---|---|---|
| ① **Durable** | team base profile, **qualitative only** (strong/fair/poor; striker-reliant…) — never raw numbers | **attending matches at the stadium** | the season |
| ② **Semi-durable** | fixtures, streaks, today's formation | newspaper, bar talk, broadcasts | days–weeks |
| ③ **Perishable** | specific player condition (hungover keeper, knocked striker) | supporters'-bar insiders, training-ground visits | one match |

- **Knowledge = geography.** Scouting is progressive (each watch sharpens the read) and **away watches are capped — a team is only fully understood by attending their home ground**, so travel is intel investment.
- **Broadcast boundary:** radio/TV give key moments + today's formation + recent form — **never playstyle or height.** Tier ① is stadium-exclusive; the ticket buys what the airwaves can't.
- **The signature time-cost collision is retained:** the far training ground (perishable tier, dawn trip) vs the supporters' bar at night. You can never do both — that collision is still the model for intel-source design.
- Intel is always *fresher than the odds board* (newspaper cycle = one day stale). **That gap is the player's entire edge**, and it is also **social currency** — a tip can be bet, sold, or gifted ([§3.8](world-design.md)).

中文摘要：情報三層階梯（以 world-design §7 為準，取代舊有平面來源清單）：①恆久（**去球場睇波**，只得質性描述，逐場磨利，**作客有上限——去佢主場先摸得透**）②中期（報紙／酒吧／廣播：賽程、狀態、陣式）③即棄（insider／訓練場：個別球員今場狀態）。廣播唔講playstyle同身高。訓練場vs酒吧嘅時間衝突保留。情報亦係社交貨幣（可賭、可賣、可送）。

## NPC Connections (was "Relationship NPCs")

Relationships are now a **first-class system and the master key to progression**. **Authoritative spec: [`world-design.md` §3.8](world-design.md)**; it also absorbs the old separate notion of "trust."

- **Per-NPC independent ratings.** Higher connection unlocks that NPC's **verbs**: hiring, giving information, accepting bribes, and other activities.
- **Levelled by:** gifting (items, food incl. self-cooked, **intel you gathered**), buying info from them, selling to them, plain engagement. *(Tuning flag: gifts need a daily cap / diminishing returns — anti-spam.)*
- **Connection is load-bearing twice over:** it is **gate ② of the Act 2 triple gate** ([§1](world-design.md)), and **the master key to all Act 3 access** — both the break-in and welcomed-in routes require it, and bribing refs/players only works at high connection ([§6.2](world-design.md)).
- **The ball kid + shelter mate** remain Act 1's named relationship NPCs; the full key-NPC roster for the connection gates is an **open thread** ([§8](world-design.md)).
- **Rule retained: opportunistic, never grindy** — favor moments occur inside things the player already does, not a standing-still friendship meter.

中文摘要：NPC關係已升為一級系統兼進度萬能匙（以 world-design §3.8 為準，並吸收舊有「信任」概念）。每個NPC獨立評分，解鎖該NPC嘅動詞（請人、畀料、收賄）。靠送禮／買料／賣嘢／傾偈提升；**情報本身就係禮物**。關係係第2幕三閘之②，亦係第3幕所有access嘅萬能匙。執波仔＋中心朋友保留；完整關鍵NPC名單未定。

## In-Fiction Promo Tokens

Era-appropriate paper vouchers the betting shop hands out — the industry's real hooks, presented honestly:

- **Free bet voucher** (stake refunded on loss — given after losing streaks, the classic hook), **odds boost stamp** (+20% payout on one slip — loyalty reward), **insurance slip** (half stake back — bought at the counter), **accumulator ticket** (unlocks combo bets — shop-trust milestone).
- **Rules:** tokens amplify reward, never probability (pillar 4 — confirmed unchanged in [`world-design.md` §3.9](world-design.md)). Tokens are a Phase 3 shop feature and are **out of scope for the Phase 1 match engine** — no token model or token validation exists in Phase 1. When tokens are built, their supply must be validated so that blind betting *with* tokens still loses long-term. In Act 2 the player hands these same vouchers to their own clients, completing the lesson.
- **Where you get them ([§3.9](world-design.md)):** the **illegal bookie** (arcade back room, docks) offers profit tokens **regularly**; **legal shops require special means** to obtain them. The grey economy is the generous one — a deliberate pull toward risk.
- **No real-money purchases. Ever.** (Permanently out of scope.)

中文摘要：遊戲內紙質促銷券（免費注、賠率加成、保險單、過關飛）——只放大回報、永不改變機率，且必須通過驗證數學。永遠沒有真錢課金。

## Global Event-Broadcasting Calendar

The calendar is a single global system that broadcasts events; every other system listens and none keeps its own clock:

- In-game clock with day phases (morning/afternoon/evening/night); shop hours, training on match day, matches Saturday 3pm.
- **Real-time pacing ([`world-design.md` §3.6](world-design.md)): 20 real minutes = one 24h in-game day.**
- **The week is the match heartbeat:** gather intel → bet → sweat the radio. **The debt deadline is MONTHLY, not weekly** ([§3.5](world-design.md)) — this supersedes the earlier "weekly Settling Day."
- Season structure: 8-team fictional league, double round-robin (14 rounds); the vertical slice uses a 10-week season. The 8 teams map onto **4 towns × 2 teams, sharing 1 stadium each** ([§2.1](world-design.md)).

中文摘要：日曆是唯一的全域廣播系統，其他系統只負責收聽。**20分鐘真實＝1日遊戲內**。每週＝比賽心跳，但**還債死線係每月，唔係每週**（取代舊有「每週找數日」）。8隊分佈4鎮，每鎮2隊共用1球場。

## Match Engine and Bookmaker Odds

The load-bearing system, built and **Gate 1 PASSED in Phase 1**. **The authoritative, frozen engine contract is [`docs/phase1-match-engine.md`](phase1-match-engine.md)** — that spec supersedes this summary in any conflict (it replaced the earlier simple direct-Poisson model with a fuller causal shot-chain, prices odds from the same engine with factors removed, and defines an exact measurable Gate 1).

> 🔒 **Contract guard:** [`world-design.md`](world-design.md) is a **Phase 3+ life-sim document and does NOT alter this contract in any way.** The two are deliberately disjoint: world-design governs how the player *reaches* information and money; this engine governs how a match resolves and how odds are priced. Any future world/economy decision that appears to require an engine change must be raised explicitly and re-reviewed — never applied silently.

Summary of the approach:

- **Teams & players:** 8 fictional teams (visible ATK/DEF/height, hidden weekly form) with Tier B named players; 0–3 hidden per-match factors are the private edge the player hunts.
- **Causal shot-chain:** effective strengths → shots (Poisson) → shots-on-target (per-shot, shooter drawn first) → goals/saves → scorer & assist attribution; corners parallel. The event timeline **is** the radio script.
- **Odds:** priced from the **same engine run Monte-Carlo with hidden factors removed** (public-information view) × `bookmaker_overround` (1.10), so blind betting loses only the vig.
- **Gate 1 (exact):** a 100,000-fixture Monte-Carlo harness with fixture-clustered bootstrap CIs — blind ROI ≈ `blind_roi` (**−9.09%**, derived from the 1.10 book), informed ROI ≥ +5% (primary), and 55–60% win rate on the near-even-money subset (secondary). See the spec for the full definition.
- All numbers live in `design/constants.json` (schema v2 for the shot-chain model) — never hard-coded.

中文摘要：承重引擎，Phase 1 純文字原型。**權威可建規格見 [`docs/phase1-match-engine.md`](phase1-match-engine.md)，衝突以它為準**（已用完整因果射門鏈取代早期簡單泊松、賠率用同引擎去因素計算、Gate 1 可量度）。賠率＝同引擎去除隱藏因素蒙地卡羅×110%抽水。Gate 1：10萬場、fixture叢集bootstrap CI——盲賭ROI≈−9.09%、有情報ROI≥+5%（主）、近均注子集55–60%勝率（次）。

## Narrative Authoring Track (parallel to engine work)

The story, characters, unique events, dialogue, quests, and relationship/choice branching are the designer's (Phil's) primary creative contribution and the game's differentiator. This work is **separated into two stages** so it can start early without jumping the engine gates:

- **Authoring (starts now, Phase 1 onward):** written in **Twine** — free, browser-based, visual boxes-and-arrows branching, no code and no Unity required. The designer *sees* the whole story map and can grow it freely on paper. Source lives in `narrative/` as `.twee` text files (plain text, git-diffable, version-controlled like everything else). This is fuel and costs nothing to expand.
- **Wiring into the game (Phase 3–5):** the authored branches are integrated into Unity through a **purpose-built narrative tool** — evaluation favors **articy:draft X** (visual node editor for characters/dialogue/quests/relationships with a Unity importer, closest to the designer's mental model) or a writer-first scripting tool (Ink / Yarn Spinner). **xNode was considered and rejected** — it is a generic node-graph framework requiring a programmer to build the entire dialogue/quest runtime in C#, wrong for a non-coder + Codex workflow. Final tool choice is a Phase 3 decision made deliberately, not in passing.

**Scope guardrail:** writing may grow freely at any time, but *integrating* narrative into Unity stays behind Gate 3 (the core day loop must be proven fun first). A rich branching quest tree is worthless if one day of the core loop isn't fun yet.

中文摘要：故事／角色／對白／任務／關係分支是設計者（Phil）的主要創作貢獻與遊戲賣點，分兩階段：**創作**（現在起，用免費視覺化的 Twine 寫，無需代碼或 Unity，原始檔放 `narrative/` 的 `.twee`）；**接入遊戲**（第3–5階段，用專門敘事工具，傾向 articy:draft X 或 Ink／Yarn Spinner；xNode 已評估並否決，因為它需要程式員自建系統）。護城河守則：寫作可隨時擴張，但接入 Unity 要留在第三關卡之後——核心循環未證明好玩前，再靚的任務樹都等於零。

## Match Manipulation / Fixing System (Act 3 core gameplay — built late)

The player can eventually *change* match outcomes, not just predict them: poisoning, bribing a ref/player, assault, stealing kit — causing a major player (striker / playmaker / keeper) to be absent or play in bad form, which swings the result the player then bets on. The mechanical expression of the Act 3 fantasy (predict the truth → set the odds → **change the truth**). **Authoritative spec: [`world-design.md` §6](world-design.md).** This is **Act 3's core gameplay, not side content.**

⚠️ **Superseded — the act escalation changed.** An earlier draft had "real leverage in Act 2 (spike a drink, intimidate)." **That is now cut:**

| Act | Fixing sabotage |
|---|---|
| 1 | **none** — players are visible (training fence, club bus) but unreachable |
| 2 | **NONE** — footballers and core team members are **untouchable**; Act 2 is a pure bookie act |
| 3 | **full** — poison, bribe ref/player, assault, steal kit |

- **Intimidation is cut entirely** as a mechanic ([§1](world-design.md)). "Changing the truth" is saved **whole** for Act 3 — that's what makes the identity flip land.
- **Petty street crime** (assault/pickpocket/mugging vs *ordinary* NPCs) is a separate bucket and *is* available from Act 1 ([§6.1](world-design.md)); it is not fixing.
- **Betting fraud (cheat-slip) is CUT** — if revived, its home is an Act 2 *threat* (clients cheating your book), never a player tool ([§6.1](world-design.md)).

**Access model ([§6.2](world-design.md)) — no greyed-out buttons.** Verbs (attack, steal, poison) are universal; gating is **physical access + consequence**. **Connection is the master key** for both routes: **break-in** (money buys schedules/entry *only with good connections* → lockpick in) or **welcomed in** (a connection introduces you → free access; bribes work only at high connection; refusal = bounty; poison = bounty only if seen).

**Engine principle for Phase 1 (costs nothing, changes nothing):** a hidden factor's *source* may be a player action, not only a random roll — a poisoned striker and a naturally-injured striker hit the match engine identically. **This does not alter the frozen Phase 1 contract** ([`phase1-match-engine.md`](phase1-match-engine.md)); Phase 1 rolls factors randomly only, and the sabotage world plugs in later for free.

**Consistency with pillar 4 ("nothing buys better luck"):** sabotage doesn't buy a better dice roll — it changes the real state the dice rolls from. It must still never trivialise the information game: it stays **risky, costly, and detectable** (bounties, severe caught-consequences), so reading the truth stays the cheaper, safer path.

中文摘要：造馬＝**第3幕核心玩法**（非側內容），以 world-design §6 為準。⚠️**升級路線已改**：第1幕無、**第2幕完全無**（球員／核心成員不可掂，純莊家幕）、第3幕全開；**恐嚇機制完全剷走**，「改變真相」整個留畀第3幕。街頭小罪（對普通NPC）另屬一桶，第1幕起可用。改彩飛已剷。Access靠**人脈萬能匙**：破門 vs 受邀兩條路，唔用灰按鈕。第一階段引擎原則（因素來源可以係玩家行動）**不改動已凍結嘅 Phase 1 合約**。

中文摘要：玩家最終能*改變*而非只預測賽果——落毒、偷球具、恐嚇／襲擊關鍵球員（前鋒／組織核心／門將），令其缺陣或差狀態，大幅左右賽果。這是第三幕「操盤者」幻想的機制化（預測真相→制定賠率→改變真相）。第一階段引擎原則（現在零成本、日後全解鎖）：隱藏因素的來源可以是玩家行動，不只是隨機；落毒的前鋒與自然受傷的前鋒對引擎一模一樣，破壞只是設定現有因素的新來源。兩條強制護欄：①跨幕升級，第一幕不提供②必須高風險高成本可偵測，否則預測遊戲死。與支柱四一致：破壞不是買更好的骰子，是改變骰子所依據的真實狀態，但絕不可令情報遊戲失去意義。
