# Unity Setup

Follow these steps exactly. The goal is a blank Unity project in the `unity/` folder, using a built-in `3D` template and a specific Unity `6 LTS` editor version.

## 1. Install Unity Hub

1. Go to `https://unity.com/download`.
2. Download and install **Unity Hub**.
3. Open Unity Hub and sign in if Hub asks you to.

## 2. Install One Exact Unity 6 LTS Editor Version

1. In Unity Hub, open the **Installs** tab.
2. Click **Install Editor**.
3. Choose **Unity 6 LTS**.
4. Pick one exact editor version and write the full version string down exactly as shown in Hub.
5. Install only the editor modules you actually need right now. The goal is the lightest clean install that lets the blank project open and play.

Do not do these things:

- Do not choose a beta, alpha, or tech stream version.
- Do not choose `Universal 3D`, `URP`, `HDRP`, or any other render-pipeline-specific template.
- Do not add extra Unity packages during Phase 0.

## 3. Prepare the `unity/` Folder

This repo already contains placeholder files so git can track the folder. Unity Hub may refuse to create a project in a non-empty folder.

If Unity Hub complains that `unity/` is not empty, delete these placeholder files right before you create the project:

- `unity/.gitkeep`
- `unity/README.txt`

That is safe. The real Unity project files will replace them.

## 4. Create the Project in the `unity/` Subfolder

1. In Unity Hub, go to **Projects**.
2. Click **New project**.
3. Choose the built-in **3D** template.
4. Set the project name to `unity`.
5. Set the location so the final project folder is exactly:

   `C:\Users\philb\Projects\ball-knowledge\unity`

6. Create the project.

Stop and fix the location if Unity Hub tries to put the project anywhere else.

## 5. Verify the Blank Project

1. Wait for Unity to finish importing.
2. Open the default blank scene if it is not already open.
3. Press the **Play** button.
4. Confirm the empty scene runs without errors in the Console.
5. Stop Play mode.

If the blank scene does not open and play cleanly, fix that before any gameplay work starts.

## 6. Record the Authoritative Editor Pin

After the project exists, the file below becomes the source of truth for the Unity editor version:

`unity/ProjectSettings/ProjectVersion.txt`

That file, once committed, is the authoritative editor-version pin for this repo.

After project creation:

1. Open `unity/ProjectSettings/ProjectVersion.txt`.
2. Copy the exact Unity version string from that file.
3. Record that exact version string in `README.md`.
4. Commit `unity/ProjectSettings/ProjectVersion.txt` with the rest of the Unity project files during the first manual commit.

## 7. What Not To Do

- Do not install extra packages just because Hub suggests them.
- Do not switch the project to URP or HDRP.
- Do not treat `unity/Packages/manifest.json` as the editor pin. It pins packages, not the Unity editor itself.
