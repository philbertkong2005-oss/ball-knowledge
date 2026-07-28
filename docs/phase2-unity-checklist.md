# Phase 2 Unity Bridge Checklist

This checklist is for the human who does the Unity editor clicks. Follow the steps exactly and compare what you see against the expected result after each step.

中文摘要：按順序開 Unity、等 package 解決、加一個空物件、掛 `BridgeSmokeTest`、按 Play，Console 必須出現 5 行指定文字。

## 1. Open the project and let Unity finish importing

1. Open Unity Hub.
   What to click: `Projects` -> the `ball-knowledge/unity` project.
   What should appear: the Unity editor opens and the bottom-right progress bar starts importing assets.
   What it means if it does not: if the project does not open, the wrong folder was selected or Unity 6.3 (`6000.3.19f1`) is missing.

2. Wait until the editor is idle.
   What to click: nothing; wait for the bottom-right import/progress spinner to disappear.
   What should appear: no compiling/importing spinner, no red errors in the Console.
   What it means if it does not: if Unity stays compiling forever or shows red errors immediately, stop and capture the Console.
   Screenshot to take: after the import spinner disappears and before clicking anything else.

## 2. Confirm the Newtonsoft package resolved

3. Open the Package Manager.
   What to click: top menu `Window` -> `Package Management` -> `Package Manager`.
   What should appear: a Package Manager window.
   What it means if it does not: the editor layout is broken; reset layout or reopen the editor.

4. Check that the Newtonsoft package is installed.
   What to click: in Package Manager, select `In Project` in the left sidebar, then look for `Newtonsoft Json for Unity`.
   What should appear: package `Newtonsoft Json for Unity`, version `3.2.1`, with no install button.
   What it means if it does not: **this is expected if the editor was already open when the project files changed** — Unity only resolves new packages on project load, so it has not noticed the request yet. Do step 4a.
   Screenshot to take: the Package Manager showing `Newtonsoft Json for Unity 3.2.1`.

4a. **If Newtonsoft is NOT in the list — install it explicitly** (observed 2026-07-28; do not skip).
   What to click: the `+` button at the top-left of the Package Manager window -> `Install package by name...` -> paste `com.unity.nuget.newtonsoft-json` -> `Install`.
   What should appear: a progress spinner, then `Newtonsoft Json for Unity 3.2.1` in the `In Project` list, then a short recompile.
   What it means if it does not: if the `+` menu has no "by name" entry, quit Unity completely and reopen the project — package resolution always runs on project load. If it still does not appear, stop and report: `Packages/manifest.json` requests it on line 7, so a failure here is a Unity/registry problem, not a project problem.
   Why this step exists: the package request lives in `Packages/manifest.json`, but Unity keeps its own resolved list (`packages-lock.json`). An editor that was open during an external file change will not re-read the manifest on its own.

5. Confirm the greybox assembly references are correct.
   What to click: in the `Project` window open `Assets` -> `Scripts`, then click `BallKnowledge.Greybox`.
   What should appear: in the Inspector, `Name` = `BallKnowledge.Greybox`; `Override References` is checked; `Precompiled References` lists `BallKnowledge.MatchEngine.dll` and `Newtonsoft.Json.dll`.
   What it means if it does not: the scripts will not compile against the engine DLL and Newtonsoft package.

6. Confirm the engine DLL importer is still auto-referenced.
   What to click: in the `Project` window open `Assets` -> `Plugins`, then click `BallKnowledge.MatchEngine.dll`.
   What should appear: in the Inspector, `Auto Referenced` is checked.
   What it means if it does not: other assemblies may fail to see the plugin DLL.
   Screenshot to take: the Inspector for `BallKnowledge.Greybox` or `BallKnowledge.MatchEngine.dll`, whichever clearly shows the settings above.

中文摘要：Package Manager 要見到 `Newtonsoft Json for Unity 3.2.1`；`BallKnowledge.Greybox` 要勾 `Override References`，列表有兩個 DLL。

## 3. Open the Console and clear old messages

7. Open the Console window.
   What to click: top menu `Window` -> `General` -> `Console`.
   What should appear: the Console panel.
   What it means if it does not: use the Layout menu to restore the default layout, then try again.

8. Clear the Console.
   What to click: in the Console, click the broom/`Clear` button.
   What should appear: the Console becomes empty.
   What it means if it does not: old logs can be mistaken for the smoke test result.
   Screenshot to take: the empty Console before Play mode.

## 4. Add the smoke-test object to the scene

