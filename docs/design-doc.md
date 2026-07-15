# Ball Knowledge Design Doc v2

**Working title:** Ball Knowledge (中文《走數》) · 90s fictional city, pre-mobile-phone era · Football (soccer) only · Single-player immersive sim.

**High concept:** You owe the wrong people a lot of money. The only way out is the football betting shop across town — but the men you owe are on the streets between you and it, and every dollar in your pocket is theirs if they catch you. A survival sim about debt, information, and walking home the long way.

中文摘要：90年代虛構城市，還未有手機。你欠下巨債，唯一出路是城另一頭的足球投注站，但收數佬就在路上。一個關於債務、情報、同繞遠路回家的生存模擬。

## Three-Act Structure

The game escalates through three acts, each climbing one rung of the information ladder:

1. **Act 1 — The Bettor.** Deep in debt, you gather information the odds board doesn't reflect yet, walk your bets to the shop, and dodge the collectors hunting you. You *consume* odds.
2. **Act 2 — The Bookie.** Debt cleared and trust earned, you move behind the counter: setting lines, courting client archetypes (the whale, the sharp, the degenerate regular), employing the people who once shared the shelter with you. You *set* odds.
3. **Act 3 — The Fixer.** With money and connections you reach the sport itself: stadium boxes, players, teams, eventually the league. You *control* the outcomes behind the odds.

Acts 2 and 3 remain paper designs until Act 1 is proven (see the roadmap's PM rules). Systems are designed to invert across acts rather than be discarded — the loan-shark pressure you fled in Act 1 becomes the collection network you operate in Act 2; the intel sources you paid become the sharps you fear.

中文摘要：三幕式——賭客（接受賠率）→ 莊家（制定賠率）→ 操盤者（控制賠率背後的結果）。系統跨幕反轉重用，不會棄置。

## Design Pillars

Every feature must pass all four:

1. **Every bet is walked.** Nothing happens through menus alone — betting, intel, repayment all require physically going somewhere and carrying something.
2. **Information is currency.** Match outcomes are partly knowable. Player skill = collecting what the odds board doesn't know yet.
3. **Debt is the clock.** All tension flows from one number and one weekly deadline. No artificial timers.
4. **Nothing buys better luck.** Progression buys safety, information, and reward size — never win probability.

中文摘要：四大支柱——每一注都用腳走出來；情報就是貨幣；債務就是時鐘；什麼都買不到更好的運氣。

## Shelter Start and Housing Ladder

The player starts in a homeless shelter. This is a genuine trade-off, not just a poverty skin:

- **Shelter:** no fixed address — collectors cannot find you at night. Costs: 10pm curfew (miss it → sleep rough: energy cap −30%, cash-theft roll), no stash, poor sleep quality (lower energy cap). Free but thin daily meal (anti-death-spiral floor).
- **Housing ladder:** shelter → rented bed → room above the tea café → own flat. Each tier adds stash capacity, sleep quality, and cooking — **but a fixed address means collectors know where you live.** Missed payments escalate to door ambushes.

Success raises exposure exactly as cash grows. The shelter also houses the recruitable NPC cast whose loyalty is earned in Act 1 and employed in Act 2.

中文摘要：露宿者中心開局。中心＝收數佬夜晚搵你唔到，但有宵禁、無藏錢點；自置居所＝有藏錢點但地址曝光。住屋階梯升級同時提升風險。

## Catch and Vig Rule

If collectors catch the player carrying cash: they confiscate **all of it**; **70% reduces the debt, 30% is the vig** (`catch_vig` in `design/constants.json`). Design intent:

- Converts failure into *bad progress*, not game over.
- Tuned so voluntary repayment always dominates — the vig keeps "getting caught on purpose" a legal but clearly inferior banking strategy, which is emergent play, not an exploit.
- Debt consequences ladder: miss one weekly payment → heat rises permanently; miss two → hospital (lose 3 days, interest still accrues); debt exceeds 2× original → game over.

中文摘要：被抓＝現金全沒收，七成扣債、三成抽水。失敗變成「劣質進度」而非死亡。主動還款永遠更划算。

## Energy as Daily Budget

One meter, deep consequences:

- **Energy is the daily action budget.** Every action costs energy; empty = forced home. Sleep sets tomorrow's budget; *when* you sleep sets your schedule.
- **Food is folded into energy** — no separate hunger meter. Food buys mid-day energy with texture: instant noodles (cheap, weak), tea café meal (mid — doubles as an intel source), proper dinner (expensive, big refill). Hunger never kills; it shrinks the day.
- **Punish with opportunity cost, never death.** Broke + tired = a small bad day, not a spiral.

中文摘要：一條精力數值＝每日行動預算；食物併入精力，不設飢餓條。懲罰用機會成本，永不用死亡。

## Training Ground vs Pub-Night Intel Collision

The information economy's signature dilemma:

- The **training ground is far away** — highest time/energy cost and route risk, most reliable intel (~80% reveal rate, occasionally closed so it never becomes a mandatory chore). Match-day warm-up viewing requires sleeping early and a dawn round trip.
- The **pub is where players drink at night** — staying late buys gossip but destroys the early start.
- **You can never do both.** Sleep early for the warm-up or stay late for the pub. This single collision is the model for all intel-source design.

Intel sources are tiered by cost and reliability (café gossip ~50%, groundskeeper ~75%, insider ~90% with limited uses), and are always *fresher than the odds board*, which updates on the newspaper cycle — one day stale. That gap is the player's entire edge.

中文摘要：訓練場遠而可靠（要早睡早起），酒吧夜晚有料（要熬夜）——永遠二選一。情報永遠比賠率板新鮮一天，這個時間差就是玩家的全部優勢。

## Relationship NPCs Across Acts 2-3

Relationships are long-term investments that glue the three acts into one game:

- **The ball kid** (met at the training ground) and **the shelter mate** are Act 1's two relationship NPCs — favor counters with 3–4 scripted moments each and small perks (a tipped factor; a patrol warning).
- They carry forward: the shelter mate becomes Act 2's first employee; the ball kid grows into an Act 3 dressing-room connection.
- **Rule: opportunistic, never grindy.** Favor moments occur inside things the player already does (sharing food, walking someone through shark turf) — no standing-still friendship meter.

中文摘要：執波仔與中心朋友是第一幕的兩個關係NPC，之後成為第二、三幕的伙記與更衣室人脈。人情要順手而為，不刷數值。

## In-Fiction Promo Tokens

Era-appropriate paper vouchers the betting shop hands out — the industry's real hooks, presented honestly:

- **Free bet voucher** (stake refunded on loss — given after losing streaks, the classic hook), **odds boost stamp** (+20% payout on one slip — loyalty reward), **insurance slip** (half stake back — bought at the counter), **accumulator ticket** (unlocks combo bets — shop-trust milestone).
- **Rules:** tokens amplify reward, never probability (pillar 4). Token supply must pass the Phase 1 validation harness — blind betting *with* tokens still loses long-term. In Act 2 the player hands these same vouchers to their own clients, completing the lesson.
- **No real-money purchases. Ever.** (Permanently out of scope.)

中文摘要：遊戲內紙質促銷券（免費注、賠率加成、保險單、過關飛）——只放大回報、永不改變機率，且必須通過驗證數學。永遠沒有真錢課金。

## Global Event-Broadcasting Calendar

The calendar is a single global system that broadcasts events; every other system listens and none keeps its own clock:

- In-game clock with day phases (morning/afternoon/evening/night); shop hours, curfew 10pm, training 9am on match day, matches Saturday 3pm.
- **The week is the heartbeat:** gather intel → bet → sweat the radio → Settling Day (weekly debt payment).
- Season structure: 8-team fictional league, double round-robin (14 rounds); the vertical slice uses a 10-week season.

中文摘要：日曆是唯一的全域廣播系統（「而家係週六三點」→ 球場開門、電台開播），其他系統只負責收聽。每週節奏＝遊戲心跳。

## Poisson Match Engine and Bookmaker Odds

The load-bearing system, prototyped text-only in Phase 1 before anything else:

- **Teams:** 8 fictional teams; visible ATK/DEF stats, hidden weekly-drifting form, 0–3 hidden per-match factors (keeper drinking, star injured, waterlogged pitch, dressing-room fight).
- **Goal model:** per-team expected goals `λ = league_avg_goals × (ATK / opp DEF) × form + home_advantage`, factors modify λ; goals sampled from Poisson(λ) and scattered across 90 simulated minutes — that event list **is** the radio commentary script.
- **Odds:** the shop computes 1X2 / Asian handicap / correct score from the Poisson score matrix using **yesterday's public information only** (no hidden factors), multiplied by `bookmaker_overround` (1.10) so blind betting slowly loses. The correct-score matrix provides the jackpot bet for free.
- **Validation gate (pass/fail):** 1,000 simulated bets each way — blind ≈ `blind_roi` (−8%); fully informed hits `informed_win_rate_min`–`max` (55–60%) with clearly positive returns. If those numbers hold, the intel economy works mathematically before a single street is built.
- All numbers live in `design/constants.json` — never hard-coded.

中文摘要：泊松入球模型＋含抽水的賠率（只用昨日公開資訊計算）。驗證標準：盲賭約輸8%、全情報55–60%勝率。所有數字放調參檔，永不寫死。

## Narrative Authoring Track (parallel to engine work)

The story, characters, unique events, dialogue, quests, and relationship/choice branching are the designer's (Phil's) primary creative contribution and the game's differentiator. This work is **separated into two stages** so it can start early without jumping the engine gates:

