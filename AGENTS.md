# AGENTS.md

## Project
"Unprompted": top-down 2D roguelite shooter in Unity 6000.6.0f1, Windows only.
A software engineer dives into parody games in an AI's archive: Level 1 Cyberjunk 2177
(sci-fi), Level 2 Elden Necklace (fantasy), Level 3 the AI's Core. The AI antagonist is ASSIST.

## Where code goes
- Our code: Assets/_Project/Scripts/<Area>/  (namespace Game.<Area>)
- Never edit anything under Assets/ThirdParty/, Assets/StreamingAssets/jdk/, Library/,
  ProjectSettings/ or Packages/

## Rules
- Do NOT hand-edit .unity, .prefab or .asset files. Describe the Editor steps instead.
- Do NOT move or rename files; a .meta file must always travel with its asset.
- Use the shared contracts in Scripts/Core (IDamageable, GameEvents, data ScriptableObjects).
  Do not change them without saying so: other people depend on them.
- Systems talk through GameEvents, not direct references. Never use GameObject.Find by name.
- Tunable numbers go in [SerializeField] private fields or ScriptableObjects, never literals.
- Dialogue lines and coding problems live in data, not in code.
- One MonoBehaviour per file, file name = class name. No new packages without asking.
- The Java runner must never block the main thread and must kill the process after a timeout.
- Finish every task with: files changed + the exact Editor steps a human must do + how to test it.
