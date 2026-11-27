# Unprompted — Asset Guide

## Overview

Third-party assets are organized by level and asset pack. Shared assets, such as UI elements and fonts, are stored separately.

## Asset Locations

| Folder under `Assets/ThirdParty/` | Contents |
|---|---|
| `Cyberjunk/NeoZero/` | Characters and environments |
| `Cyberjunk/SteamRobots/` | Mechanical enemies |
| `Cyberjunk/EnemyDrone/` | Electrical and explosive drones |
| `Cyberjunk/Music/` | Cyberjunk soundtrack |
| `EldenNecklace/DungeonTileset2/` | Fantasy environments, characters and enemies |
| `EldenNecklace/Music/` | Fantasy soundtrack |
| `Shared/UI/` | Kenney Fantasy UI Borders |
| `Shared/Fonts/` | Aoboshi One and JetBrains Mono |

Custom game artwork belongs under `Assets/_Project/Art/`.

## Sprite Import Settings

The basic import settings have been configured in batches.

| Setting | World Sprites | UI Sprites |
|---|---|---|
| Texture Type | Sprite (2D and UI) | Sprite (2D and UI) |
| Pixels Per Unit | 16 | 100 |
| Mesh Type | Full Rect | Full Rect |
| Filter Mode | Point (no filter) | Point (no filter) |
| Compression | None | None |

## Sprite Slicing

Sprite sheets should be sliced as needed by the developer using them.

1. Select the texture in the Project panel.
2. Set Sprite Mode to Multiple.
3. Open the Sprite Editor.
4. Select Slice.
5. Choose Grid by Cell Size.
6. Enter the appropriate frame dimensions.
7. Verify the boundaries and click Apply.

### Recommended Cell Sizes

- Neo Zero character sheet: 32×32.
- Modular environment tiles: typically 16×16.
- Buildings, props and enemies: inspect the original sheet before slicing.
- Individual animation frames: use Sprite Mode Single.

Do not apply the same slicing dimensions to every asset. Some sprites occupy multiple tiles.

## Audio

- Cyberjunk: Cybershaman — recommended load type: Streaming.
- Elden Necklace: Whispers in the Fog — recommended load type: Decompress On Load.

Both tracks are recognized as AudioClips in Unity. Enable Loop on the AudioSource when appropriate and test the transitions.

## Fonts

- Aoboshi One: menus, headings and fantasy dialogue.
- JetBrains Mono: technical interfaces, upgrades and AI dialogue.

TextMeshPro font assets can be created as needed during UI development.

## Collaboration Guidelines

- Preserve original asset licenses.
- Keep Unity `.meta` files with their corresponding assets.
- Move existing assets through Unity to preserve references.
- Avoid duplicating textures unnecessarily.
- Follow the repository's Git LFS configuration for audio and large source files.
- Do not add new Unity packages without team approval.
- Check `CREDITS.md` for redistribution permissions before sharing third-party assets.