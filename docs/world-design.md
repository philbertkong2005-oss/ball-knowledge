# World, Economy, Items, Crime & Actions — Design Decisions

**Status:** LOCKED design decisions from planner Q&A sessions, 2026-07-15/16, **revised 2026-07-17**.
**Scope:** World map, economy, item system, crime/risk systems, player actions, intel system, **bookmaking (§9)**. These are Phase 3+ life-sim systems — nothing here changes the Phase 1 match engine contract (`phase1-match-engine.md`).
**Relation to `design-doc.md`:** companion document. Where this conflicts with the design doc's older sketches, **this document supersedes** (notable: Act 2 fixing-lite is now cut entirely — see §6).

**2026-07-17 revision — what changed:**
| § | Change |
|---|---|
| §1, §1.1 | **Act 2 restructured into two halves.** Fat Keung now falls at the **midpoint**, not the start. First half = a book **co-owned with Fanta**, on Fat Keung's turf, under his protection, paying his cut. Second half = **build shops from empty shells, don't seize his**. Fanta = permanent partner |
| §1.2 | **NEW** — Act 3 access, motive (the income ceiling), and gate. **No shops in the city** |
| §2.1 | **Town 3 cut.** 3 locations, teams re-split **2 + 2 + 4**. Metro now opens **Act 3** (was Act 2). Town 2 gated by the mountain |
| §2.2.1–2.2.2 | **NEW** — 3 empty shop shells (non-deferrable), laundromat promoted, **map scale (700m, walk 2.5 m/s)**, zone legibility |
| §3.5, §3.6 | Debt **= $100,000** confirmed + **`constants.json` conflict flagged** (§10). Pacing math **rebuilt on the fixture calendar and the Gate 1 +44% ROI** |
| §3.10 | **Arcade laundering cut** — Fanta owns it forever. Laundromat is the first front. Arcade prize caps resolved as a guardrail |
| §3.12 | **NEW** — bet-shop income `max × Q × C` |
| §4.1.1–4.1.2 | **NEW** — `fine = heat × $20`, `bribe = fine × 2` (cash), and **flee is fully manual** (30s chase timer, does not touch the meter) |
| §5 | Shop fit-out **reuses the furniture system** |
| §7 | Tier ① lifespan **"the season" → persists with −10 rollover decay** |
| §8 | Tuning table rebuilt: **8 open decisions needing Phil**, tuning rows re-prioritised, 3 rows moved out |
| §9 | **NEW — the whole bookmaking / odds system** |
| §10 | **NEW** — `constants.json` build handoff |

