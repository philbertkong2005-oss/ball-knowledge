# World, Economy, Items, Crime & Actions — Design Decisions

**Status:** LOCKED design decisions from planner Q&A session, 2026-07-15/16.
**Scope:** World map, economy, item system, crime/risk systems, player actions, intel system. These are Phase 3+ life-sim systems — nothing here changes the Phase 1 match engine contract (`phase1-match-engine.md`).
**Relation to `design-doc.md`:** companion document. Where this conflicts with the design doc's older sketches, **this document supersedes** (notable: Act 2 fixing-lite is now cut entirely — see §6).

中文摘要：本文件記錄2026-07-15/16策劃問答鎖定嘅世界、經濟、物品、犯罪、行動、情報系統設計。同design doc衝突時以本文件為準（注意：第2幕造馬-lite已完全剷走）。

---

## 1. Structure & Acts

- **Identity evolution is the selling point (Structure A):** each act the player's primary identity genuinely changes — Bettor → Bookie → Fixer. Betting ability is **retained in all three acts** (secondary verb in Acts 2–3; in Act 3 it is the payoff step of every fix).
- **Act 2 is a pure bookie act.** Footballers and core team members are **untouchable in Act 2**; intimidation was cut as a mechanic entirely. "Changing the truth" is saved whole for Act 3.
- **Act 2 entry = triple gate:** ① debt principal cleared + ② connection levels with key NPCs reached ("trust" is unified into the NPC connection system, not a separate concept) + ③ player XP level reached.
- Act fun statements: Act 1 = detective info-edge + survival tension (prey). Act 2 = "house always wins" power flip, book-balancing vs sharps (predator). Act 3 = heist-like fixing operations, you *make* the truth.

中文摘要：身份進化係賣點（賭客→莊家→造馬者），落注三幕保留。第2幕係純莊家幕，球員不可掂。入幕=三閘：債清+關鍵NPC關係+等級。

---

## 2. World & Map