9. Open the sample scene.
   What to click: in the `Project` window open `Assets` -> `Scenes`, then double-click `SampleScene`.
   What should appear: the Hierarchy shows `SampleScene` contents and the Scene view updates.
   What it means if it does not: the wrong scene is open.

10. Create the empty GameObject.
   What to click: in the `Hierarchy`, right-click empty space -> `Create Empty`.
   What should appear: a new object appears in the Hierarchy.
   What it means if it does not: the click landed on the wrong target.

11. Rename the object exactly.
   What to click: select the new object, press `F2` or slow double-click the name, then type `BridgeSmokeTestRunner`.
   What should appear: the Hierarchy object name becomes exactly `BridgeSmokeTestRunner`.
   What it means if it does not: the checklist and screenshots will not match the expected setup.

12. Add the script component.
   What to click: with `BridgeSmokeTestRunner` selected, in the Inspector click `Add Component`, search for `Bridge Smoke Test`, and click it.
   What should appear: a `Bridge Smoke Test` component is attached under the Transform.
   What it means if it does not: if the script will not attach, there is still a compile/import problem.

13. Save the scene.
   What to click: top menu `File` -> `Save`.
   What should appear: Unity stops showing the scene as modified.
   What it means if it does not: the runner object may disappear next time the scene opens.
   Screenshot to take: the Hierarchy showing `BridgeSmokeTestRunner` selected with the `Bridge Smoke Test` component visible in the Inspector.

中文摘要：在 `SampleScene` 建立空物件，名字一定要是 `BridgeSmokeTestRunner`，再掛上 `Bridge Smoke Test` component。

## 5. Press Play and compare the Console output

14. Enter Play mode.
   What to click: the top-center `Play` button.
   What should appear: the Play button turns blue and the game starts.
   What it means if it does not: Unity is still compiling or there are blocking errors.

15. Read the five Console lines exactly.
   What to click: nothing; watch the Console.
   What should appear: these exact five log lines, in this order:

```text
[BallKnowledge] Config OK — conversion_base=0.205 home_advantage=1.24 formations=10
[BallKnowledge] Teams OK — 8 teams, first=Harbour FC
[BallKnowledge] Greybox OK — walk=2.5 sprint=5 carry_threshold=500 debt=100000
[BallKnowledge] Canned bet seed=8 -> Harbour FC 2-0 Eastport Rovers  [EXPECTED: 2-0 home win]
[BallKnowledge] BRIDGE SMOKE TEST PASSED
```

   What it means if it does not: any red error, any missing line, any different number, or any `0` value means the bridge is not working and should be treated as a bug.

16. Exit Play mode after confirming the output.
   What to click: click the top-center `Play` button again.
   What should appear: the Play button returns to normal and the editor exits Play mode.
   What it means if it does not: the editor may be stuck or still compiling.
   Screenshot to take: the Console showing all five expected lines.

## Troubleshooting

| Problem | What to check | What it usually means |
| --- | --- | --- |
| Script will not attach | Open Console and look for red compile errors | Unity scripts did not compile; fix the first red error before trying again |
| Newtonsoft missing | Package Manager does not show `Newtonsoft Json for Unity 3.2.1` | Unity has not re-resolved the manifest (normal if the editor was open when the files changed). Fix with step 4a: `+` -> `Install package by name...` -> `com.unity.nuget.newtonsoft-json`. Quitting and reopening the project also works |
| `The type or namespace name 'BallKnowledge' could not be found` | Click `Assets/Scripts/BallKnowledge.Greybox` and confirm `Override References` is checked and `BallKnowledge.MatchEngine.dll` plus `Newtonsoft.Json.dll` are both listed | The asmdef is not referencing the engine DLL or Newtonsoft correctly |
| Zeros in the config line | Compare the first line against `conversion_base=0.205 home_advantage=1.24 formations=10` | The engine JSON resolver is wrong or not being used; default Newtonsoft silently bound zeros |
| Red errors on import | Read the first red Console entry after opening the project | Package restore, asmdef references, or a syntax/import issue blocked compilation |

## Required screenshots

1. After Unity finishes importing.
2. Package Manager showing `Newtonsoft Json for Unity 3.2.1`.
3. The Inspector showing the `BallKnowledge.Greybox` references or the plugin DLL `Auto Referenced` checkbox.
4. Empty Console before Play mode.
5. `BridgeSmokeTestRunner` selected with the `Bridge Smoke Test` component attached.
6. Console showing the five expected smoke-test lines.