**2026-07-29 revision (from Phil's SketchUp draft + build feedback):**
| § | Change |
|---|---|
| §1.1 | **Act 2 second half reworked.** Fat Keung's own 3 shops **shutter → the player reopens THEM** (re-register + renovate); no separate "empty shells", the same 3 buildings change state. Fanta takes territory + arcade, not the shop buildings. **3 bet shops in Town 1, not 4** |
| §2.2.1 | Rewritten to match: **3 shop buildings (state-changing), not 6.** **Filler buildings = the Act-2 buyable laundering-front pool**; tall-building **facade rule** (ground-floor front, sealed floors above). NPC Residential Buildings ≠ shops |
| §2.1 | Town 2 now gated by a **bridge** (was "the mountain") |
| §2.2.3 | **Real map = 541 × 641 m** (Phil's draft); 700×700 logged as tested upper bound. Draft wins over the from-scratch coord table |
| §8.2 | Deposit-valve note updated: **up to ~7 laundering fronts** (the fillers), not ~3 — widening must shrink |

中文摘要：本文件記錄2026-07-15/16策劃問答鎖定嘅世界、經濟、物品、犯罪、行動、情報系統設計，**2026-07-17大幅修訂、2026-07-29再修**。**07-29改動**（來自Phil嘅SketchUp草稿+build回饋）：第2幕後半改成**重開肥強自己嗰3間熄檔舖**（唔係另起吉殼、同3間舖跨幕轉狀態）、**鎮1得3間bet shop唔係4間**、**填充建築=第2幕可買洗錢門面池**（高樓地下鋪做門面、樓上封死）、鎮2改用**橋**封鎖、**真圖=541×641m**（草稿為準）、存款閥門面數升到約7間要縮細每間幅度。**07-17改動**：第2幕拆成兩半（肥強改喺中段先倒，前半同Fanta合資喺佢地頭開地下盤口）、第3幕access同動機（收入天花板）+**大城唔起舖**、**鎮3剷**（3個地點、2+2+4隊、地鐵改第3幕開）、吉舖同地圖比例（700m）、債$100k確認+`constants.json`衝突、**街機唔洗錢**（Fanta永久持有，洗衣舖上位）、舖頭收入公式、罰款/賄賂公式+**flee全手動**、傢俬系統重用、第①層情報改為跨季保留但每季−10、調參表重建（**8個要你拍板嘅決定**）、**全新第9章做莊系統**、第10章交俾build session。同design doc衝突時以本文件為準（注意：第2幕造馬-lite已完全剷走）。**07-17改動**：第2幕拆成兩半（肥強改喺中段先倒，前半同Fanta合資喺佢地頭開地下盤口）、第3幕access同動機（收入天花板）+**大城唔起舖**、**鎮3剷**（3個地點、2+2+4隊、地鐵改第3幕開）、吉舖同地圖比例（700m）、債$100k確認+`constants.json`衝突、**街機唔洗錢**（Fanta永久持有，洗衣舖上位）、舖頭收入公式、罰款/賄賂公式+**flee全手動**、傢俬系統重用、第①層情報改為跨季保留但每季−10、調參表重建（**8個要你拍板嘅決定**）、**全新第9章做莊系統**、第10章交俾build session。

---

## 1. Structure & Acts

- **Identity evolution is the selling point (Structure A):** each act the player's primary identity genuinely changes — Bettor → Bookie → Fixer. Betting ability is **retained in all three acts** (secondary verb in Acts 2–3; in Act 3 it is the payoff step of every fix).
- **Act 2 is a pure bookie act.** Footballers and core team members are **untouchable in Act 2**; intimidation was cut as a mechanic entirely. "Changing the truth" is saved whole for Act 3.
- **Act 2 entry = triple gate:** ① debt principal cleared + ② connection levels with key NPCs reached ("trust" is unified into the NPC connection system, not a separate concept) + ③ player XP level reached. **The key NPCs for gate ② are Fanta (the arcade owner) and the shelter mate.**
- **Act 2 has two halves, split by the Fat Keung takedown at its midpoint** (§1.1). First half = run the illegal book by hand. Second half = build the legal shop network.
- Act fun statements: Act 1 = detective info-edge + survival tension (prey). Act 2 = "house always wins" power flip, book-balancing vs sharps (predator). Act 3 = heist-like fixing operations, you *make* the truth.

### 1.1 Act 2 arc — the Fat Keung takedown (LOCKED, rev. 2026-07-17)

- **All 3 legit bet shops in town 1 are run by Fat Keung.** The player owes him money AND wins bets out of his shops — you repay him with his own money; his shops ban you (temporarily) because you genuinely bleed him.
- **Fanta (the arcade owner) is a lieutenant under Fat Keung.**

**First half — the co-owned illegal book.**
- The player and Fanta **co-own** an illegal book in Fanta's arcade back room. The arcade is Fanta's; the turf and the protection are **Fat Keung's**, and the book pays him a **percentage cut**.
- **Fat Keung personally protects the operation that funds his own overthrow.** This rhymes with Act 1, where the player repays his loan with money won out of his shops. At every stage, Fat Keung pays for his own destruction.
- **Co-ownership is the conspiracy** — the player does not grind a relationship meter, they run a business with Fanta and the trust *is* the business.
- **Fat Keung's cut is the mechanical motive for the takedown.** A percentage, not a flat fee, so it scales: the better the book does, the more it stings.
- Every fixture also prices against Fat Keung's 3 legit shops at the 2-day reveal (§9.3) — **the first half is a price war against the man you are going to kill, on his turf, under his protection.**

**Midpoint — the takedown.**
- The player and Fanta take Fat Keung down together (fixed narrative, not a player choice), then mop up his docks office.
- **The takedown deletes two leakages at once:** Fat Keung's protection cut disappears, and per the spoils split Fanta exits the betting side. The player goes from a taxed partial stake to **sole ownership of the entire trade** — the payoff is a number the player has watched for half an act.
- **Spoils split:** Fanta takes the gang/territory side; the player takes the entire betting side. **Guardrail (stamped): the player's underworld role caps forever at "the enforcement arm of the betting business"** — collect the book's own debts, protect its venues; a territory/racket-management system never exists.
- **Fanta keeps the arcade forever** and remains a **permanent partner into Act 3**. The arcade is not a laundering front (§3.10).

**Second half — reopen the shops you bled him out of** (rev. 2026-07-29 — supersedes "build from separate shells").
- **Fat Keung's own 3 legit shops shutter within a week of the takedown, then become the player's to reopen.** They do NOT convert to Fanta's businesses. The player re-registers the licence at the town hall (card fee + time) and renovates each dead shop back into a working book. **There are no separate "empty shells" — the shells ARE Fat Keung's former shops** (one per non-rural zone; the same 3 the player has bet in and been banned from all through Act 1).
- **The spoils split, restated:** Fanta takes the whole **territory / enforcement side plus the arcade**; the player takes the **betting trade** — which now means three shuttered husks to pour money into. **He got the turnkey half, you got the fixer-uppers.** The distrust beat survives, just re-imaged: Fanta walked away with the easy side while you sink your Act-1 winnings reopening dead shops.
- **Why reopening still isn't a victory lap:** debt is already cleared at the gate, so there is no clock — the pressure has to come from somewhere, and it comes from the sink. A shuttered shop earns nothing until you pay to re-register and re-fit it. **You come out of the takedown cash-rich and asset-poor; spending it back down IS Act 2's tension.**
- Fat Keung's collectors are now unemployed and come to **you** — the men who hunted you in Act 1 end up on your payroll.
- **Staff are a wage line and a capacity unlock, never a management loop.** Hire once, pay wages, done. No quitting, stealing, morale or training — that is a different game.
- **Depth curve, not uniform depth:** the first shop is fully walked (haul the furniture, hire the man face to face). Later shops progressively delegate. Walked × 3 is a rhythm; menu × 3 breaks pillar 1.

**Expansion timeline.**
- **Town 2 opens at the midpoint** — the **bridge** to the east is the physical gate (§2.1). Town 3 is **cut** (§2.1).
- **The big city is Act 3's stage, and the player never builds shops there** (§1.2). The empire caps at towns 1–2.

### 1.2 Act 3 — access and gate (rev. 2026-07-17)

- **Loop:** pick a fixture → scout access (connection route or break-in, §6.2) → execute the fix → **bet it** → collect. Betting is the payoff step of every fix, so the Act 1 skill stays load-bearing to the end.
- **Where the bet goes: the city.** The player owns every book in towns 1–2, so nobody there will take the action. The city's bookmakers don't know them and are deep enough to absorb a big bet. **This is also the guardrail's answer: no shops in the city.** The city is where you *bet*, not where you expand.
- **Your own book becomes the liability.** Fix Aberdeen to lose, take the city's money — but your shops in towns 1–2 take Aberdeen action all week from locals who might know. **The empire you spent an act building is what exposes you**, at the exact moment it should feel safest.
- **Motive = the ceiling.** `max × Q × C` (§3.12) maxes out. The game shows a flat income line and says there is nothing left to earn. The city is the only frontier and it is closed — entrenched bookies, licences you cannot get. **Fixing is the crowbar.** Pillar 3's debt clock is long gone by Act 3, so the pressure must come from a visible ceiling.
- **Act 3 gate — mirrors Act 2's triple gate** rather than being a completion checklist: ① every shop across towns 1–2 operating + ② connection (Fanta at max — he opens the city door) + ③ player XP level. **[OPEN: confirm gate ② is Fanta and not an uninvented city NPC.]**

中文摘要（rev. 2026-07-17）：3間正行投注站全部係肥強嘅。**前半幕**：玩家同Fanta（街機老闆，肥強手下）**合資**開地下盤口，喺Fanta後舖、肥強地頭、肥強保護、按比例抽水——**肥強親自保護緊一盤資助佢自己被拆嘅生意**。合資本身就係陰謀；抽水就係拆佢嘅機制動機。**中段拆肥強**：一次過刪走兩條漏水（肥強抽水 + Fanta按分贓退出賭業）→ 玩家由被抽水嘅部分股東變成整條賭業唯一老闆。分贓（rev. 2026-07-29）：**Fanta攞成個地盤/執行側 + 街機**，玩家攞賭業。**Fanta永久持有街機、第3幕仍是拍檔。護欄不變：玩家黑道角色永遠封頂喺執行臂。****後半幕＝重開你逼到肥強熄檔嘅嗰3間舖**（rev. 2026-07-29，改咗）：肥強自己3間正行舖拆完一星期內熄燈→變成你嘅→重新登記+裝修先開得返。**冇另一批「吉殼」——同3間舖跨幕轉狀態**（Act 1肥強營業→熄檔→你重開）。地下盤口喺**街機後舖**，唔係第4間舖面，所以**係3間，唔係4間**。**點解重開都唔係勝利巡遊**：債喺入閘已清、冇時鐘，壓力來自個sink——熄檔舖要俾錢重登記+重裝先賺到錢；你拆完肥強係現金多、資產少，掟返啲錢落去就係第2幕張力。**分贓疑心beat照在**：Fanta攞現成嗰半、你攞爛尾嗰半。肥強啲收數佬失業，自己搵上門。**員工=人工開支+產能解鎖，永不做管理循環。****填充建築＝第2幕可買嘅洗錢門面池**（laundromat係最平嗰間；高樓＝地下鋪做洗錢、樓上封死）。**中段開鎮2**（**橋**係實體閘），**鎮3剷**。**第3幕大城唔起舖**——大城係你落注嘅地方，唔係擴張嘅地方；帝國封頂喺鎮1鎮2。第3幕動機=天花板：收入見頂、大城關門、造馬就係撬棍。第3幕入閘鏡像第2幕三閘。

中文摘要：身份進化係賣點（賭客→莊家→造馬者），落注三幕保留。第2幕係純莊家幕，球員不可掂。入幕=三閘：債清+關鍵NPC關係+等級。

---

## 2. World & Map

### 2.1 Region structure (rev. 2026-07-17 — town 3 cut)
- **3 locations total, 8 teams:** Town 1 (2 teams, 1 shared stadium) + Town 2 (2 teams, 1 shared stadium) + **the City (4 teams, 2 shared stadiums)**. The giant Harbour FC lives in the city.
  - **Town 3 is cut.** It was a repeat, not an escalation — town 2 already delivers the whole lesson (the model travels, expansion is real, fresh view), and town 3 delivers it a second time for another ~20–30 locations, NPC roster and storyline. **Scope is the #1 risk.** Two towns and a city is a *business*; three towns and a city starts to feel like territory, which is the shape the stamped guardrail exists to prevent.
  - **The city carries 4 teams to preserve the 8-team league** (2 + 2 + 4 = 8). Big city, more clubs — real football geography works this way, and it makes the city feel genuinely bigger, which serves Act 3. **[OPEN: 2 stadiums recommended over 3** — §2.1's rule is one shared ground per two teams; a third ground buys flavour at the cost of a whole location asset.**]**
- **One location per beat:** Town 1 = Act 1 + Act 2's first half. Town 2 = Act 2's second half. The City = Act 3.
- **The metro station to the city stays closed through Acts 1–2 and opens in Act 3.** A station you walk past for two acts and never ride is stronger foreshadowing than one that opens halfway. (Supersedes the earlier "opens in Act 2".)
- **Town 2 is gated by a bridge** (rev. 2026-07-29 — was "the mountain") — Town 2 sits across the water to the east, reached by a bridge that is impassable until the Act 2 midpoint. Whatever opens the bridge *is* the midpoint reward, and the player will have stared at it for two acts. Same trick as the closed metro, and a bridge you cannot cross is even more concrete than a ridge. (In the model the bridge is a symbolic placeholder; geometry to be redone.)
- Design lean: **small and interactive over big and empty.**
- **Act 1 local teams:** Aberdeen Fishermen 香港仔漁民 (supporters' bar A, docks) + Eastport Rovers 東港流浪 (supporters' bar B, residential). Keeper Wong (the demo's hungover keeper) is a local.

### 2.2 Act 1 town — four zones
| Zone | Contents | Risk profile |
|---|---|---|
| **Downtown core** | bank, town hall, police station, hospital, bet shop #1 (flagship), general store #1, newsstand, hotel, general bar, realtor, car dealer, phone booths | police-heavy: safest with cash, deadliest with contraband (search central) |
| **Old harbour / docks** | docks & fish market, black-market fence, Fat Keung's office, arcade + illegal bookie, bet shop #2, shelter #2, supporters' bar A, **laundromat (first of the filler laundering-front pool, §2.2.1)** | gang turf: contraband walks free, cash bleeds. Monthly payment day = cash run into the lion's den |
| **Residential fringe** | NPC houses, rentable rooms (housing ladder), **bet shop #3**, general store #2, gas stop #1 + fast food, park (slip-scavenging hotspot, unsafe sleep), shelter #1 (start home), supporters' bar B, phone booths | neutral; max gang heat patrols wherever the player lives |
| **Rural outskirts** | training ground (Act 1 watch through fence → Act 3 infiltrate), stadium (town edge, on bus line), natural area + campground (hunting?? unlocked), general store #3, gas stops #2–3 + fast food, treasure boxes concentrated | no factions but far — trips cost time/energy while cash-heat ticks |

- **Transport spine:** one bus line linking zones; taxis downtown & at gas stops; **one closed metro station "to the city" that opens in Act 3** (rev. 2026-07-17 — was Act 2; see §2.1). Expansion foreshadowed in concrete: you walk past it for two acts and never ride it.
- **ATMs on streets across every zone EXCEPT the camp/natural area.** Deposit & withdraw at any ATM once the $500 card exists. The bank building keeps card setup + later business banking.
- Zone symmetry: police zone and gang zone mirror each other — **what you carry decides where is safe.**
- Tension distances: ① overflow cash ↔ your stash (big-win days: the dangerous walk is home, not to the bank), ② stealing is easy, *fencing* means hauling contraband across town, ③ morning training-ground intel ↔ racing back to bet before odds move.
- Counts: 3 bet shops (one per non-rural zone — ban rotation costs real legwork; all three are Fat Keung's, see §1.1), 2 shelters, 3 general stores, 3 gas stops + fast food, 3 bars total (2 supporters' + 1 general).

#### 2.2.1 Shop buildings & laundering fronts (rev. 2026-07-29)
- **3 bet-shop buildings total in Town 1** — one per non-rural zone (docks, downtown, residential). In Act 1 all three trade as **Fat Keung's** legit shops (the ones the player bets in and gets banned from). Post-takedown they **shutter, then reopen as the player's** after re-registration + renovation (§1.1). **There is no separate set of empty shells — the same 3 buildings change state across the acts.** (Supersedes the earlier "6 shop-shaped buildings / 3 FK + 3 separate shells".) The illegal Act-2-first-half book runs from the **arcade back room**, not a fourth shopfront — so it is **3 bet shops, not 4** (confirmed 2026-07-29).
- **Filler buildings are the Act-2 buyable laundering-front pool** (rev. 2026-07-29) — not visual dead space. §3.1 makes "buying businesses widens the deposit cap = laundering" a core valve; the filler buildings are the businesses you buy to widen it. This makes every filler interactive (§2.1's "small and interactive over big and empty") and answers where the launderable businesses come from. The **laundromat** (docks) is simply the **first / cheapest** of this pool — the tutorial front (§3.10).
- **Facade rule for tall buildings:** a tall filler has its laundering business on the **ground floor only; every floor above is sealed and non-enterable.** Skyline density without walkable interiors — a hard scope fence against multi-floor interior building.
- **⚠️ Consequence for the deposit valve (§8.2):** ~5 fillers + 1 tall + laundromat ≈ **up to 7 laundering fronts**, where the valve was scoped for ~3. Per-business widening must shrink accordingly or the valve blows open. Tuning note, not a blocker.
- **NPC Residential Buildings** (the model's `Shop___Residential_Building` group — **rename in SketchUp**) are pure population/housing texture, **not shops** and not fronts.

#### 2.2.2 Scale (rev. 2026-07-17)
- **~700m corner to corner, buildings at real-life size.** Bounded by water and mountain — no invisible walls.
- **Walk 2.5 m/s, sprint 5 m/s — game speed, not real human speed (1.4 m/s).** At 1.4 m/s the diagonal costs 8.3 real minutes = **10 game hours = 41% of a 20-minute day**, and the player gets one errand done. At 2.5 m/s it is 4.7 real min / 5.6 game hours — a real commitment, not the whole day. Typical 250–300m trips land at ~2 real min / ~2.4 game hours = **three or four errands a day**, which is a life-sim day and makes "I can't do everything today" a choice rather than a punishment.
- **700m is what makes the transport ladder exist.** At 200m nobody rides the bus, the taxis are decoration, and the Act 2 car is a cosmetic. At 700m fare-vs-time-vs-heat is a live decision every day, and the first car collapses a 5-minute walk into 40 seconds — which is exactly the *"I made it out of the shelter era"* beat §5.1 asks for.
- **Sprint × Energy × cash = tension distance ① for free.** Sprinting burns Energy; Energy 0 → forced sleep → 50% chance of losing 10% of on-body cash (§4.3). So carrying a big win home: walk and bleed +5.6 gang heat, or sprint for +2.8 and risk dropping unconscious in the street with the whole roll in your pocket. **The fastest way home with a big win is the one most likely to get you robbed.**
- **⚠️ Zone boundaries are gameplay-critical, not flavour.** Shop income is `max × Q × C` where **C is district-scoped** (§3.12) — the player must know, standing on a street, which district they are in and whose good graces they are building. Same for §2.2's risk mirror ("what you carry decides where is safe"), which only works if you feel yourself cross the line. **Greybox fix: flat colour per district, hard edges at boundary streets.**
- **Density, not distance, is the real test.** 700m of populated street is a world; 700m of greybox between the harbour and the football cluster is a corridor — the failure mode §2.1's lean explicitly warns against.
- **Rural will not feel rural.** Four districts plus outskirts inside 700m puts the "far" zone ~300m out — mechanically far enough (~2.4 game hours, real heat cost) but it will not *read* as leaving town. Buy that feeling with terrain and sightlines; there are no metres to spare.
- **[OPEN: the football cluster.]** The current draft clusters training pitch + team facility + hotel + a bet shop within a block or two. §2.2 sites the training ground in **rural outskirts** specifically so the morning-intel run is a *race* (tension ③) — adjacent, that race is a walk to the next building. **But the cluster may be the better idea:** hotel + training + team facility together is a coherent "football district" (Act 1 watch through the fence, Act 3 infiltrate all three), and away teams sleeping next to the ground is real. **If the cluster stays, move the bet shops away from it, not the pitch** — the race is what matters.
- **Barber and pharmacy were cut** — their functions fold into general stores: **general stores sell medicine and clothing/disguise gear** (one store per zone, so even buying a disguise has legwork).
- **Treasure boxes/stashes:** diegetically **under-bridge stashes and plain unopened boxes**; free to open (no lockpick requirement); densest in rural zone. **Loot pool: rare gun, 100% profit token, car key (car location revealed)** — unique finds give good XP. Car keys spawn Act 2+.

#### 2.2.3 Act 1 town layout — geometry & Gate-2 (NEW 2026-07-28, build-unblocking)

**Companion diagram:** [`docs/assets/town1-gate2-layout.svg`](assets/town1-gate2-layout.svg) — top-down, generated from the coordinate tables below. The tables are authoritative; the SVG visualises them.

**Coordinate system.** Unity world units = metres. Origin `(0,0)` at the **town crossroads** (map centre). **X = east(+)/west(−), Z = north(+)/south(−).** Building positions are **centres**; footprints are `W×D` (X-extent × Z-extent) in metres.

**⚠️ Scale note (rev. 2026-07-29):** the coordinate tables below are the *from-scratch spec* on a 700×700 footprint. **Phil's actual SketchUp draft is 541 × 641 m** (839 m diagonal) and is the layout being built — deliberately smaller than the empirically-walked 700×700 (§2.2.2) so the town reads *dense* rather than as an empty corridor (§2.2.2's own warning: "density, not distance, is the real test"). **700×700 is logged as the tested upper bound, not the target.** The `town1-AS-READ-from-obj` render captures the real draft; where this table and the draft disagree, **the draft wins** — these coordinates are a reference to reconcile against, not an override.

##### Zone arrangement (the mirror)
Four quadrants meeting at the crossroads. **The two faction zones are point-reflections of each other through the origin** — same structure, inverted danger. This is the §2.2 "police zone and gang zone mirror each other" made concrete:

| Quadrant | Zone | Corner character |
|---|---|---|
| **SW** X[−350,0] Z[−350,0] | **Docks (gang)** | **Water/water corner** — the gang owns the harbour (S + W edges are waterfront) |
| **NE** X[0,350] Z[0,350] | **Downtown (police)** | **Mountain/mountain corner** — the police are inland/civic (N + E edges are mountain) |
| **NW** X[−350,0] Z[0,350] | **Residential (neutral)** | water W, mountain N — transitional; **player lives here** |
| **SE** X[0,350] Z[−350,0] | **Rural (outskirts)** | water S, mountain E — transitional; the football district + stadium |

- **Water** wraps the **SW corner** (whole S edge Z<−320, whole W edge X<−320). **Mountain** wraps the **NE corner** (whole N edge Z>320, whole E edge X>320). No invisible walls — you are stopped by harbour or ridge.
- **Gang at the water, police at the mountain** is the thematic payoff of the mirror: smuggling turf on the docks, civic power inland. Carrying contraband you avoid the NE (police); carrying cash you avoid the SW (gang); the safe thread between them is the NW↔SE neutral diagonal through the crossroads.
- **Zone legibility (§2.2.2):** flat colour per quadrant, hard colour edge at the boundary streets (Z=0 and X=0 axes). The player must *feel* the crossing.
- **Mountain road to Town 2** exits the NE corner beside the (closed, Act-1) metro station — the mountain physically gates expansion (§2.1).

##### Gate-2 geometry (THIS is the test)
The slice: walk from home, place the canned bet, walk the winnings home — through the one patrolled gang corridor.

- **Start — Shelter #1 (home):** `(−170, 40)`, residential, ~40m north of the docks boundary.
- **Destination — Bet shop #2 (Fat Keung's):** `(−150, −170)`, docks, east side of the corridor.
- **The route:** due south, crossing the **Z=0 residential→docks boundary at X≈−175** (the colour change = "you just entered gang turf"), down the corridor to the shop. **211m one way** (~84s / ~1.7 game-hours at walk 2.5 m/s); round trip ~3.4 game-hours.
- **The ONE patrolled corridor:** a single **N–S street centred X=−175, Z from −20 (boundary) to −275 (harbour front), 10m wide.** It is the only through-route: warehouses + water box it on the west, warehouses + the fence/Fat Keung's office on the east, so you cannot detour around the patroller.
- **Patroller path:** one gang NPC walking `(−175, −20) ⇄ (−175, −275)` end to end (the single patroller this greybox task builds).
- **Hiding spots (3), along the corridor:** alley recess `(−200, −165)` (west) · shipping-container stack `(−152, −205)` (east) · under-bridge stash `(−175, −265)` (harbour end, doubles as a §2.2 treasure spot). These are the line-of-sight breaks that make §6.3 manual stealth + §4.1.2 flee playable.
- **Why the walk carries tension both ways:** the canned payout (§ decision e) returns **>$500 to hand**, so the **walk home crosses the gang cash-threshold** (§4.2) — the corridor is dangerous on the way out (if already carrying) and on the way back (winnings). The empty shell at `(−150, −70)` sits *on the route*, so the player sees a "for lease" Act-2 hook on their very first Gate-2 walk.

##### Building placement
> **⚠️ Superseded for exact positions by Phil's 541×641 draft (rev. 2026-07-29).** These 700×700 coordinates are a from-scratch reference. Two model changes override the tables below: (1) **the "Empty shell — X" rows are gone** — per §2.2.1 there are only **3 bet-shop buildings**, which trade as Fat Keung's in Act 1 and become the player's shells post-takedown (no separate shells to place); (2) positions come from the draft. **What stays authoritative here is the Gate-2 *design logic*** — one patrolled docks corridor, a single patroller, 3 hiding spots, the walk that crosses the $500 threshold, the water/mountain mirror — which the build session **re-anchors onto the draft's actual shelter and docks-bet-shop positions** (the next reconciliation task).

**Docks (gang) — SW.** Corridor X=−175.
| Building | Centre | Footprint |
|---|---|---|
| Bet shop #2 (FK, Gate-2 dest.) | (−150, −170) | 10×15 |
| **Empty shell — docks** | (−150, −70) | 10×15 |
| Fat Keung's office | (−120, −230) | 12×15 |
| Arcade + illegal bookie | (−205, −140) | 15×20 |
| Black-market fence | (−215, −230) | 8×10 |
| Shelter #2 | (−260, −80) | 15×20 |
| Supporters' bar A (Aberdeen) | (−110, −100) | 10×12 |
| Laundromat (1st front) | (−205, −55) | 8×12 |
| Fish market / sheds (waterfront) | (−270, −290) | 40×25 |

**Residential (neutral) — NW.**
| Building | Centre | Footprint |
|---|---|---|
| **Shelter #1 (START)** | (−170, 40) | 15×20 |
| Bet shop #3 | (−90, 110) | 10×15 |
| **Empty shell — residential** | (−110, 175) | 10×15 |
| General store #2 | (−165, 130) | 10×12 |
| Gas stop #1 + fast food | (−290, 60) | 15×15 |
| Park (scavenge / unsafe sleep) | (−270, 210) | 50×40 |
| Supporters' bar B (Eastport) | (−200, 275) | 10×12 |
| NPC houses (×6) | ~(−110, 250) | 8×10 each |
| Rentable rooms | (−80, 300) | 12×18 |
| Phone booths | (−150, 80); (−100, 190) | 2×2 |

**Downtown (police) — NE.**
| Building | Centre | Footprint |
|---|---|---|
| Bet shop #1 (flagship) | (70, 80) | 12×18 |
| **Empty shell — downtown** | (115, 65) | 10×15 |
| Bank | (120, 150) | 20×25 |
| Town hall (licence reg.) | (185, 205) | 25×30 |
| Police station & prison | (245, 285) | 30×35 |
| Hospital | (85, 235) | 25×30 |
| General store #1 | (150, 95) | 10×12 |
| Newsstand | (55, 130) | 5×5 |
| General bar | (100, 175) | 10×12 |
| Realtor | (165, 265) | 10×12 |
| Car dealer | (275, 175) | 25×20 |
| Metro station (closed) | (330, 30) | 20×20 |
| Phone booths | (85, 105); (195, 160) | 2×2 |

**Rural (outskirts) — SE.** Football district clustered NW of the quadrant (decision a).
| Building | Centre | Footprint |
|---|---|---|
| Stadium (town edge, bus line) | (245, −270) | 150×120 |
| Training ground (pitch) | (110, −150) | 70×45 |
| Team facility | (150, −110) | 20×25 |
| Away-team hotel | (105, −95) | 20×30 |
| Natural area + campground | (290, −110) | 60×60 |
| General store #3 | (55, −70) | 10×12 |
| Gas stop #2 + fast food | (150, −55) | 15×15 |
| Gas stop #3 + fast food | (230, −215) | 15×15 |
| Treasure boxes (concentrated) | (300,−170); (315,−60); (275,−300) | — |

##### Streets & transport
- **Widths:** main spine (the two axis roads through the crossroads) **12m**; zone through-streets **8m**; **docks corridor 10m**; alleys / side streets **4–6m**.
- **Bus line:** spine along the axes — E–W road at Z≈0, N–S road at X≈0, meeting at the crossroads; stops at each zone `(−175,−15)` docks · `(−90,15)` residential · `(75,20)` downtown · `(150,−25)` rural, plus a **stadium spur** to `(255,−200)`.
- **ATMs:** streetside in all zones **except** the rural natural-area/campground (§2.2).

##### Decisions recorded on this layout
- **(a) Football cluster — KEEP IT, in rural** (§8.1 #4 resolved). Stadium + training ground + team facility + away-team hotel form one coherent "football district" at the SE edge (Act 1: watch through the fence; Act 3: infiltrate all three; away teams sleep next to the ground — real). **The hotel moved out of downtown into this cluster.** **No bet shop sits in or near it** — the nearest is bet shop #1 downtown, ~233m away, so the §2.2 morning-intel *race* (tension ③) survives. This is exactly §8.1 #4's recommendation: the thing that moves is the bet shop, not the pitch.
- **(b) `catch_vig = 0.30` ratified** into §4.2 (§10 item 4 closed). Owner: **wd:§4.2**.

中文摘要（§2.2.3）：第1幕地圖幾何，解鎖build。**座標系**：Unity單位=米，原點(0,0)喺鎮中心十字路口，X東西/Z南北，全圖±350（=實測行過嘅700×700）。**四區佈局=鏡像**：黑幫碼頭喺SW（水/水角，佔海港）、警察downtown喺NE（山/山角，內陸civ）——兩個勢力區係穿過原點嘅點對稱，同構but危險相反；住宅(NW)同郊野(SE)係中間過渡帶。水包住SW角、山包住NE角，山路喺NE出鎮2。**Gate-2幾何（就係個測試）**：由庇護所#1 (−170,40) 行去碼頭投注站#2 (−150,−170)，向南過Z=0邊界（顏色一變＝入咗黑幫地頭），落一條**唯一巡邏走廊**（X=−175、10m闊、被貨倉同水夾住冇得繞）；**一個巡邏兵** (−175,−20)⇄(−175,−275)；**3個匿藏點**（西巷、貨櫃堆、橋底）。派彩>$500入手→**返程過黑幫現金門檻**→出入都有張力。吉舖(−150,−70)喺條路上，第一次行就見到「吉舖招租」第2幕鈎。**建築座標表**見上（碼頭/住宅/downtown/郊野四區全部中心座標+footprint）。**街闊**：主幹12m、區內街8m、碼頭走廊10m、巷4–6m。巴士線沿兩條軸。**已記決定**：(a)足球區保留、擺郊野、酒店由downtown搬入，**唔擺投注站**（保住晨早情報賽跑）；(b)`catch_vig=0.30`正式收入§4.2。

中文摘要：4鎮各2隊共用1場。第1幕單鎮四區：downtown（警多）、碼頭（黑幫地頭）、住宅、郊野。帶咩決定邊度安全。ATM遍佈全區除營地。

---

## 3. Economy

### 3.1 Two monies, one valve
- **Cash** — physical, on-body, spends on street/small stuff, dangerous to carry (gang heat).
- **Bank/card** — safe, unlimited, big/legit purchases are card-only.
- Cash on body has **no dirty/clean tag** — the deposit cap alone represents "some of it is dirty."
- **The valve:** cash→bank conversion is capped per day. **$500 credit-card setup (bought with cash — the bootstrap) unlocks conversion; buying businesses widens the cap (= laundering).** Card = unlock, business = accelerate.
- **Withdrawals: unlimited** (one-way valve, matching Schedule 1). Withdrawing turns safe money back into hot cash — the player punishes themselves.

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
- Principal **= $100,000 CAD** (confirmed 2026-07-17; real-world-aligned prices; set for pacing, see §3.6).
- **⚠️ `design/constants.json` contradicts this section and must be reconciled** (build handoff, see §10):
  - `starting_debt: 500000` → **should be `100000`**.
  - `weekly_debt_interest: 0.1` → **semantically wrong.** This section specifies a *monthly minimum* with *+10% on the missed installment*, not weekly interest on the principal. That is a rename plus a behaviour change, not a value edit.
  - `catch_vig: 0.3` → a live number this document never ratified. Either §4.2 adopts 0.3 as the gang-stop vig or the constant goes.
  - **Before touching any of it: confirm these three fields are dead to the match engine and the `validate` harness.** If `validate` reads them, the Phase 1 freeze applies and the numbers do not move.
- **Monthly minimum payments go toward the principal.** Overdue adds **+10% to that missed installment** (not the whole principal). Prepay/overpay allowed.
- Debt is paid **in cash** (lives entirely in the cash economy — bettable winnings repay it directly).
- Anti-rush: money alone cannot open Act 2 (triple gate, §1). Early repayment's reward is *relief* (gang pressure off), not a content skip.

### 3.6 Pacing math (rev. 2026-07-17 — now calendar-derived)
- **20 real minutes = one 24h in-game day.**
- **Act 1 = exactly one season = 98 game days ≈ 33 real hours** (see the fixture calendar, §9.6). The earlier "+6%/day at 2 bets/day" was written before a calendar existed — with one, the player compounds **per matchday**, not per day.
- **The math, rebuilt on the Gate 1 proven number:**
  - ~49 bettable matchdays in a season
  - $250 → $100k = **400× growth** → needs **+13% per matchday**
  - **Informed ROI = +44%** (proven, Gate 1) × ~30% of bankroll staked per matchday = **+13.2%** ✓
- So a solid informed player betting roughly a third of their roll each matchday reaches $100k in one season. **§3.6's original headline (100 days / 33h) survives intact and now rests on the proven engine number instead of a sketch.**
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

### 3.10 Arcade & minigames
- Machines are **pay-to-play skill/fun games with no betting** (e.g. batting machine, penalty-shootout minigame) — a pure entertainment expense, zero competition with the betting core.
- **Prize pool (skill rewards, never money):** gift trinkets (feed the NPC gifting system), game achievements, occasional profit tokens, unique clothes, bonus bet items. **Guardrail (not a tuning knob):** the question is not "what rarity?" but "can arcade skill farm tokens at all?" — answer **no**, via a hard daily cap. A probability invites exactly the grinding that killed part-time jobs (§3.2).
- **The arcade is NOT a laundering front, and Fanta owns it forever** (rev. 2026-07-17). It is his business, not a thing the player buys out. Supersedes the earlier "Act 2 dual identity".
  - **The first laundering front is the laundromat** (docks, §2.2.1) — already sited in this document as the "first buyable front candidate", now promoted to *the* one.
  - **What the cut exposes is better than what it replaced.** Follow the money: the **illegal book pays in cash** (all of it, needs the valve); **legal bet shops pay into the bank** (§3.2, clean on arrival). So the deposit valve bites hardest in **Act 2's first half**, when the co-owned book is the only income and the player is drowning in cash they cannot bank — and the second half's legal shops dissolve the problem. **"Why go legit" stops being a story beat and becomes a mechanical relief: you go legal because you physically cannot store the money otherwise.**

### 3.11 Global rule — every item is sellable
- No junk-tier items. Where depends on the item: normal goods → general stores/market; flagged/stolen → fence only; meat & hide → market / fish market.
- **Guardrail (tuning):** sellable ≠ profitable. Every non-betting income (hunting, scavenging, odds-and-ends) prices below what betting earns — universal sellability is texture and freedom, never a second career.

### 3.12 Bet-shop income (Act 2 second half onward — NEW 2026-07-17)

```
daily income = max_income × Q × C
```
- **Q = fit-out quality**, 0–1. Comes from renovation + furniture, **reusing the §5 furniture system** — same items, same general stores, same haul. Quality tier = what is installed. A bespoke shop-fitting system is expensive; reusing the home-furniture pipeline is nearly free.
- **C = district connection**, 0–1. The **total** connection points across that district's NPCs, over the district maximum. **Total, not average** — an average punishes you for meeting people (a new NPC starts at 0 and drags your shop's income down), which taxes the exact social play the game is built to reward.
- **5–6 NPCs per district**, each 0–100. Fewer NPCs means each carries more weight, which is what you want — they are intel sources and gift targets, not point buckets. Five deep beats ten shallow, and it is half the work.

**Why multiplicative and not additive.** The design intent is that quality and connection each carry ~50%. Additive (`0.5Q + 0.5C`) breaks the fiction: a beautiful shop in a district that has never heard of you would earn **half income**. Multiplicative earns **nothing** — which is the stated intent exactly. And it *is* 50/50: both terms carry identical weight, so a 10% gain in either is worth the same 10% of income.

**The consequence is the point: you cannot buy your way into a district.** Money alone opens a shop that earns nothing. You must be known there first — which makes "which district do I open in" a real decision, front-loads the social play, and keeps renovation as a **multiplier on social capital rather than a substitute for it**. That is the scope fence on renovation, written as maths instead of a rule.

- **Q's floor is free: the town hall will not licence an empty room.** The minimum legal fit-out sets it. The registration beat is already doing the work.
- **C can approach zero** — so a player *can* build a shop where they know nobody and watch it earn ~$40/day. Fair lesson, cruel ambush. **Fix: show district connection on the purchase screen.** A decision with a visible cost, not a surprise.
- **Two payoffs, two playstyles:** §3.8 already gates hiring/info/bribes behind *high* connection with one person — depth buys **verbs**. Total-based shop income pays the opposite pattern — breadth buys **income**. Neither dominates.
- **Do not inflate `max_income`.** At $2,000/day a maxed town 1 (3 shops) ≈ $6,000/day, landing right next to Act 1's endgame betting income. That is not a failure: **the bookie does not need to out-earn the bettor, he needs to earn it without variance.** Same money, zero risk, every day, whether the fishermen win or lose. *That* is "house always wins" — the promotion is the flat line.
- **⚠️ [OPEN: `Q × C` vs `√(Q × C)`.]** Same fiction, opposite strategy — this quietly decides whether Act 2's second half rewards concentration or spread:

| | One shop maxed | Three shops at half/half |
|---|---|---|
| `Q × C` | 1.0 | 0.75 → **concentrate** |
| `√(Q × C)` | 1.0 | 1.5 → **spread** |

  **Recommended: `Q × C` (concentrate).** It front-loads the NPC content (you actually live in the docks for a while instead of skimming three districts), slows expansion so the empire feels earned, gives Act 2's second half a rhythm instead of a blur, and matches §2.1's stated lean — small and interactive over big and empty.

中文摘要（§3.12）：舖頭日收入 = `max_income × Q × C`。**Q**=裝修質素（重用 §5 傢俬系統，唔開新系統）；**C**=分區關係**總分**（唔用平均——平均會因為你識多個新NPC而扣你錢，即係懲罰緊遊戲最想鼓勵嘅社交玩法）；**每區5-6個NPC**（5個深過10個淺，仲慳一半功夫）。**點解用乘法唔用加法**：加法之下，一間靚舖喺完全唔識你嘅區照收一半——同你個fiction相反；乘法收零，正正係你講嗰個意思。**而乘法本身就係50/50**（兩項權重相同）。**重點後果：你買唔到入場券**——淨係有錢開到嘅舖係零收入，你要先被人認識。**裝修永遠只係社交資本嘅乘數，唔係替代品——呢個就係裝修系統嘅範疇圍欄，用數學寫。**Q嘅底由牌照免費提供（政府唔會俾牌一間吉房）；C可以趨近零，所以**購買介面要顯示區域關係**（由「伏」變「有代價嘅決定」）。**唔好谷高 max_income**：莊家唔需要贏過賭客，佢需要嘅係**賺得冇波幅**——條平線本身就係升職。

中文摘要：兩種錢一條閥：$500卡解鎖存款（有上限），買生意開大閥。打工全剷（同核心競爭）。債$100k，月供入本金，逾期嗰期+10%，現金還。20分鐘=1日。XP多源，輸注XP低到唔值得farm。NPC關係每人獨立，送禮/買料/賣嘢/傾偈養關係。

---

## 4. Risk systems — dual heat

Two predators, two logics: police care about your *crimes*, the gang cares about your *money*.

### 4.1 Police heat
- Rises only when a crime is **seen & reported by an NPC or witnessed by police**; each crime type has its own value (values = tuning). No witness = no heat (perfect crime; rewards stealth).
- Effect: **stop-and-search frequency scales with heat.**
- **Search outcomes:** cash is fine (legal). Illegal/flagged-stolen items & crime tools → **fine (amount scales with heat) if you can & will pay; can't/refuse → jail.** Third option: **flee** (see 4.1.2).
- **Jail:** time skip + lose flagged items & crime tools + small money fine. **No debt afterward.** Real sting = missed events; if the skip makes a debt payment overdue, gang heat rises (systems chain).
- **Decay:** 99→0 in ~7 in-game days.
- **Bounty at 100:** police chase on sight until you hide and outlast the countdown. Clears by paying the fine, being caught (jail + fine), or staying uncaught long enough (= the 7-day decay, **not** the 30-second chase timer — see 4.1.2).
- **Weapon use seen by police = instant bounty** (skips the meter).

#### 4.1.1 Fine & bribe (NEW 2026-07-17)
```
fine  = heat × $20        (heat 100 → $2,000; heat 50 → $1,000)
bribe = fine(heat) × 2    (heat 100 → $4,000)
```
- **Both are cash** (§3.3). This is load-bearing: if bribes were card-payable you would buy your way out of a search from a bank balance, carry nothing, and never touch gang heat. The whole bind below collapses.
- **The bribe is not a button.** §6.4 gates the at-bust bribe on **that officer's connection/corruptibility**. So the insurance **does not always pay out** — you can carry $4,000, bleeding gang heat all day, and get searched by an honest one. **Carrying bribe money is a gamble, not a purchase.**
- **Pillar 4 legal:** money buys **safety**, never luck. Early game you go to jail because you cannot afford $2,000; late game you pay and shrug.
- **The fine goes trivial by late Act 1** ($2,000 vs a $100k bankroll = 2%) — **correct**, because this section already says the money was never the point: *"Real sting = missed events."* The punishment migrates from cash to **time and items** exactly as the player gets rich enough to stop caring about cash.
- **⚠️ The bind (emergent, nobody designed it):** bribe at max heat = **$4,000**; the gang-heat threshold is **$500** (§4.2). Police insurance is **8× the amount that makes you hot with the gang**. Carrying $4,000 all day = **+24 gang heat/day → max in four days.** So high police heat makes you want bribe money in your pocket, and bribe money *is* gang heat. **Dodging the police feeds the gang.** §4's thesis is "what you carry decides where is safe"; these two numbers turn it into: **at high heat, nothing you carry makes you safe anywhere.**

#### 4.1.2 Flee — fully manual (NEW 2026-07-17)
- **There are no flee odds. It is not a dice roll.** §6.3 locks stealth as full-manual vision-based, and explicitly names "search-flee hiding" as one of the things that system exists to serve. A probability here would break §6.3 **and** pillar 1 (*every bet is walked — no menu-only actions*).
- **The spec:** the pursuing NPC walks to the last point at which its vision saw the player. Losing sight, it walks and scans for **30 seconds**. 30 seconds without sighting the player **ends the chase** — NPCs give up and return to patrol. Action-game rules, line-of-sight only.
- **⚠️ The 30 seconds ends the chase. It does not touch the meter.** Fleeing still adds the **large heat lump**. If 30 seconds also cleared the wanted state, three locked rules would cancel each other: the heat lump would erase itself, the 7-day decay would never matter, and two of the three bounty-clear routes would die (hiding in a bush for half a minute is strictly better than paying $2,000).
- So after a successful flee you are un-pursued but **dirtier than when you started** — still high heat, still stopped on sight, still carrying the lump. **Fleeing is not an escape, it is a deferral.** You did not get away with it; you postponed it and made it worse.

### 4.2 Gang heat (separate meter)
- Fuel ①: **days overdue** on the monthly payment. Fuel ②: **carrying >$500 cash outdoors, +1/hour** (travel counts; camping counts as indoor). The two components are independent.
- Effect: higher chance of being spotted & stopped by gang members.
- **Gang stop — choose:** bribe / hand over all on-body cash toward debt **+ vig** / fight back & flee.
- **Vig = 0.30** (ratified 2026-07-28, §10 item 4 closed; this section now OWNS the constant `catch_vig`). Defined as a **30% surcharge added to remaining debt on the amount seized**: seize $1,000 → $1,000 comes off principal, $300 is added back as penalty, so getting caught with cash carried costs you a net $300 for the privilege. Loan-shark logic; it is a punishment for carrying, not a payment plan. **This is a life-sim value and does NOT belong in the match engine's `EngineConfig`** (see §10 item 5).
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
  - **Bet-shop fit-out reuses this system** (rev. 2026-07-17) — same items, same general stores, same haul. A shop's `Q` in §3.12 is what is installed. **Do not build a second, bespoke shop-fitting system.**
- **Gold bars: cut.**
- **Treasure boxes:** see §2.2 (form, loot pool, free to open).
- **Hunting (natural area):** role = **food supply, not income** — meat is mainly a cheap cooking ingredient; meat and hide are sellable (global rule §3.11) but priced as texture, never a career. Tool: **gun** (hunting is naturally gated behind rare gun acquisition).

### 5.1 Driving & fuel
- **Real driving:** the player drives to most places; **hitting people with the car is a crime like any other** (assault-class, heat if witnessed — universal verbs hold).
- **Availability: from Act 2, price-gated** — cars are card-only big assets, so laundered savings are the natural gate. First car = the "I made it out of the shelter era" beat. Treasure-box car keys spawn Act 2+.
- **Fuel: real meter.** The car runs dry; park at a gas stop to auto-refuel (paid). **Run dry mid-road → walk to a gas stop for a jerry can.**
- **The car is outdoors:** the cash-carry gang-heat tick (+1/hr over $500) keeps running while driving — a car never shelters your money.
- **Boot = weak stash:** police pull-overs search the vehicle (contraband in the boot still busts you); boot cash still counts as carried for gang heat. Convenience, never immunity.

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
| **① Durable** | team base profile — **qualitative only** (attack: strong/fair/poor; structural reads: striker-reliant, playmaker-dependent…); never raw numbers | **attending matches at the stadium** | **persists across seasons, −10 at rollover** (rev. 2026-07-17 — supersedes "the season"; see §9.5) |
| **② Semi-durable** | fixtures, streaks, today's formation | newspaper, bar talk, broadcasts | days–weeks |
| **③ Perishable** | specific player condition (hungover keeper, knocked striker) | supporters'-bar insiders, training-ground visits | one match |

- **Scouting is progressive** (逐場磨利): each watch sharpens the read. **Away watches are capped** — a team can only be fully understood by attending **their home ground** → travel is intel investment; knowledge = geography.
- **Broadcast boundary:** radio/TV give vivid key-moment commentary + today's formation + recent form — **never playstyle or height**. Tier ① is stadium-exclusive (the ticket buys what airwaves can't).
- **Stadium build tier:** rung 1 now — crowd audio + scoreboard + commentary (the match is *heard*, not rendered). Rung 2 (abstract pitch visualization driven by engine events) = future upgrade. Rung 3 (rendered players) = never (that's a different game).
- Stadium roles: intel purchase, dopamine peak (watch your bet live), NPC ecosystem (fans, punters, runners), cross-act inversion (terraces → your book → Act 3 boxes).
- Supporters' bars: one per team, 2 per town; each holds an insider / long-term supporter NPC = high-quality source for that team.

中文摘要：情報三層：①恆久（球場睇波,質性描述,逐場磨利,作客睇有上限——去佢主場先摸得透）②中期（報紙/酒吧:賽程、狀態、陣式）③即棄（insider/訓練場:個別球員今場狀態）。廣播只講陣式+狀態+關鍵時刻,唔講playstyle。球場第1級=聲音氣氛。

---

## 8. Open threads & tuning table (rev. 2026-07-17)

### 8.1 Open design decisions — **these need Phil, not a playtest**

| # | Decision | Recommendation |
|---|---|---|
| 1 | **`Q × C` vs `√(Q × C)`** (§3.12) — silently decides whether Act 2's second half rewards concentration or spread | `Q × C` (concentrate) |
| 2 | **Empty shop shells: 2 or 3?** (§2.2.1) | 3 — one per non-rural zone, mirrors existing geography, maps 1:1 onto NPC districts |
| 3 | **The phone: booth or brick?** (§9.7) | Booth — a brick kills the training-ground race and the sharp-detection loop |
| 4 | **Football cluster** (§2.2.2) — keep it and move the bet shops away, or move the pitch to rural? | Keep the cluster, move the shops — the race is what matters |
| 5 | **City stadiums: 2 or 3?** (§2.1) | 2 — preserves the one-ground-per-two-teams rule; a third buys flavour at the cost of a location asset |
| 6 | **Act 3 gate ②** (§1.2) — is Fanta the city door, or an uninvented city NPC? | Fanta |
| 7 | **Season decay magnitude** (§9.5) — −10 proposed | −10; **hard constraint: must stay under +20/season** |
| 8 | **The 85 cap** (§9.1) — deliberate or an accident? | **Deliberate.** Say it out loud or someone "fixes" it to 100 |

### 8.2 Tuning table — **propose a default, feel it in-game, sign off**

| Tuning item | Status |
|---|---|
| Monthly minimum payment amount (vs income curve) | **Do first.** Derivable on paper from §3.6; everything else is denominated in it |
| Deposit-cap baseline & per-business widening | **Do second.** Derivable once the payment curve exists. **⚠️ Note (rev. 2026-07-29): the laundering fronts are the filler buildings (§2.2.1) — ~5 fillers + 1 tall + laundromat ≈ up to 7 fronts, not the ~3 originally scoped. Per-business widening must shrink accordingly, and consider diminishing returns per additional front, or the valve blows wide open.** |
| Heat→detect-time curve shape | Genuine feel-tuning. **Police and gang must not share a curve** — §4 says two predators, two logics; one shape makes them one system with two labels |
| `k` in `bribe = fine × k` (§4.1.1) | Structure is decided; only `k` is free. Feel-test it |
| Per-crime police-heat values | Needs real walking distances — measure, don't guess |
| Jail durations, bounty sizes | After the heat curve |
| Gift caps / diminishing returns (anti-spam) | Cheap |
| Food expiry, slot counts, bag upgrade sizes | Cheap — placeholder and move on |
| Fuel prices, car prices | Cheap |
| `max_income` per shop (§3.12) | ~$2,000/day placeholder. **Do not inflate** |

### 8.3 Moved out of the tuning table

- **Arcade prize rarity caps → resolved as a guardrail, not a knob** (§3.10). The question was never "what rarity" but "can arcade skill farm tokens at all?" Answer: no, hard daily cap.
- **Scouting reveal rates → resolved** (§9.1). They are the familiarity ladder, not a tuning row.
- **Full NPC roster → promoted to a system dependency, not a story pass.** §3.12's income formula cannot be tuned until district roster size is fixed (5–6 per district), and §4.1.1's bribe needs named corruptible cops. It is now **upstream** of the deposit-cap work rather than parallel to it.

### 8.4 Not yet designed

- **Fixture calendar exists (§9.6) but the season/league data model does not** — team ratings, the 14-round schedule, promotion/relegation (if any), off-season churn magnitude per team.
- **Tier ③ fake intel** (§9.4) is a new mechanic this document introduces. §3.8's "intel is social currency" is its natural home: if NPCs sell tips, some sell bad ones. Needs a real spec.

---

## 9. Bookmaking — the book & odds system (NEW 2026-07-17)

**Scope:** Act 2 onward. This is how the player prices a match once they are the house. It consumes §7's intel ladder and gives it teeth. Nothing here touches the Phase 1 match engine contract.

**The thesis, in one line:** *the whole identity flip lives in one mechanic — same verb, opposite use.* In Act 1 you learn the keeper is hungover and you **bet** it. In Act 2 you learn the keeper is hungover and you **shade your line** so the people who don't know eat the wrong price. The player is not taught a new skill; the skill they spent 33 hours mastering is inverted.

### 9.1 The familiarity meter

Per-team, 0–100. Measures how well the player understands that team's playstyle and general ability.

| Source | Value | Notes |
|---|---|---|
| **Tier ① — home watch ×3** (max) | **+45** | Full read. Accumulates and persists |
| **Tier ① — away watch ×3** (max) | **+30** | Half read (§7's away cap). Accumulates and persists |
| **Tier ② — newspaper / bar / broadcast** | **+10** | **Per match, does not accumulate.** Available 1–5 days out |
| **Maximum** | **85** | |

- **Persistent portion caps at 75**; tier ②'s +10 is applied fresh each match.
- **The 15-point gap is deliberate: you can never fully price a match.** Pillar 4's sibling — *nothing buys certainty*. The player stares at 85/100 forever and knows there is a slice of the world they will never own. **[OPEN #8: confirm, or it gets "fixed" to 100.]**
- **Tier ② is public, so it can never be an edge.** The paper lands on every doorstep — punters read it, rival bookies read it. It stops you being lost; it does not make you sharp. **The edge stays on the terraces (①) and in the pub the night before (③).**
- **Tier ② is also the Harbour FC lifeline** — you cannot watch the city teams until Act 3, but you can read about them. Never blind on the giant, just permanently worse on him than on the fishermen.

### 9.2 The range

The system does not tell the player the true line. It offers a **range** to price within. **Width is linear in the meter.**

- **⚠️ Width is defined in win-percentage and converted to American odds only for display.** A fixed width in American odds means wildly different things: ±50 points at −110 spans **24 points of probability**; at −600 it spans **2**. The meter would lie to the player, and lie worst exactly where the betting is most interesting. The engine already produces probabilities and `dotnet run -- board` already converts them — this is how it wants to be built.

| Meter | Range |
|---|---|
| 85 | true probability **±1.5 points** |
| 40 | **±8 points** |
| 0 | **no range** — blind |

- **The range has teeth, and the teeth are sharps.** Nothing else punishes a bad line. Wide range → you guess → the sharp in your shop takes the mispriced side and bleeds you. **Familiarity is armour against arbitrage, not a hint system.** This closes the loop between §7's ladder and §1's "book-balancing vs sharps" — they are one system seen from two ends.
- **A punter betting heavy is itself intel.** Someone walks in and hammers Aberdeen — do they know something? Other people's money is a tier of information the player must read. This is Act 2's signature paranoia and it costs almost nothing to build.

### 9.3 The book timeline

| When | What |
|---|---|
| **3–5 days out** | Player sets the line, within the range their familiarity permits |
| **1–5 days out** | Tier ② available — moderate narrowing |
| **2 days out** | **Every book in the game reveals simultaneously** |
| **1 day out** | Scouting day — tier ③ perishable intel. Player may move the line |

- **Anti-copy:** the player cannot see any other book's line before finalising their own. No copying.
- **The 2-day reveal is itself intel.** You set blind, all lines reveal, *then* you adjust — so you see where you differ from the market before your final move.
- **In Act 2's first half that reveal is aimed at Fat Keung.** You see his 3 shops' prices before your last adjustment: undercut him, pull his punters, bleed him on price. **The takedown motive, built out of the odds board** (§1.1).

### 9.4 Shading — where the edge is taken

**You sweeten the side you know will lose.**

Keeper drunk → Team A loses → **you want maximum money on Team A**, because when A loses you keep every stake. So you move **−400 → −300**, not −500.

| Your price | Punter risks | Who bites | A loses → you keep |
|---|---|---|---|
| **−500** (expensive) | $500 to win $100 | ~3 of 10 | $1,500 |
| **−300** (cheap) | $300 to win $100 | ~8 of 10 | **$2,400** |

- Moving to −500 makes Team A *expensive to back* and **chases off the exact money you were trying to catch.** Sweetening pulls it in.
- **Exposure is the price of the edge.** If the tip is fake and A wins, you pay out generously on inflated volume: **−500 costs you $300, −300 costs you $800.** Sweetening earns more when right and costs more when wrong — *the player risks their moneyline to earn more*, exactly as intended.
- **Fake intel exists.** Distinguishing a real tip from a planted one is the skill that makes sweetening a decision instead of a formula. (New mechanic — see §8.4.)

### 9.5 Season rollover

- **Tier ① stats shift for every team: same style, different strength.** §7's tier ① is qualitative-only, so the *structural* read survives — they still play through their striker; that striker is just a different, slightly worse man. **The player loses half their read, not all of it.**
- **Persistent meter decays −10 at rollover.** The fiction picked the number: shape held, strength moved.
- **⚠️ Hard constraint: decay must stay under +20/season.** Away-only teams (the city's four, locked until Act 3) gain only **+20/season** (two visits to your stadium). At −20 or worse they rebuild exactly what they lost, every season, forever, and **the +30 away cap becomes unreachable in principle** — Act 3's payoff never fully arrives. At −10 they net +10/season, hit the away cap around season 2, and hover at 30–40 until the metro opens. **The away ladder finishes exactly as the home ladder becomes available.** Clean handoff, no dead zone.
- **This is what keeps pillar 2 an economy instead of a climb.** The world moves, so knowledge rots. Information is currency, and currency has to be re-earned.
- **Tier ② gets a second job:** the newspaper reports who churned over the off-season → the player decides where to spend next season's 14 stadium visits. **The off-season becomes a planning beat**, and the newsstand already sited downtown earns its rent twice.

### 9.6 Fixture calendar

- 8 teams, double round-robin = **56 matches, 14 rounds**.
- **4 matches per round (all 8 play), 1 round per week, matches spread across 4 days** (Mon/Wed/Fri/Sun). A match to bet on **every ~2 days**, but each team still plays weekly — like real football.
- **14 rounds = 1 season = 98 game days ≈ 33 real hours = Act 1.** Act 1 is exactly one season. Nothing was forced; the existing numbers land there (§3.6).
- Act 1's stadium hosts **14 matchdays** a season — Aberdeen's 7 home + Eastport's 7 home. That is the player's entire tier ① budget in Act 1.

**Attend all 14 and you end Act 1 at:**

| Team | Home | Away | Tier ② | **Total** |
|---|---|---|---|---|
| Aberdeen | +45 | +10 (the derby) | +10 | **65** |
| Eastport | +45 | +10 (the derby) | +10 | **65** |
| The other 6 | — | +20 (2 visits) | +10 | **30** |

**What that buys, for free:**
- **You enter Act 2 a rookie bookmaker who genuinely knows two teams.** Your book prices the derby tight and bleeds a wide range on everything else. That is the first half's tension, and nobody had to design it.
- **Ranges tighten by attendance, never by money.** Pillar 4 holds without a rule.
- **To push your local teams past 65 you must watch them away** — which means leaving town, which is exactly what the Act 2 midpoint opens. **The familiarity ladder pulls you out of town at the moment the bridge to Town 2 opens.** Knowledge is geography, delivered by the calendar.
- **City teams cap at 40/85 through Acts 1–2** (away ×3 = +30, tier ② = +10). **Harbour FC is unpriceable until Act 3** — every fixture involving the giant is a wide range your own book must eat, and the edge therefore lives entirely with the small clubs you have stood on the terraces watching. Pillar 2 as a number.
- **Then the irony lands:** Act 3 opens the metro, +45 arrives, Harbour jumps to 85 — **your book is finally safe at the exact moment you start manufacturing the results.** Three acts spent learning to price the truth, and the reward is that you no longer need it.

### 9.7 What changes at the Act 2 midpoint

| | First half — illegal book | Second half — legal network |
|---|---|---|
| **Intel** | Gather it yourself | Gather it yourself (unchanged — this is the skill) |
| **Setting the line** | Walk back to the arcade back room | **Phone it in** |
| **Operating** | Stand at the counter when punters come | Hired staff |
| **Focus** | Price the book, read the sharps | Gather intel, explore town 2, open shops, renovate, build district connections |

**⚠️ [OPEN #3: booth or brick?] Recommended: phone booths, not a mobile.**

§2.2 already sites **phone booths in downtown and residential — not the docks, not rural.** Keep it there and everything holds:
- You scout the training ground (rural, no booths) → **you still have to reach one to phone it in.** The morning race survives.
- **Calls only go out.** In 1990 your staff cannot reach you — so you check in, booth to booth, shop by shop, and that is when you hear *"a fella came in and put five grand on Aberdeen."* **The phone becomes the sharp-detection loop**, and it costs time and legwork.
- **A mobile brick kills both.** Hold it back as the *last* convenience: when the player no longer has to be anywhere, the game has nothing left to ask of them — which is precisely when Act 3 must hand them a reason to move again (§1.2's ceiling).

中文摘要（§9）：**成個身份翻轉住喺一個機制:同一個動詞,相反用法**——第1幕你知門將宿醉→你**落注**;第2幕你知門將宿醉→你**𠝹盤**,等唔知嘅人食錯價。**9.1 Familiarity meter**(每隊0-100):主場睇3次+45(全讀)、作客睇3次+30(半讀,§7上限)、第②層+10(逐場、唔累積)。**上限85,個15分窿係故意嘅——你永遠定唔到一場波嘅完美價**(支柱4嘅兄弟:冇嘢買得到確定性)。**9.2 Range**:闊度隨meter線性,**⚠️必須以勝率百分比定義,只喺顯示時轉美式賠率**(固定美式闊度喺−110係24個百分點、喺−600係2個——個錶會呃玩家,而且喺落注最有趣嗰度呃得最勁)。**Range有牙,隻牙就係sharps**:寬range=你靠估=舖頭嗰個sharp食你錯價。**Familiarity係防套利嘅盔甲,唔係提示系統。9.3 時序**:3-5日前set→2日前全世界同時開盤(**唔可以抄**)→前一日摸料改盤。**2日前嘅開盤本身就係情報,而第2幕前半佢對準肥強**(睇住佢三間舖嘅價先落最後一手=劈價搶客=拆佢嘅動機由賠率板長出嚟)。**9.4 𠝹盤方向:餵靚你知會輸嗰邊**——門將醉→A輸→你要最多錢押A→**−400郁去−300,唔係−500**(−500令A唔抵買,趕走你正正想收嗰筆錢)。**曝險就係edge嘅代價**:料係流嘅,−300賠$800、−500只賠$300。**9.5 換季**:打法留低、強度變,**衰減−10**,**⚠️硬約束:必須少過+20/季**(否則大城隊永遠摸唔到作客上限,第3幕payoff永不到達)。**世界會郁,知識會腐爛——呢個令支柱2由「攀升」變成「經濟」。9.6 賽程**:每輪4場、一週一輪、分散4日、**14輪=1季=98日≈33鐘=第1幕**。第1幕全勤 → 本地兩隊65、其餘六隊30。**你入第2幕係個真係只識兩隊嘅新手莊家**;**range靠出席收窄,永遠唔靠錢**;**要推高本地隊過65就要睇作客=出鎮=中段開山路**(知識=地理);**大城隊兩幕封頂40/85——Harbour定唔到價,edge只住喺你熟嘅細club**;**第3幕地鐵一開,佢彈到85——你本書終於安全嗰一刻,正正就係你開始親手製造賽果。9.7 中段之後**:情報仍然自己摷(呢個先係技術),但落盤改用**電話亭**(⚠️OPEN:亭定大哥大?建議亭)——**電話只出唔入**,你要逐個亭check in,而嗰陣你先聽到「有人落咗五千喺漁民度」:**電話變成偵測sharp嘅loop**。大哥大留返做最後一件便利品。

---

## 10. Build handoff — `design/constants.json` reconciliation

**Owner:** build session. **Blocker:** must be verified before any of it is touched.

1. **⚠️ Confirm `starting_debt`, `weekly_debt_interest` and `catch_vig` are dead to the match engine and the `validate` harness.** If `validate` reads them, the **Phase 1 freeze applies and nothing moves.** Everything below is contingent on this check. **UPDATE 2026-07-28: `starting_debt 500000→100000` is DONE and the full proof re-ran identical (commit `cd19123`) — which proves that field is inert to the engine+harness. The other two are its siblings.**
2. ~~`starting_debt: 500000` → `100000`~~ **DONE** (§3.5).
3. `weekly_debt_interest: 0.1` → **rename + re-semantic.** §3.5 specifies a monthly minimum with +10% on the *missed installment*, not weekly interest on the principal. → moves to a life-sim economy value owned by **wd:§3.5** (suggested `overdue_installment_penalty: 0.10`).
4. ~~`catch_vig: 0.3` → §4.2 ratifies or the constant goes.~~ **RATIFIED 2026-07-28 at 0.30, owned by wd:§4.2** (§4.2 now defines it as a 30% surcharge on the seized amount). The *value* is settled; item 5 governs where it *lives*.

### 10.1 The engine-eviction pass (decision c) — **APPROVED, conditioned**

The build session reports items 3–4 are **blocked by the Phase 1 freeze**: `starting_debt`, `weekly_debt_interest` and `catch_vig` are `required` members of the FROZEN engine's `EngineConfig`, so renaming/deleting them edits the frozen public API. Recommendation: one separately-reviewed pass that evicts all three world-economy fields from `EngineConfig` at once, verified by an identical Phase 1 proof.

**Verdict: APPROVE.** These three are life-sim values that were mis-parked in a match engine — they never belonged there, and every future economy change (monthly payment, deposit cap, …) otherwise either can't touch them or forces an engine edit. The surface is small now (3 fields); it only grows if deferred. The `cd19123` result already demonstrates the class is inert (a value change left the proof byte-identical).

**Conditions (all four, or it does not ship):**
1. **Its own `/codex-review` plan** — reviewed as an isolated engine-plumbing change, not folded into map/greybox work.
2. **Scope = exactly these three fields** (+ their `required` markers, JSON, and any test references). **Zero** changes to match math, event logic, or the pricing pipeline.
3. **Full Phase 1 proof re-runs byte-identical** — 9/9 + 6/6, both validators, `validate`. If any number moves, revert; the field was NOT inert and the freeze holds.
4. **Tag before and after** (`pre-engine-eviction` / `post-engine-eviction`).

**This is a one-way door — treat it as one.** It is the *only* sanctioned reason to touch the frozen engine, and only because the fields are provably not engine state. **Relocation, not deletion:** the values move to the greybox economy block with provenance — `starting_debt`→wd:§3.5, `overdue_installment_penalty`→wd:§3.5, `catch_vig`→wd:§4.2.

### 10.2 Build-session items d & e (recorded — build decisions, not §8.1 design locks)

- **(d) Gate-2 sprint integrity — RECOMMEND (a)-partial: implement the sprint→stamina *drain* now, defer the *consequence*.** §2.2.2's Energy is the designed counterweight, but Energy is deferred out of the slice, so free unlimited sprint lets the player blow past the single patroller and **Gate 2 would measure the corridor tension with its counterweight switched off** — a gate that passes without testing what it exists to test. Implement a **bounded sprint** (sprint depletes a stamina reserve → forced back to walk until it regenerates), which is a **strict subset of §2.2.2 Energy** (nothing thrown away — it becomes Energy's sprint-drain input when Energy lands) and restores the "can't just sprint the whole corridor" pressure. **Defer the full consequence** (Energy 0 → forced sleep → 50% cash-loss) with the rest of the survival system, and **stamp the Gate-2 result: "walk-home-with-winnings cash-loss tension deferred; re-verify when Energy lands."** Drain/regen rates = **TUNE** (feel-test), owner **wd:§2.2.2**. *Fallback if build capacity is tight:* (b) accept free sprint and stamp the caveat loudly — but a gate with a known trivialising exploit is a weak gate, so (a)-partial is the rec.
- **(e) Canned Gate-2 payout — it's a correctness bug, not a design choice: pay off the real odds.** The board prices the fixture (~1.29 decimal); the payout MUST be `stake × board_odds`, not the hardcoded 2.0. Fix = **option (1): compute the payout from the board odds** the engine already produces — do not store a payout constant. Keep the fixture (Harbour heavy favourite is a legitimate Act-1 bet) and the $1,000 stake: the ~$1,290 returned to hand **already exceeds the $500 gang threshold**, so the Gate-2 walk-home tension (§2.2.3) is live without a fixture change. No new stored value; provenance = derived-from-engine.

### 10.3 Provenance ledger — values locked/moved this round (for greybox.json citation)

| Value | Disposition | Cite |
|---|---|---|
| `catch_vig` | 0.30, ratified | `wd:§4.2` |
| `starting_debt` | 100000, done | `wd:§3.5` |
| `overdue_installment_penalty` | 0.10 (replaces `weekly_debt_interest`), pending eviction | `wd:§3.5` |
| Town-1 building coords / footprints / street widths | locked | `wd:§2.2.3` |
| Sprint stamina drain/regen | to implement | `wd:§2.2.2`, **TUNE** |
| Gate-2 payout | derived from board odds | (no constant) |

**Process note for `greybox.json`:** three of five recently-flagged "value gaps" were already decided in this document — the build session **cannot tell locked from open** because the file does not say. **Every value should be either doc-cited or marked `TUNE`** (`cash_carry_threshold` → `§4.2`; `heat_detect_curve` → `TUNE`). `tools/validate_constants.py` is the natural enforcement point. Same rule for scaffolding: an ungated bribe or a placeholder flee roll is **fine as scaffolding and must be marked as scaffolding** — otherwise it is the design by the time anyone checks.

中文摘要（§10）：**動 `constants.json` 之前必須先確認嗰三個欄位同 match engine / `validate` 完全無關——如果 `validate` 讀到,Phase 1 凍結生效,一律唔准郁。**然後:`starting_debt` → `100000`;`weekly_debt_interest` → 改名兼改語意(月供+逾期嗰期罰10%,唔係本金週息);`catch_vig: 0.3` → 要麼 §4.2 正式採納,要麼剷。**`greybox.json` 流程:每個值要麼標doc出處,要麼標 `TUNE`**——最近五個「value gap」入面三個本文件早就定咗,build session分唔出鎖定同開放,**係因為檔案冇講**。鷹架同理:冇閘嘅賄賂、暫代嘅flee骰,**當鷹架冇問題,但一定要標明係鷹架**,唔係等有人翻返轉頭,佢就已經係設計。