### 2.1 Region structure
- **4 towns/cities total, 2 teams each, 1 shared stadium each** — maps the full 8-team league onto geography.
- Act 1: one starting town. Acts 2–3 open: +2 small towns + 1 bigger city downtown (the giant Harbour FC lives in the city).
- Design lean: **small and interactive over big and empty.**
- **Act 1 local teams:** Aberdeen Fishermen 香港仔漁民 (supporters' bar A, docks) + Eastport Rovers 東港流浪 (supporters' bar B, residential). Keeper Wong (the demo's hungover keeper) is a local.

### 2.2 Act 1 town — four zones
| Zone | Contents | Risk profile |
|---|---|---|
| **Downtown core** | bank, town hall, police station, hospital, bet shop #1 (flagship), general store #1, pharmacy, barber, newsstand, hotel, general bar, realtor, car dealer, phone booths | police-heavy: safest with cash, deadliest with contraband (search central) |
| **Old harbour / docks** | docks & fish market, black-market fence, Fat Keung's office, arcade + illegal bookie, bet shop #2, shelter #2, supporters' bar A, laundromat (first buyable front candidate) | gang turf: contraband walks free, cash bleeds. Monthly payment day = cash run into the lion's den |
| **Residential fringe** | NPC houses, rentable rooms (housing ladder), general store #2, gas stop #1 + fast food, park (slip-scavenging hotspot, unsafe sleep), shelter #1 (start home), supporters' bar B, phone booths | neutral; max gang heat patrols wherever the player lives |
| **Rural outskirts** | training ground (Act 1 watch through fence → Act 3 infiltrate), stadium (town edge, on bus line), natural area + campground (hunting?? unlocked), general store #3, gas stops #2–3 + fast food, treasure boxes concentrated | no factions but far — trips cost time/energy while cash-heat ticks |

- **Transport spine:** one bus line linking zones; taxis downtown & at gas stops; **one closed metro station "to the city" that opens in Act 2** (expansion foreshadowed in concrete).
- **ATMs on streets across every zone EXCEPT the camp/natural area.** Deposit & withdraw at any ATM once the $500 card exists. The bank building keeps card setup + later business banking.
- Zone symmetry: police zone and gang zone mirror each other — **what you carry decides where is safe.**
- Tension distances: ① overflow cash ↔ your stash (big-win days: the dangerous walk is home, not to the bank), ② stealing is easy, *fencing* means hauling contraband across town, ③ morning training-ground intel ↔ racing back to bet before odds move.
- Counts: 3 bet shops, 2 shelters, 3 general stores, 3 gas stops + fast food, 3 bars total (2 supporters' + 1 general).

中文摘要：4鎮各2隊共用1場。第1幕單鎮四區：downtown（警多）、碼頭（黑幫地頭）、住宅、郊野。帶咩決定邊度安全。ATM遍佈全區除營地。

---

## 3. Economy

### 3.1 Two monies, one valve
- **Cash** — physical, on-body, spends on street/small stuff, dangerous to carry (gang heat).
- **Bank/card** — safe, unlimited, big/legit purchases are card-only.
- Cash on body has **no dirty/clean tag** — the deposit cap alone represents "some of it is dirty."
- **The valve:** cash→bank conversion is capped per day. **$500 credit-card setup (bought with cash — the bootstrap) unlocks conversion; buying businesses widens the cap (= laundering).** Card = unlock, business = accelerate.
- Withdrawal limits: **TBD (open)** — lean is unlimited, matching Schedule 1's one-way valve.

### 3.2 Payment channels
| Source | Channel |
|---|---|
| Income floor (slips, graffiti) | cash, very low |
| Betting winnings | cash |
| Selling any items / fencing stolen | cash |
| Illegal business | cash |
| Legal business | bank |
| ~~Part-time jobs~~ | **REMOVED** — jobs competed with the betting core (risk-free lane); intel-feeder variants also cut (any job sited at an intel location double-dips) |

### 3.3 Cash-OK vs card-only
- **Cash:** food, clothes, info, small items, room rent, bus/taxi/metro, furniture, weapons/tools, betting, medicine, lockpicks, ammo, disguise items, illegal-business staff wages & hiring, bribes, debt payments.
- **Card-only:** apartment/house, business purchase, car, bet-shop licence (town hall), business renovation, legal-business staff wages, vehicle upgrades.
- Principle: cash = daily life + consumables + core loop + crime; card = durable assets & wealth. You must launder to turn street success into lasting wealth.

### 3.4 Income floor (anti-stuck safety net)
- **Scavenge discarded betting slips & scratch cards** (streets + bins): mostly scrap at very-very-low value; small chance of an **uncashed winning slip** (one-off surprise).
- **Clean graffiti/posters off walls**: small cash on the spot.
- Both **floor-style**: anytime/anywhere, no boss, no shift. Deliberately **boring-enough-to-push-upgrade** (Schedule 1 litter-picking role).

### 3.5 Debt clock (Act 1 pressure)
- Principal **≈ $100,000 CAD** (real-world-aligned prices; set for pacing, see §3.6).
- **Monthly minimum payments go toward the principal.** Overdue adds **+10% to that missed installment** (not the whole principal). Prepay/overpay allowed.
- Debt is paid **in cash** (lives entirely in the cash economy — bettable winnings repay it directly).
- Anti-rush: money alone cannot open Act 2 (triple gate, §1). Early repayment's reward is *relief* (gang pressure off), not a content skip.

### 3.6 Pacing math (reference)
- **20 real minutes = one 24h in-game day.**
- $250 → $100k compounding at 2 bets/day: solid player (+6%/day) ≈ 100 game-days ≈ **33h**; aggressive (+11%/day) ≈ 55 days ≈ **18h**. Slower than Schedule 1's 5–10h identity shift, acceptable for a meaty Act 1; tune via floor income and early bet limits.
- **Bet-shop bans are temporary (1–2 days)** — they are cooldowns, not compounding-killers.

### 3.7 XP & levels
| Source | XP |
|---|---|
| Betting | win = more; losing = consolation XP tuned so low farming is never worth the time |
| Gathering intel | scales with source difficulty (risk / connection / money) |
| Successful crimes | medium |
| Debt payments | medium |
| Floor activities | small |
| Unique hidden items (treasure boxes) | pretty good |
| Improving NPC connections | medium |
- Level unlocks items & abilities and is gate ③ for act progression.

### 3.8 NPC connections
- **Per-NPC independent ratings.** Higher connection unlocks that NPC's verbs: hiring, giving information, accepting bribes, other activities.
- Leveling verbs: **gifting** (items, food incl. self-cooked, intel the player gathered), buying info from them, selling to them, plain engagement.
- Intel is social currency: a tip can be bet, sold, or gifted.
- Tuning flag: gifts need a daily cap or diminishing returns (anti-spam).

### 3.9 Profit tokens
- Illegal bookie (arcade back room) offers profit tokens **regularly**; legal shops require special means to obtain them. (Doc pillar unchanged: tokens amplify reward, never probability.)

中文摘要：兩種錢一條閥：$500卡解鎖存款（有上限），買生意開大閥。打工全剷（同核心競爭）。債$100k，月供入本金，逾期嗰期+10%，現金還。20分鐘=1日。XP多源，輸注XP低到唔值得farm。NPC關係每人獨立，送禮/買料/賣嘢/傾偈養關係。

---

## 4. Risk systems — dual heat

Two predators, two logics: police care about your *crimes*, the gang cares about your *money*.

### 4.1 Police heat
- Rises only when a crime is **seen & reported by an NPC or witnessed by police**; each crime type has its own value (values = tuning). No witness = no heat (perfect crime; rewards stealth).
- Effect: **stop-and-search frequency scales with heat.**
- **Search outcomes:** cash is fine (legal). Illegal/flagged-stolen items & crime tools → **fine (amount scales with heat) if you can & will pay; can't/refuse → jail.** Third option: **flee** — outlast the search countdown while hidden; success adds a **large heat lump** (overflow past max still → bounty).
- **Jail:** time skip + lose flagged items & crime tools + small money fine. **No debt afterward.** Real sting = missed events; if the skip makes a debt payment overdue, gang heat rises (systems chain).
- **Decay:** 99→0 in ~7 in-game days.
- **Bounty at 100:** police chase on sight until you hide and outlast the countdown. Clears by paying the fine, being caught (jail + fine), or staying uncaught long enough.
- **Weapon use seen by police = instant bounty** (skips the meter).

### 4.2 Gang heat (separate meter)
- Fuel ①: **days overdue** on the monthly payment. Fuel ②: **carrying >$500 cash outdoors, +1/hour** (travel counts; camping counts as indoor). The two components are independent.
- Effect: higher chance of being spotted & stopped by gang members.
- **Gang stop — choose:** bribe / hand over all on-body cash toward debt **+ vig** / fight back & flee.
- **Losing the fight:** all cash to debt + vig, health loss, out cold half a day, wake outside the hospital.
- **Max:** gang patrols near the player's fixed address and sleep spots. **Sleeping in the same spot 2+ nights = stakeout** (shelters ×2 / hotel / camping = rotation gameplay).
- Clears: overdue component drops **instantly** on clearing the overdue amount; cash component decays **-0.25/hour whenever below $500**.

### 4.3 Survival-bar consequences
- **Energy = 0:** forced sleep on the spot → 3h time skip, 50% chance of losing 10% of on-body cash.
- **Health = 0:** out half a day, wake at hospital, pay the bill.

中文摘要：雙heat。警察軌：被目擊先計分→搜身頻率→罰款（隨heat）或坐監（時間跳+冇收違禁+細罰款，無後續債）→100=通緝。7日自然衰減。黑幫軌：逾期日數+戶外帶現金>$500每鐘+1（-0.25/鐘衰減）→被截三選一→打輸清袋+vig+昏半日。同點瞓2晚被伏。

---

## 5. Item system

- **Inventory:** slot-based, upgradeable (backpacks/bags; home stash expandable via furniture). **On-body vs stash** are distinct states: carried = searchable/robbable, stashed = safe but must be fetched. Items split **clean vs flagged/incriminating**.
- **Survival bars ×3:** Health, Energy, Hunger. Food → hunger; **hunger > 80 grants slow health regen**; sleep restores energy *and* health; medicine heals directly; coffee / energy drinks / cigarettes = temporary energy boosts.
- **Cooking:** 1 veg + 1 meat + 1 carb + in-game time = a dish at ~half market price. No recipes/minigame/skill. Food **expires**; fridge/storage furniture matters. Self-cooked food is giftable.
- **Clothing:** disguise value (one number, multiplies stop-chance from both trackers) + **lock-and-key dress gates** (a few specific doors check outfits; everywhere else ignores clothing. Guardrail: never grows into a matrix).
- **Weapons:** punch ring, knife, pistol (bat cut). Sources: **steal from police/gang, treasure boxes, or extreme black-market price** — not normally sold. Every police officer carries a pistol. Gang loadouts mixed (gun/knife/ring). **Gunshots draw both police and gang to investigate.** Weapons are search contraband.
- **Furniture:** bed quality (faster recovery), food storage, cooking, drawers (stash capacity), decoration, **TV & radio (live match results at home)**.
- **Gold bars: cut.**
- **Treasure boxes:** scattered world loot (unique hidden items = good XP; a gun source). Contents = open thread.

中文摘要：格仔袋可升級，身上vs收藏分開。三bar：肚餓>80慢回血，瞓覺回血回energy。煮飯=1菜1肉1碳水半價，食物過期。衫=偽裝值+幾道dress門。武器：拳環/刀/槍，唔賣（偷/寶箱/黑市天價），槍聲引雙方。金條剷。

---

## 6. Actions & crime taxonomy

### 6.1 Crime buckets
- **① Petty street crime** (Act 1+): against ordinary NPCs — assault, pickpocket, mugging. Risk side = police heat system (§4.1).
- **② Fixing sabotage:** Act 1 none, **Act 2 none (players/core team untouchable — pure bookie act)**, Act 3 full (poison, bribe ref/player, assault, steal kit → form/absence effects). This is Act 3's core gameplay, not side content.
- **③ Betting fraud (cheat-slip): CUT.** If revived later, its home is an *Act 2 threat* (clients cheating the player's book), never a player tool.

### 6.2 Act 3 access model (systemic, universal verbs)
- Verbs (attack, steal, poison…) are universal; gated by **physical access + consequence**, never by greyed-out buttons. Act 1: players visible (training fence, club bus) but unreachable.
- **Connection is the master key** — both routes below require it.
- **Route 1 — break-in:** money buys info (schedules, entry) *only with good connections* → lockpick skill to enter facilities/hotel rooms. Caught = same bounty but **more severe** (bigger fine, longer heat cooldown).
- **Route 2 — welcomed in:** a connection introduces you to the team → free access. **Bribe:** works only with high connection rating; refusal = bounty. **Poison:** bounty only if an NPC sees the pour. **Attack:** caught = bounty; must clear the bounty to regain access.

### 6.3 Stealth model
- **Full manual, vision-based:** real sightlines, patrols, hiding spots, chase/search countdowns. **No footstep/sound propagation system** — the only sound events are **gunshots and attacks** (which summon both factions). Serves: Act 3 infiltration, poison sightlines, search-flee hiding, bounty chases, crawl/bush verbs.

### 6.4 Bribes (escalation across acts; overlaps = the "pay money" verb aimed at different targets)
| Act | Target | Effect |
|---|---|---|
| 1 | collectors, bartenders | buy a pass at a cash search; buy tips |
| 2 | police | **proactive:** lower heat / bury charges. **At-bust:** grease the officer → keep item, no record, no heat — priced **well above** the fine, gated by that cop's connection/corruptibility |
| 3 | refs, players | = the fixing mechanic (success gated by connection rating, §6.2) |

### 6.5 Fake ID
- **Quest reward only (special quest, Act 2 or 3).** Functions: bet inside shops during their **temporary ban** window (bans last ~1–2 days; main value = time saved), and easier escape from police/gang stops (**cooldown** after each stop use).

### 6.6 Legal verbs (captured)
Walk, run, jump, crawl/hide (bush), eat, sleep, buy/sell items-assets-info, place bet, talk to NPC, interact with world objects — plus derived: observe/gather intel (the pillar verb), listen to radio / watch TV, deposit/withdraw (ATM), stash/fetch, take transport, scavenge (floor), cook, gift.

中文摘要：三桶：街頭小罪1幕起；造馬破壞第3幕先有（第2幕球員不可掂）；改彩飛剷。第3幕access：人脈=萬能匙，受邀路vs破門路，各有爆煲觸發。潛行=全手動視覺型，冇腳步聲，只有槍聲/襲擊發聲。賄賂跨幕升級。假ID=任務獎勵，過暫時封殺+截停脫身（有冷卻）。

---

## 7. Intel system — three-tier ladder

| Tier | Content | Source | Lifespan |
|---|---|---|---|
| **① Durable** | team base profile — **qualitative only** (attack: strong/fair/poor; structural reads: striker-reliant, playmaker-dependent…); never raw numbers | **attending matches at the stadium** | the season |
| **② Semi-durable** | fixtures, streaks, today's formation | newspaper, bar talk, broadcasts | days–weeks |
| **③ Perishable** | specific player condition (hungover keeper, knocked striker) | supporters'-bar insiders, training-ground visits | one match |

- **Scouting is progressive** (逐場磨利): each watch sharpens the read. **Away watches are capped** — a team can only be fully understood by attending **their home ground** → travel is intel investment; knowledge = geography.
- **Broadcast boundary:** radio/TV give vivid key-moment commentary + today's formation + recent form — **never playstyle or height**. Tier ① is stadium-exclusive (the ticket buys what airwaves can't).
- **Stadium build tier:** rung 1 now — crowd audio + scoreboard + commentary (the match is *heard*, not rendered). Rung 2 (abstract pitch visualization driven by engine events) = future upgrade. Rung 3 (rendered players) = never (that's a different game).
- Stadium roles: intel purchase, dopamine peak (watch your bet live), NPC ecosystem (fans, punters, runners), cross-act inversion (terraces → your book → Act 3 boxes).
- Supporters' bars: one per team, 2 per town; each holds an insider / long-term supporter NPC = high-quality source for that team.

中文摘要：情報三層：①恆久（球場睇波,質性描述,逐場磨利,作客睇有上限——去佢主場先摸得透）②中期（報紙/酒吧:賽程、狀態、陣式）③即棄（insider/訓練場:個別球員今場狀態）。廣播只講陣式+狀態+關鍵時刻,唔講playstyle。球場第1級=聲音氣氛。

---

## 8. Open / parked threads

| Thread | State |
|---|---|
| Arcade minigames economics | parked — proposal on the table: house-edge games (winnable short-term, losing long-term; Act 2 the player collects the edge). NOT yet accepted |
| Hunting in natural area | tentative ("??") — new verb, needs gun/trap/noise rules |
| Driving / fuel system | implied by car dealer + 3 gas stops + vehicle upgrades — big mechanic, needs its own session |
| Treasure box contents & placement | open |
| Key-NPC roster for connection gates | open (doc has ball kid + shelter mate as Act 1 relationship NPCs) |
| Bank withdrawal limits | TBD — lean unlimited (Schedule 1 one-way valve) |
| Monthly minimum amount, per-crime heat values, gift caps, food expiry times, slot counts | tuning table, spec stage |
| "Fine vs jail" exact thresholds, bounty fine sizes | tuning |