- **Authoring (starts now, Phase 1 onward):** written in **Twine** — free, browser-based, visual boxes-and-arrows branching, no code and no Unity required. The designer *sees* the whole story map and can grow it freely on paper. Source lives in `narrative/` as `.twee` text files (plain text, git-diffable, version-controlled like everything else). This is fuel and costs nothing to expand.
- **Wiring into the game (Phase 3–5):** the authored branches are integrated into Unity through a **purpose-built narrative tool** — evaluation favors **articy:draft X** (visual node editor for characters/dialogue/quests/relationships with a Unity importer, closest to the designer's mental model) or a writer-first scripting tool (Ink / Yarn Spinner). **xNode was considered and rejected** — it is a generic node-graph framework requiring a programmer to build the entire dialogue/quest runtime in C#, wrong for a non-coder + Codex workflow. Final tool choice is a Phase 3 decision made deliberately, not in passing.

**Scope guardrail:** writing may grow freely at any time, but *integrating* narrative into Unity stays behind Gate 3 (the core day loop must be proven fun first). A rich branching quest tree is worthless if one day of the core loop isn't fun yet.

中文摘要：故事／角色／對白／任務／關係分支是設計者（Phil）的主要創作貢獻與遊戲賣點，分兩階段：**創作**（現在起，用免費視覺化的 Twine 寫，無需代碼或 Unity，原始檔放 `narrative/` 的 `.twee`）；**接入遊戲**（第3–5階段，用專門敘事工具，傾向 articy:draft X 或 Ink／Yarn Spinner；xNode 已評估並否決，因為它需要程式員自建系統）。護城河守則：寫作可隨時擴張，但接入 Unity 要留在第三關卡之後——核心循環未證明好玩前，再靚的任務樹都等於零。
