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

## Part 2 — Scale test scene

This section is separate from the bridge smoke test. Do not delete or rename `BridgeSmokeTestRunner`. The point here is to feel whether the locked 700m scale is right before any buildings exist.

Expected figures to keep in mind:

- `700m x 700m` square -> diagonal about `990m`.
- Walking at `2.5 m/s` -> about `6.6` real minutes for the diagonal.
- Sprinting at `5.0 m/s` -> about `3.3` real minutes for the diagonal.
- With `greybox.clock.real_minutes_per_game_day = 20`, a `6.6` real-minute diagonal is about `7.9` in-game hours.

If this feels bad, that is a finding, not a failure. This test exists to challenge the design decision.

1. Open the sample scene again if needed.
   What to click: in the `Project` window open `Assets` -> `Scenes`, then double-click `SampleScene`.
   What should appear: the Hierarchy shows `SampleScene` contents, including `BridgeSmokeTestRunner`.
   What it means if it does not: the wrong scene is open, or the bridge smoke-test object was not saved.

2. Run the one-click builder.
   What to click: top menu `Ball Knowledge` -> `Build Scale Test Scene`.
   What should appear: a large grey ground appears, marker posts appear along two edges, a different-coloured far-corner marker appears, and a `Player` object with child `PlayerCamera` appears in the Hierarchy.
   What it means if it does not: if nothing new appears, the editor scripts did not compile. Open the Console and capture the first red error.

3. Confirm the new scene objects look sane.
   What to click: in the `Hierarchy`, click `BallKnowledgeScaleTestScene`, then expand it.
   What should appear: a root object containing the ground, markers, and `Player`. `BridgeSmokeTestRunner` should still exist outside that root. There should not be duplicate scale-test roots from repeated runs.
   What it means if it does not: if duplicates stack up, the builder is not replacing its own objects cleanly.
   Screenshot to take: the Hierarchy showing `BallKnowledgeScaleTestScene`, `Player`, and `BridgeSmokeTestRunner`.

4. Check the greybox assembly reference for Input System.
   What to click: in the `Project` window open `Assets` -> `Scripts`, then click `BallKnowledge.Greybox`.
   What should appear: in the Inspector, `References` includes `Unity.InputSystem`.
   What it means if it does not: `FirstPersonController` cannot resolve `UnityEngine.InputSystem` and Play mode will fail.

5. Enter Play mode.
   What to click: the top-center `Play` button.
   What should appear: the view switches to the player camera, the cursor locks/hides, and an on-screen black HUD box appears in the top-left with `Speed`, `Distance`, `This trip`, `Elapsed`, `Game time`, and the controls line.
   What it means if it does not: if the HUD is missing, or the cursor never locks, or a red error appears, stop and capture the Console.

6. Verify walk, sprint, crouch, reset, and free-cursor behavior.
   What to click: press `WASD` to move, hold `Shift` to sprint, hold `Ctrl` to crouch, press `R` to reset `This trip`, and press `Esc` to free the cursor.
   What should appear: `Speed` should read about `2.5 m/s` while walking, `5.0 m/s` while sprinting, and `1.25 m/s` while crouched. `This trip` should reset to `0.0 m` when `R` is pressed. `Esc` should release the cursor so you can get the mouse back.
   What it means if it does not: if any number is materially off, the controller is not obeying `greybox.json`; if `Esc` does nothing, the mouse-lock escape path is broken.
   Screenshot to take: the HUD while walking and while sprinting.

7. Walk the diagonal.
   What to click: from the start corner, walk toward the opposite far-corner marker. You may also sprint part of the way for comparison, but do at least one full walk-speed run.
   What should appear: `Distance` and `This trip` climb steadily, `Elapsed` advances in real time, and `Game time` advances faster than real time. The edge markers should make progress visible without opening code.
   What it means if it does not: if the HUD numbers stall, drift wildly, or the edge markers are missing/unreadable, the scale test is not trustworthy.

8. Compare the far-corner numbers against the expected diagonal.
   What to click: stop near the far-corner marker and read the HUD.
   What should appear: for a full diagonal walk, `This trip` should be close to `990m`, `Elapsed` should be about `06:36`, and `Game time` should be about `7.9 h`. A mostly sprinted diagonal should land around `03:18`.
   What it means if it does not: if the distance is far from `990m`, the scene scale is wrong; if the elapsed time is far from the expected pace, the movement speeds are wrong.
   Screenshot to take: the HUD at the far corner after a full walk-speed diagonal.

9. Exit Play mode and save only if you want to keep the built scene objects.
   What to click: click the top-center `Play` button again; if you want the scene objects kept on disk, then use `File` -> `Save`.
   What should appear: Play mode ends and the scene returns to edit state.
   What it means if it does not: if the editor stays in Play mode or the scene does not return cleanly, capture the Console.

10. Report back these exact observations.
   What to report:
   `1.` How the diagonal walk felt in plain language.
   `2.` The HUD numbers you saw near the far corner for `This trip`, `Elapsed`, and `Game time`.
   `3.` Whether `700m at 2.5 m/s` felt like "a real commitment" or like "a slog".
   `4.` Any mismatch in walk/sprint/crouch speed readouts, marker visibility, or cursor unlock behavior.
