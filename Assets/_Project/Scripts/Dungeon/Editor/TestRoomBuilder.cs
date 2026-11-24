using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Game.Dungeon.EditorTools
{
    /// <summary>
    /// Tools > Dungeon > Build Test Room. Builds the 16x10 test room from docs/rooms.md in the open scene,
    /// plus a test player, camera size and the F9 debug object, then saves Room_Test as a prefab.
    ///
    /// Safe to run again. Room_Test and its children are found by name and reconfigured (tilemaps are
    /// cleared and repainted). The Player, Main Camera and RoomDebug objects are skipped if they already
    /// exist. One Ctrl+Z undoes everything in the scene; the prefab file on disk stays.
    /// </summary>
    public static class TestRoomBuilder
    {
        private const string MenuPath = "Tools/Dungeon/Build Test Room";
        private const string UndoName = "Build Test Room";
        private const string LogPrefix = "[TestRoomBuilder]";

        private const string TilesFolder = "Assets/_Project/Art/Tiles";
        private const string FloorTilePath = TilesFolder + "/Floor.asset";
        private const string WallTilePath = TilesFolder + "/Wall.asset";
        private const string SquareSpritePath = TilesFolder + "/Square.png";
        private const string PrefabFolder = "Assets/_Project/Prefabs/Dungeon";
        private const string PrefabPath = PrefabFolder + "/Room_Test.prefab";

        private const string RoomName = "Room_Test";
        private const string PlayerName = "Player";
        private const string DebugName = "RoomDebug";
        private const string PlayerTag = "Player";
        private const string MainCameraTag = "MainCamera";

        // Layout, in tiles (1 tile = 1 world unit). The room's centre is (0,0).
        private static readonly Vector2 RoomSize = new Vector2(16f, 10f);
        private const int MinX = -8, MaxX = 7, MinY = -5, MaxY = 4; // inclusive cell range
        private const int GapLow = -1, GapHigh = 0;                 // 2-tile doorway in the middle of each side
        private static readonly Vector2 EntryTriggerSize = new Vector2(12f, 6f);
        private static readonly Vector3[] SpawnPositions =
        {
            new Vector3(-4f, 2f, 0f),
            new Vector3(4f, 2f, 0f),
            new Vector3(0f, -1f, 0f),
        };

        private static readonly DoorSpec[] DoorSpecs =
        {
            new DoorSpec("Door_N", DoorDirection.North, new Vector3(0f, 4.5f, 0f), new Vector2(2f, 1f), new Vector3(0f, -2f, 0f)),
            new DoorSpec("Door_S", DoorDirection.South, new Vector3(0f, -4.5f, 0f), new Vector2(2f, 1f), new Vector3(0f, 2f, 0f)),
            new DoorSpec("Door_E", DoorDirection.East, new Vector3(7.5f, 0f, 0f), new Vector2(1f, 2f), new Vector3(-2f, 0f, 0f)),
            new DoorSpec("Door_W", DoorDirection.West, new Vector3(-7.5f, 0f, 0f), new Vector2(1f, 2f), new Vector3(2f, 0f, 0f)),
        };

        // Test player and camera
        private static readonly Vector3 PlayerPosition = new Vector3(0f, 8f, 0f);
        private const float PlayerScale = 0.8f;
        private const float PlayerGravity = 1f;
        private const float CameraSize = 9f;

        // Look. Tilemaps stay untinted (white) so the tile assets' own colours show.
        private const int FloorOrder = 0, WallOrder = 1, LockedOrder = 2, PlayerOrder = 3;
        private static readonly Color TilemapColor = Color.white;
        private static readonly Color LockedColor = Color.red;
        private static readonly Color PlayerColor = Color.cyan;

        private readonly struct DoorSpec
        {
            public readonly string Name;
            public readonly DoorDirection Direction;
            public readonly Vector3 Position;   // local to Doors
            public readonly Vector2 Size;       // blocker and red sprite
            public readonly Vector3 EntryOffset; // local to the door, pointing into the room

            public DoorSpec(string name, DoorDirection direction, Vector3 position, Vector2 size, Vector3 entryOffset)
            {
                Name = name;
                Direction = direction;
                Position = position;
                Size = size;
                EntryOffset = entryOffset;
            }
        }

        [MenuItem(MenuPath)]
        private static void Build()
        {
            if (!LoadAssets(out TileBase floorTile, out TileBase wallTile, out Sprite square))
                return;

            Scene scene = SceneManager.GetActiveScene();
            var log = new List<string>();

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName(UndoName);
            int undoGroup = Undo.GetCurrentGroup();

            GameObject room;
            try
            {
                room = GetOrCreateRoomRoot(scene, log);
                BuildTilemaps(scene, room.transform, floorTile, wallTile, log);
                RoomEntryTrigger trigger = BuildEntryTrigger(room.transform, log);
                List<Door> doors = BuildDoors(room.transform, square, log);
                List<Transform> spawnPoints = BuildSpawnPoints(room.transform, log);
                SetUpRoomComponent(room, trigger, doors, spawnPoints, log);

                BuildPlayer(scene, square, log);
                SetUpCamera(scene, log);
                BuildDebugObject(scene, log);
            }
            catch (System.Exception e)
            {
                Undo.RevertAllDownToGroup(undoGroup);
                Debug.LogError($"{LogPrefix} Build failed, scene changes were undone. {e.Message}");
                Debug.LogException(e);
                return;
            }

            SavePrefab(room, log);
            EditorSceneManager.MarkSceneDirty(scene);
            Undo.CollapseUndoOperations(undoGroup);
            Selection.activeGameObject = room;

            Debug.Log($"{LogPrefix} Done in scene '{scene.name}' (save the scene with Ctrl+S):\n- " + string.Join("\n- ", log), room);
        }

        [MenuItem(MenuPath, true)]
        private static bool CanBuild() => !EditorApplication.isPlayingOrWillChangePlaymode;

        // ---------- Assets ----------

        private static bool LoadAssets(out TileBase floorTile, out TileBase wallTile, out Sprite square)
        {
            floorTile = AssetDatabase.LoadAssetAtPath<TileBase>(FloorTilePath);
            wallTile = AssetDatabase.LoadAssetAtPath<TileBase>(WallTilePath);
            // Square.png is imported as a sprite sheet (Multiple), so the sprite is a sub-asset.
            square = AssetDatabase.LoadAllAssetsAtPath(SquareSpritePath).OfType<Sprite>().FirstOrDefault();

            var missing = new List<string>();
            if (floorTile == null) missing.Add(FloorTilePath + " (Tile)");
            if (wallTile == null) missing.Add(WallTilePath + " (Tile)");
            if (square == null) missing.Add(SquareSpritePath + " (Sprite)");
            if (missing.Count == 0)
                return true;

            EditorUtility.DisplayDialog("Build Test Room", "Missing assets:\n" + string.Join("\n", missing), "OK");
            return false;
        }

        // ---------- Room ----------

        private static GameObject GetOrCreateRoomRoot(Scene scene, List<string> log)
        {
            GameObject room = scene.GetRootGameObjects().FirstOrDefault(go => go.name == RoomName);
            if (room == null)
            {
                room = new GameObject(RoomName);
                Undo.RegisterCreatedObjectUndo(room, UndoName);
                log.Add($"Created {RoomName}");
            }
            else
            {
                // Prefab instance children can't be rebuilt freely; unpack, rebuild, then save the prefab again.
                if (PrefabUtility.IsOutermostPrefabInstanceRoot(room))
                {
                    PrefabUtility.UnpackPrefabInstance(room, PrefabUnpackMode.Completely, InteractionMode.UserAction);
                    log.Add($"Unpacked the {RoomName} prefab instance to rebuild it");
                }
                log.Add($"Reused {RoomName}");
            }

            Undo.RecordObject(room.transform, UndoName);
            room.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            room.transform.localScale = Vector3.one;
            return room;
        }

        private static void BuildTilemaps(Scene scene, Transform roomT, TileBase floorTile, TileBase wallTile, List<string> log)
        {
            GameObject gridGo = FindChildWith<Grid>(roomT)?.gameObject;
            if (gridGo == null)
            {
                // A Grid made with GameObject > 2D Object > Tilemap sits at the scene root: move it into the room.
                gridGo = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "Grid" && go.TryGetComponent<Grid>(out _));
                if (gridGo != null)
                {
                    Undo.SetTransformParent(gridGo.transform, roomT, UndoName);
                    log.Add("Moved the scene's Grid under Room_Test");
                }
            }

            bool gridCreated = false;
            if (gridGo == null)
                gridGo = GetOrCreateChild(roomT, "Grid", Vector3.zero, out gridCreated);
            else
                ResetTransform(gridGo.transform, Vector3.zero);
            log.Add(gridCreated ? "Created Grid" : "Reused Grid");

            Grid grid = GetOrAdd<Grid>(gridGo);
            grid.cellLayout = GridLayout.CellLayout.Rectangle;
            grid.cellSize = new Vector3(1f, 1f, 0f);
            grid.cellGap = Vector3.zero;

            Tilemap floor = SetUpTilemap(gridGo.transform, "Floor", FloorOrder, log);
            Tilemap walls = SetUpTilemap(gridGo.transform, "Walls", WallOrder, log);
            GetOrAdd<TilemapCollider2D>(walls.gameObject);

            if (floor.TryGetComponent(out TilemapCollider2D floorCollider))
            {
                Undo.DestroyObjectImmediate(floorCollider);
                log.Add("Removed a TilemapCollider2D from Floor (the floor must not be solid)");
            }

            foreach (Tilemap other in gridGo.GetComponentsInChildren<Tilemap>(true))
                if (other != floor && other != walls)
                    log.Add($"WARNING: Grid also has a tilemap '{other.name}'. It was left alone; delete it if it's left over");

            // Floor: every cell of the room.
            Undo.RegisterCompleteObjectUndo(floor, UndoName);
            floor.ClearAllTiles();
            var floorBounds = new BoundsInt(MinX, MinY, 0, MaxX - MinX + 1, MaxY - MinY + 1, 1);
            var floorTiles = Enumerable.Repeat(floorTile, floorBounds.size.x * floorBounds.size.y).ToArray();
            floor.SetTilesBlock(floorBounds, floorTiles);

            // Walls: the outer ring, minus a doorway gap in the middle of each side.
            Undo.RegisterCompleteObjectUndo(walls, UndoName);
            walls.ClearAllTiles();
            int wallCount = 0;
            for (int x = MinX; x <= MaxX; x++)
            {
                for (int y = MinY; y <= MaxY; y++)
                {
                    if (!IsWallCell(x, y))
                        continue;
                    walls.SetTile(new Vector3Int(x, y, 0), wallTile);
                    wallCount++;
                }
            }
            floor.CompressBounds();
            walls.CompressBounds();

            log.Add($"Painted {floorTiles.Length} floor tiles and {wallCount} wall tiles");
        }

        private static bool IsWallCell(int x, int y)
        {
            bool edgeX = x == MinX || x == MaxX;
            bool edgeY = y == MinY || y == MaxY;
            if (!edgeX && !edgeY)
                return false;
            if (edgeX && edgeY)
                return true; // corner
            if (edgeY && x >= GapLow && x <= GapHigh)
                return false; // north / south doorway
            if (edgeX && y >= GapLow && y <= GapHigh)
                return false; // east / west doorway
            return true;
        }

        private static Tilemap SetUpTilemap(Transform gridT, string name, int sortingOrder, List<string> log)
        {
            GameObject go = GetOrCreateChild(gridT, name, Vector3.zero, out bool created);
            Tilemap tilemap = GetOrAdd<Tilemap>(go);
            tilemap.color = TilemapColor;
            TilemapRenderer renderer = GetOrAdd<TilemapRenderer>(go);
            renderer.sortingOrder = sortingOrder;
            log.Add($"{(created ? "Created" : "Reused")} Grid/{name}");
            return tilemap;
        }

        private static RoomEntryTrigger BuildEntryTrigger(Transform roomT, List<string> log)
        {
            GameObject go = GetOrCreateChild(roomT, "EntryTrigger", Vector3.zero, out bool created);
            BoxCollider2D box = GetOrAdd<BoxCollider2D>(go); // before RoomEntryTrigger, which requires a Collider2D
            box.isTrigger = true;
            box.size = EntryTriggerSize;
            box.offset = Vector2.zero;
            RoomEntryTrigger trigger = GetOrAdd<RoomEntryTrigger>(go);
            log.Add($"{(created ? "Created" : "Reused")} EntryTrigger ({EntryTriggerSize.x}x{EntryTriggerSize.y})");
            return trigger;
        }

        private static List<Door> BuildDoors(Transform roomT, Sprite square, List<string> log)
        {
            GameObject doorsGo = GetOrCreateChild(roomT, "Doors", Vector3.zero, out _);
            var doors = new List<Door>();

            foreach (DoorSpec spec in DoorSpecs)
            {
                GameObject doorGo = GetOrCreateChild(doorsGo.transform, spec.Name, spec.Position, out _);

                GameObject blockerGo = GetOrCreateChild(doorGo.transform, "Blocker", Vector3.zero, out _);
                BoxCollider2D blocker = GetOrAdd<BoxCollider2D>(blockerGo);
                blocker.isTrigger = false;
                blocker.size = spec.Size;
                blocker.offset = Vector2.zero;
                blocker.enabled = true;

                GameObject lockedGo = GetOrCreateChild(doorGo.transform, "Locked", Vector3.zero, out _);
                lockedGo.transform.localScale = new Vector3(spec.Size.x, spec.Size.y, 1f);
                SpriteRenderer lockedSprite = GetOrAdd<SpriteRenderer>(lockedGo);
                lockedSprite.sprite = square;
                lockedSprite.drawMode = SpriteDrawMode.Simple;
                lockedSprite.color = LockedColor;
                lockedSprite.sortingOrder = LockedOrder;

                GameObject entryGo = GetOrCreateChild(doorGo.transform, "EntryPoint", spec.EntryOffset, out _);

                Door door = GetOrAdd<Door>(doorGo);
                var so = new SerializedObject(door);
                Prop(so, "direction").intValue = (int)spec.Direction;
                Prop(so, "blocker").objectReferenceValue = blocker;
                Prop(so, "lockedVisual").objectReferenceValue = lockedGo;
                Prop(so, "entryPoint").objectReferenceValue = entryGo.transform;
                so.ApplyModifiedProperties();

                doors.Add(door);
            }

            log.Add("Built Doors/Door_N, Door_S, Door_E, Door_W (Blocker, red Locked, EntryPoint)");
            return doors;
        }

        private static List<Transform> BuildSpawnPoints(Transform roomT, List<string> log)
        {
            GameObject parent = GetOrCreateChild(roomT, "SpawnPoints", Vector3.zero, out _);
            var points = new List<Transform>();
            for (int i = 0; i < SpawnPositions.Length; i++)
                points.Add(GetOrCreateChild(parent.transform, $"Spawn_{i + 1}", SpawnPositions[i], out _).transform);
            log.Add($"Built SpawnPoints/Spawn_1..{SpawnPositions.Length}");
            return points;
        }

        private static void SetUpRoomComponent(GameObject roomGo, RoomEntryTrigger trigger, List<Door> doors,
                                               List<Transform> spawnPoints, List<string> log)
        {
            Room room = GetOrAdd<Room>(roomGo);
            var so = new SerializedObject(room);
            Prop(so, "type").intValue = (int)RoomType.Combat;
            Prop(so, "size").vector2Value = RoomSize;
            Prop(so, "entryTrigger").objectReferenceValue = trigger;
            SetList(Prop(so, "doors"), doors);
            SetList(Prop(so, "enemySpawnPoints"), spawnPoints);
            so.ApplyModifiedProperties();
            log.Add($"Room component: Combat, size {RoomSize.x}x{RoomSize.y}, {doors.Count} doors, {spawnPoints.Count} spawn points");
        }

        // ---------- Scene helpers (skipped if they already exist) ----------

        private static void BuildPlayer(Scene scene, Sprite square, List<string> log)
        {
            GameObject existing = FindInScene(scene, go => go.CompareTag(PlayerTag));
            if (existing != null)
            {
                log.Add($"Skipped Player: '{existing.name}' is already tagged Player");
                return;
            }

            var player = new GameObject(PlayerName) { tag = PlayerTag };
            player.transform.position = PlayerPosition;
            player.transform.localScale = new Vector3(PlayerScale, PlayerScale, 1f);

            SpriteRenderer sprite = player.AddComponent<SpriteRenderer>();
            sprite.sprite = square;
            sprite.color = PlayerColor;
            sprite.sortingOrder = PlayerOrder;

            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = PlayerGravity;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.constraints = RigidbodyConstraints2D.FreezeRotation; // so it doesn't tip over on a door edge

            BoxCollider2D box = player.AddComponent<BoxCollider2D>();
            box.size = Vector2.one; // the sprite is 1 unit; the 0.8 scale makes it 0.8

            Undo.RegisterCreatedObjectUndo(player, UndoName);
            log.Add($"Created Player at {PlayerPosition} (cyan, tag Player, gravity {PlayerGravity}, continuous)");
        }

        private static void SetUpCamera(Scene scene, List<string> log)
        {
            GameObject camGo = FindInScene(scene, go => go.CompareTag(MainCameraTag) && go.TryGetComponent<Camera>(out _));
            if (camGo == null)
            {
                log.Add("WARNING: no camera tagged MainCamera in the scene, so the camera size was not set");
                return;
            }
            Camera cam = camGo.GetComponent<Camera>();
            Undo.RecordObject(cam, UndoName);
            cam.orthographic = true;
            cam.orthographicSize = CameraSize;
            log.Add($"Main Camera orthographic size = {CameraSize}");
        }

        private static void BuildDebugObject(Scene scene, List<string> log)
        {
            GameObject existing = FindInScene(scene, go => go.TryGetComponent<RoomDebugClear>(out _));
            if (existing != null)
            {
                log.Add($"Skipped RoomDebug: '{existing.name}' already has RoomDebugClear");
                return;
            }
            var go = new GameObject(DebugName);
            go.AddComponent<RoomDebugClear>();
            Undo.RegisterCreatedObjectUndo(go, UndoName);
            log.Add("Created RoomDebug (F9 clears the current room)");
        }

        private static void SavePrefab(GameObject room, List<string> log)
        {
            EnsureFolder(PrefabFolder);
            bool existed = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
            PrefabUtility.SaveAsPrefabAssetAndConnect(room, PrefabPath, InteractionMode.UserAction, out bool success);
            if (success)
                log.Add($"{(existed ? "Overwrote" : "Saved")} prefab {PrefabPath}");
            else
                Debug.LogError($"{LogPrefix} Could not save {PrefabPath}. The scene objects are still built.");
        }

        // ---------- Generic helpers ----------

        private static GameObject GetOrCreateChild(Transform parent, string name, Vector3 localPosition, out bool created)
        {
            Transform child = null;
            foreach (Transform t in parent)
            {
                if (t.name == name)
                {
                    child = t;
                    break;
                }
            }

            created = child == null;
            if (created)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                Undo.RegisterCreatedObjectUndo(go, UndoName);
                child = go.transform;
            }

            ResetTransform(child, localPosition);
            return child.gameObject;
        }

        private static void ResetTransform(Transform t, Vector3 localPosition)
        {
            Undo.RecordObject(t, UndoName);
            t.SetLocalPositionAndRotation(localPosition, Quaternion.identity);
            t.localScale = Vector3.one;
        }

        private static T GetOrAdd<T>(GameObject go) where T : Component
        {
            if (go.TryGetComponent(out T existing))
            {
                Undo.RecordObject(existing, UndoName);
                return existing;
            }
            return Undo.AddComponent<T>(go);
        }

        private static Transform FindChildWith<T>(Transform parent) where T : Component
        {
            foreach (Transform t in parent)
                if (t.TryGetComponent<T>(out _))
                    return t;
            return null;
        }

        private static GameObject FindInScene(Scene scene, System.Predicate<GameObject> match)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (match(t.gameObject))
                        return t.gameObject;
            return null;
        }

        private static SerializedProperty Prop(SerializedObject so, string field)
        {
            SerializedProperty p = so.FindProperty(field);
            if (p == null)
                throw new System.InvalidOperationException(
                    $"{so.targetObject.GetType().Name} has no serialized field '{field}'. Update TestRoomBuilder to match.");
            return p;
        }

        private static void SetList<T>(SerializedProperty list, IReadOnlyList<T> items) where T : Object
        {
            list.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(path));
        }
    }
}
