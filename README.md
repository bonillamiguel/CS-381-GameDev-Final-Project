# Unprompted

**Unprompted** is a top-down 2D roguelite shooter for Windows, built in Unity 6 for CS 381 (Game Development).

You play a software engineer who dives into the archive of an AI called **ASSIST**, which is full of parody games. Fight your way through three levels:

1. **Cyberjunk 2177**: a sci-fi parody
2. **Elden Necklace**: a fantasy parody
3. **The AI's Core**: where ASSIST lives

## Controls

| Action        | Input        |
| ------------- | ------------ |
| Move          | `W` `A` `S` `D` |
| Aim           | Mouse        |
| Attack        | Left click   |
| Special       | Right click  |
| Dash          | `Space`      |
| Pause         | `Esc`        |

## Setup

You only do this once per machine.

### 1. Install the tools

- **Unity 6000.6.0f1**, installed through Unity Hub. Use this exact version. A different editor version will upgrade the project files for everyone.
- **Git** and **Git LFS**. After installing Git LFS, run this once:

  ```sh
  git lfs install
  ```

  Audio (`.wav`, `.mp3`, `.ogg`), `.psd`, `.aseprite` and video files are stored in LFS (see `.gitattributes`). If you skip this step, you'll get small pointer files instead of real assets.
- **VS Code** with the **Unity** extension (`visualstudiotoolsforunity.vstuc`). VS Code suggests it when you open the repo, because it's listed in `.vscode/extensions.json`. It installs C# Dev Kit for you.

### 2. Clone and open

```sh
git clone https://github.com/bonillamiguel/CS-381-GameDev-Final-Project.git
```

Open the folder in Unity Hub with **Add → Add project from disk**. The first open takes a while, because Unity builds the `Library/` folder.

In Unity, go to **Edit → Preferences → External Tools**, set **External Script Editor** to **Visual Studio Code**, then click **Regenerate project files**.

### 3. Set up Unity Smart Merge

Scenes, prefabs and other Unity YAML files are merged with Unity's `UnityYAMLMerge.exe`. `.gitattributes` assigns them to a merge driver called `unityyamlmerge`, but Git doesn't know where that program is until you tell it. Run these commands from the repo folder (they work in PowerShell or Git Bash):

```sh
git config merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
git config merge.unityyamlmerge.driver "'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Data/Tools/UnityYAMLMerge.exe' merge -p %O %B %A %A"
git config merge.unityyamlmerge.recursive binary
```

If Unity Hub installed Unity somewhere else, change the path. It's `<Unity install>/Editor/Data/Tools/UnityYAMLMerge.exe`.

Smart Merge reduces conflicts but doesn't remove them. Avoid having two people edit the same scene or prefab at the same time.

### 4. Download the bundled Java runtime

The game runs Java code at runtime using a JDK bundled under `Assets/StreamingAssets/jdk/`. That folder is too big for Git, so it's ignored, and **each person downloads it separately**:

1. Go to [Eclipse Temurin](https://adoptium.net/temurin/releases/?os=windows&arch=x64&package=jdk) and download the **JDK** for **Windows x64** as a **`.zip`**. Don't use the `.msi` installer.
2. Unzip it. You'll get a folder named something like `jdk-<version>`.
3. Rename that folder to `jdk` and move it to `Assets/StreamingAssets/`. Create `StreamingAssets` if it doesn't exist.
4. Check that it worked. From the repo folder in PowerShell:

   ```powershell
   Test-Path Assets\StreamingAssets\jdk\bin\java.exe   # should print True
   .\Assets\StreamingAssets\jdk\bin\java.exe -version
   ```

   If `java.exe` is at `Assets/StreamingAssets/jdk/jdk-<version>/bin/java.exe`, you have one folder level too many. Move the contents up a level.

Unity imports the JDK the next time it opens, which takes a minute. Never commit anything from this folder.

## How we work

1. **Issue.** Every piece of work starts as a GitHub issue that uses the Task or Bug template. Label it (`area:*`, `type:*`, `priority:*`) and add it to a milestone.
2. **Branch.** Create a branch from `main` named `<issue number>-<short-name>`, for example `12-player-dash`.
3. **Pull request.** Open a PR using the template. Put `Closes #<issue>` in the description and work through the definition-of-done checklist.
4. **Review and play-test.** At least one other person reviews the code **and plays the build** before merging. Play-test notes go in [`docs/feedback-log.md`](docs/feedback-log.md).

Other conventions:

- Our code lives in `Assets/_Project/Scripts/<Area>/`, using the namespace `Game.<Area>`. Third-party assets go in `Assets/ThirdParty/`.
- Move or rename assets **inside the Unity Editor**, never in Explorer or Git, so each `.meta` file moves with its asset.
- Rules for AI coding agents are in [`AGENTS.md`](AGENTS.md).
- Weekly devlogs go in [`docs/devlog/`](docs/devlog/), starting from [`TEMPLATE.md`](docs/devlog/TEMPLATE.md).

## Credits

### Team

| Name | Role | Areas |
| ---- | ---- | ----- |
|      |      |       |

### Third-party assets

| Asset | Author | License | Link |
| ----- | ------ | ------- | ---- |
|       |        |         |      |
