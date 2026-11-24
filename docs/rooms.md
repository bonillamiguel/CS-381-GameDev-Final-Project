# Rooms: how to author one

A room is a prefab. The floor generator (#16) places rooms and links their doors; the wave
spawner (#14) fills Combat and Boss rooms with enemies. Scripts live in
`Assets/_Project/Scripts/Dungeon/` (namespace `Game.Dungeon`).

## Hierarchy

```
Room_<Level>_<Name>           Room                     <- position = centre of the room
  Grid                        Grid (cell size 1)
    Floor                     Tilemap, TilemapRenderer (Order in Layer 0)
    Walls                     Tilemap, TilemapRenderer (Order 1), TilemapCollider2D
  EntryTrigger                BoxCollider2D (Is Trigger), RoomEntryTrigger
  Doors
    Door_N                    Door (Direction North)
      Blocker                 BoxCollider2D (NOT a trigger), fills the doorway gap
      Locked                  sprite shown while locked
      Unlocked                sprite shown while open (optional)
      EntryPoint              empty, 1-2 tiles inside the room
    Door_S / Door_E / Door_W  same
  SpawnPoints
    Spawn_1 ... Spawn_n       empty Transforms
```

Save prefabs in `Assets/_Project/Prefabs/Dungeon/`.

## Rules

1. **The root is the centre.** Set `Room > Size` to the room's outer size in world units
   (walls included). The yellow gizmo shows it. The generator places rooms by this box.
2. **Every doorway is a gap in the Walls tilemap**, with a Door object sitting in the gap.
   The door's Blocker fills the gap exactly. Doors start unlocked.
3. **The entry trigger stays at least one tile away from every doorway.** If the player is still
   standing in a doorway when the doors lock, the blocker turns on around them and they get stuck.
4. **Spawn points stay away from doors** and inside the floor. Red gizmos show them.
5. **Doors list and Entry Trigger** are filled automatically when you add the Room component.
   If you add doors later, right-click the Room component > *Collect Doors And Trigger From Children*.
   Spawn points are dragged into `Enemy Spawn Points` by hand.
6. A room has at most one door per direction. Leave out the ones it doesn't need.
7. Pick the `Type`: **Start** (where the floor begins), **Combat**, **Boss**, **Reward**
   (fragment, heal, shop). Only Combat and Boss rooms lock.

## What happens at runtime

| When | What the Room does |
| --- | --- |
| Player enters any room | Raises `GameEvents.RoomEntered(roomRoot)`. Every real entry, not only the first. Stepping out of the trigger and back in without visiting another room does not count. |
| First entry into an uncleared Combat/Boss room | Locks every door **before** raising `RoomEntered`. |
| `GameEvents.RoomCleared(roomRoot)` for this room | Unlocks every door. The room is cleared and never locks again. |
| Start / Reward rooms | Count as cleared from the start and never lock. |

## For the wave spawner (#14)

- Listen to `GameEvents.RoomEntered`, then `room.GetComponent<Room>()`.
- **Only start waves when `room.Type` is Combat or Boss and `!room.IsCleared`.** `RoomEntered` also
  fires for Start/Reward rooms and for rooms the player comes back to.
- Use `room.EnemySpawnPoints` for positions.
- When the last wave is dead, call `GameEvents.RaiseRoomCleared(room.gameObject)`. That is what opens
  the doors. In a Boss room, whatever handles the boss's death must raise it too.

## For the floor generator (#16)

- `room.WorldBounds` / `room.Size` for placement; `room.Doors` and `room.GetDoor(direction)`.
- Link neighbours with `doorA.Connect(doorB)`. It links both sides. Find the matching door with
  `other.GetDoor(Door.Opposite(doorA.Direction))`.
- `door.ConnectedDoor`, `door.ConnectedRoom`, `door.IsConnected`. `door.EntryPoint` is where to put a
  player who arrives through that door.
- Unconnected doors are left open for now. The generator decides what to do with them
  (e.g. swap in a wall, or `Lock()` them for good).

## Testing without enemies

Put an object with `RoomDebugClear` in the test scene. In the Editor, **F9** raises `RoomCleared`
for the room the player is in, and the Console logs every `RoomEntered` / `RoomCleared`.
It removes itself in builds.
