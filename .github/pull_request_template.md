Closes #

## What changed

<!-- Short summary of the change. -->

## Files changed

<!-- Main scripts, scenes, prefabs or assets touched. -->

## Editor steps

<!-- Anything a person must do in the Unity Editor after pulling (assign references, add components, rebuild data). Write "None" if nothing. -->

## How to test

1. 
2. 

## Definition of done

- [ ] Linked issue's acceptance criteria are met
- [ ] Project opens and compiles with no errors or new warnings in the Console
- [ ] Play-tested in the Editor: the change works and nothing nearby broke
- [ ] Code is in `Assets/_Project/Scripts/<Area>/` with namespace `Game.<Area>`
- [ ] Follows `AGENTS.md` (GameEvents instead of direct references, no `GameObject.Find`, tunables in `[SerializeField]` or ScriptableObjects, one MonoBehaviour per file)
- [ ] Shared contracts in `Scripts/Core` unchanged, or the change is called out above
- [ ] Every new or moved asset has its `.meta` file committed, and no stray files (`Library/`, `.csproj`, `.sln`, JDK)
- [ ] Large binaries (audio, `.psd`, `.aseprite`, video) go through Git LFS
- [ ] No new packages, or the team agreed to them
- [ ] Reviewed by at least one teammate, who also played it
