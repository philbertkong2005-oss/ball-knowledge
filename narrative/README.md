# Narrative source 敘事原始檔

This folder holds the game's **written story** — characters, dialogue, quests, unique events, and the branching between choices. It is authored in **Twine** and stored here as `.twee` text files so git can version and diff it like any other source.

本資料夾存放遊戲的**書面故事**——角色、對白、任務、獨特事件、以及選擇之間的分支。用 **Twine** 創作，以 `.twee` 純文字檔存放，讓 git 可以像其他原始碼一樣做版本控制與比對。

## Why Twine 為什麼用 Twine

- Free, runs in your browser, no code and no Unity needed. 免費、瀏覽器運行、無需代碼或 Unity。
- You *see* the whole story as visual boxes and arrows — perfect for tracking branching. 你能*看到*整個故事是視覺化的方框與箭頭——最適合追蹤分支。
- This is your creative work and it can start now, in parallel with the engine. 這是你的創作，現在就能開始，與引擎開發並行。

## How to start 如何開始

1. Go to `twinery.org` and click **Use it online** (or download the desktop app). 到 `twinery.org` 點 Use it online（或下載桌面版）。
2. Choose **Library → Import → From File**, and import `act1-opening.twee` from this folder. 選 Library → Import → From File，匯入本資料夾的 `act1-opening.twee`。
3. You'll see a starter story map. Edit it, add passages, drag arrows. 你會看到一個起步故事地圖。編輯它、加段落、拉箭頭。
4. When you want to save back into the project: **Library → (your story) → Publish/Export → as Twee**, overwrite the file here, then commit. 想存回項目時：Publish/Export → as Twee，覆蓋這裡的檔案，然後提交。

## Rule 守則

Write as much as you want — it's free fuel. But this text does **not** get wired into Unity until **Phase 3**, and only after the core day loop is proven fun (Gate 3). See `docs/design-doc.md` → Narrative Authoring Track. 盡情寫——這是免費燃料。但這些文字要到**第三階段**、且核心循環證明好玩之後（Gate 3）才接入 Unity。詳見 `docs/design-doc.md` 的敘事創作支線。
