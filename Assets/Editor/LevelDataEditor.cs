#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData levelData;
    private Vector2 scrollPosition;
    private int selectedPassengerIndex = -1;
    private bool showPassengers = true;
    private bool showWalls = true;
    private bool showTunnels = true;
    private bool showBuses = true;

    private bool gridEditMode = false;
    private ObjectType selectedType = ObjectType.Passenger;
    private bool removeMode = false;
    private PassengerColor selectedColor = PassengerColor.Red;
    private List<string> availableTraits = new List<string>();
    private bool[] selectedTraits = new bool[0];
    private SpawnDirection selectedSpawnDirection = SpawnDirection.Up;

    private enum ObjectType { Passenger, Wall, Tunnel }
    private enum SpawnDirection { Up, Down, Left, Right }

    private void OnEnable()
    {
        levelData = (LevelData)target;
        availableTraits = new List<string> { "RopedTrait" };
        selectedTraits = new bool[availableTraits.Count];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Level Data Editor", EditorStyles.boldLabel);

        DrawGridEditingSection();
        EditorGUILayout.Space(10);
        DrawPassengersSection();
        EditorGUILayout.Space(10);
        DrawWallsSection();
        EditorGUILayout.Space(10);
        DrawTunnelsSection();
        EditorGUILayout.Space(10);
        DrawBusesSection();
        EditorGUILayout.Space(10);
        DrawUtilityButtons();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelData);
            serializedObject.ApplyModifiedProperties();
        }
    }

    private void DrawGridEditingSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Grid Editing", EditorStyles.boldLabel);

        gridEditMode = EditorGUILayout.Toggle("Grid Edit Mode", gridEditMode);

        if (gridEditMode)
        {
            EditorGUILayout.HelpBox("Left click in Scene view to place (if empty) or remove (in remove mode) objects", MessageType.Info);

            selectedType = (ObjectType)EditorGUILayout.EnumPopup("Object Type", selectedType);
            removeMode = EditorGUILayout.Toggle("Remove Mode", removeMode);

            if (!removeMode)
            {
                if (selectedType != ObjectType.Wall)
                {
                    selectedColor = (PassengerColor)EditorGUILayout.EnumPopup("Color", selectedColor);

                    EditorGUILayout.LabelField("Traits:", EditorStyles.boldLabel);
                    for (int i = 0; i < availableTraits.Count; i++)
                    {
                        if (i < selectedTraits.Length)
                            selectedTraits[i] = EditorGUILayout.Toggle(availableTraits[i], selectedTraits[i]);
                    }

                    if (selectedType == ObjectType.Tunnel)
                    {
                        selectedSpawnDirection = (SpawnDirection)EditorGUILayout.EnumPopup("Spawn Direction", selectedSpawnDirection);
                    }
                }
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPassengersSection()
    {
        EditorGUILayout.BeginVertical("box");

        showPassengers = EditorGUILayout.Foldout(showPassengers, $"Passengers ({levelData.passengers.Count})", true);

        if (showPassengers)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));

            for (int i = 0; i < levelData.passengers.Count; i++)
                DrawPassengerEntry(i);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Passenger"))
            {
                levelData.passengers.Add(new PassengerData
                {
                    gridPosition = Vector2Int.zero,
                    color = PassengerColor.Red,
                    traitTypes = new List<string>()
                });
            }

            if (GUILayout.Button("Clear All Passengers"))
            {
                if (EditorUtility.DisplayDialog("Clear Passengers", "Are you sure you want to remove all passengers?", "Yes", "Cancel"))
                {
                    levelData.passengers.Clear();
                    RefreshGameManager();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawPassengerEntry(int index)
    {
        PassengerData passenger = levelData.passengers[index];

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = GetColorFromEnum(passenger.color);
        EditorGUILayout.LabelField("", EditorStyles.helpBox, GUILayout.Width(20), GUILayout.Height(20));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField($"Passenger {index}", EditorStyles.boldLabel);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            levelData.passengers.RemoveAt(index);
            RefreshGameManager();
            return;
        }

        EditorGUILayout.EndHorizontal();

        passenger.gridPosition = EditorGUILayout.Vector2IntField("Grid Position", passenger.gridPosition);
        passenger.color = (PassengerColor)EditorGUILayout.EnumPopup("Color", passenger.color);

        EditorGUILayout.LabelField("Traits:");
        EditorGUI.indentLevel++;
        for (int t = 0; t < passenger.traitTypes.Count; t++)
        {
            EditorGUILayout.BeginHorizontal();
            passenger.traitTypes[t] = EditorGUILayout.TextField(passenger.traitTypes[t]);
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                passenger.traitTypes.RemoveAt(t);
                t--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Trait"))
            passenger.traitTypes.Add("");

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawWallsSection()
    {
        EditorGUILayout.BeginVertical("box");

        showWalls = EditorGUILayout.Foldout(showWalls, $"Walls ({levelData.walls.Count})", true);

        if (showWalls)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));

            for (int i = 0; i < levelData.walls.Count; i++)
                DrawWallEntry(i);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Wall"))
            {
                levelData.walls.Add(new WallData
                {
                    gridPosition = Vector2Int.zero
                });
            }

            if (GUILayout.Button("Clear All Walls"))
            {
                if (EditorUtility.DisplayDialog("Clear Walls", "Are you sure you want to remove all walls?", "Yes", "Cancel"))
                {
                    levelData.walls.Clear();
                    RefreshGameManager();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawWallEntry(int index)
    {
        WallData wall = levelData.walls[index];

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.gray;
        EditorGUILayout.LabelField("", EditorStyles.helpBox, GUILayout.Width(20), GUILayout.Height(20));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField($"Wall {index}", EditorStyles.boldLabel);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            levelData.walls.RemoveAt(index);
            RefreshGameManager();
            return;
        }

        EditorGUILayout.EndHorizontal();

        wall.gridPosition = EditorGUILayout.Vector2IntField("Grid Position", wall.gridPosition);

        EditorGUILayout.EndVertical();
    }

    private void DrawTunnelsSection()
    {
        EditorGUILayout.BeginVertical("box");

        showTunnels = EditorGUILayout.Foldout(showTunnels, $"Tunnels ({levelData.tunnels.Count})", true);

        if (showTunnels)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));

            for (int i = 0; i < levelData.tunnels.Count; i++)
                DrawTunnelEntry(i);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Tunnel"))
            {
                levelData.tunnels.Add(new TunnelData
                {
                    gridPosition = Vector2Int.zero,
                    direction = new Vector2Int(0, 1),
                    spawnTemplate = new PassengerData { color = PassengerColor.Red, traitTypes = new List<string>() }
                });
            }

            if (GUILayout.Button("Clear All Tunnels"))
            {
                if (EditorUtility.DisplayDialog("Clear Tunnels", "Are you sure you want to remove all tunnels?", "Yes", "Cancel"))
                {
                    levelData.tunnels.Clear();
                    RefreshGameManager();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawTunnelEntry(int index)
    {
        TunnelData tunnel = levelData.tunnels[index];

        if (tunnel.spawnTemplate == null)
            tunnel.spawnTemplate = new PassengerData { color = PassengerColor.Red, traitTypes = new List<string>() };

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = Color.blue;
        EditorGUILayout.LabelField("", EditorStyles.helpBox, GUILayout.Width(20), GUILayout.Height(20));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField($"Tunnel {index}", EditorStyles.boldLabel);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            levelData.tunnels.RemoveAt(index);
            RefreshGameManager();
            return;
        }

        EditorGUILayout.EndHorizontal();

        tunnel.gridPosition = EditorGUILayout.Vector2IntField("Grid Position", tunnel.gridPosition);
        tunnel.direction = EditorGUILayout.Vector2IntField("Direction", tunnel.direction);
        tunnel.spawnTemplate.color = (PassengerColor)EditorGUILayout.EnumPopup("Spawn Color", tunnel.spawnTemplate.color);

        EditorGUILayout.LabelField("Spawn Traits:");
        EditorGUI.indentLevel++;
        for (int t = 0; t < tunnel.spawnTemplate.traitTypes.Count; t++)
        {
            EditorGUILayout.BeginHorizontal();
            tunnel.spawnTemplate.traitTypes[t] = EditorGUILayout.TextField(tunnel.spawnTemplate.traitTypes[t]);
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                tunnel.spawnTemplate.traitTypes.RemoveAt(t);
                t--;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Trait"))
            tunnel.spawnTemplate.traitTypes.Add("");

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawBusesSection()
    {
        EditorGUILayout.BeginVertical("box");
        showBuses = EditorGUILayout.Foldout(showBuses, $"Buses ({levelData.buses.Count})", true);

        if (showBuses)
        {
            SerializedProperty busesProperty = serializedObject.FindProperty("buses");
            EditorGUILayout.PropertyField(busesProperty, true);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawUtilityButtons()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Level Data"))
        {
            string path = EditorUtility.SaveFilePanel("Save Level Data", "Assets/", levelData.name, "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);
                AssetDatabase.CreateAsset(Instantiate(levelData), path);
                AssetDatabase.SaveAssets();
            }
        }

        if (GUILayout.Button("Refresh Scene"))
            RefreshGameManager();

        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Generate Grid Layout"))
            GridLayoutWindow.ShowWindow(levelData);

        EditorGUILayout.EndVertical();
    }

    private void RefreshGameManager()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
            gameManager.RespawnPassengers();
    }

    private Color GetColorFromEnum(PassengerColor passengerColor)
    {
        return passengerColor switch
        {
            PassengerColor.Red => Color.red,
            PassengerColor.Blue => Color.blue,
            PassengerColor.Green => Color.green,
            PassengerColor.Yellow => Color.yellow,
            PassengerColor.Purple => new Color(0.5f, 0f, 1f),
            _ => Color.white
        };
    }

    private void OnSceneGUI()
    {
        if (!gridEditMode)
            return;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridCell gridCell = hit.collider.GetComponent<GridCell>();
                if (gridCell != null)
                {
                    HandleClickAtPosition(gridCell.gridPosition);
                    e.Use();
                }
            }
        }

        Handles.color = Color.white;
        foreach (var passenger in levelData.passengers)
        {
            Vector3 worldPos = new Vector3(passenger.gridPosition.x, 0.1f, passenger.gridPosition.y);
            Handles.DrawWireCube(worldPos, Vector3.one * 0.8f);

            Handles.color = GetColorFromEnum(passenger.color);
            Handles.DrawSolidDisc(worldPos + Vector3.up * 0.1f, Vector3.up, 0.3f);
        }
        foreach (var wall in levelData.walls)
        {
            Vector3 worldPos = new Vector3(wall.gridPosition.x, 0.1f, wall.gridPosition.y);
            Handles.DrawWireCube(worldPos, Vector3.one * 0.8f);

            Handles.color = Color.gray;
            Handles.DrawSolidDisc(worldPos + Vector3.up * 0.1f, Vector3.up, 0.3f);
        }
        foreach (var tunnel in levelData.tunnels)
        {
            Vector3 worldPos = new Vector3(tunnel.gridPosition.x, 0.1f, tunnel.gridPosition.y);
            Handles.DrawWireCube(worldPos, Vector3.one * 0.8f);

            Handles.color = Color.blue;
            Handles.DrawSolidDisc(worldPos + Vector3.up * 0.1f, Vector3.up, 0.3f);

            Vector3 dirVec = new Vector3(tunnel.direction.x, 0, tunnel.direction.y) * 0.5f;
            Handles.DrawLine(worldPos + Vector3.up * 0.1f, worldPos + Vector3.up * 0.1f + dirVec);
        }
        Handles.color = Color.white;
    }

    private void HandleClickAtPosition(Vector2Int gridPos)
    {
        if (removeMode)
        {
            RemoveAtPosition(gridPos);
        }
        else if (!HasAnyAtPosition(gridPos))
        {
            PlaceSelectedAtPosition(gridPos);
        }
        RefreshGameManager();
        EditorUtility.SetDirty(levelData);
    }

    private bool HasAnyAtPosition(Vector2Int gridPos)
    {
        return levelData.passengers.Any(p => p.gridPosition == gridPos) ||
               levelData.walls.Any(w => w.gridPosition == gridPos) ||
               levelData.tunnels.Any(t => t.gridPosition == gridPos);
    }

    private void RemoveAtPosition(Vector2Int gridPos)
    {
        PassengerData passenger = levelData.passengers.FirstOrDefault(p => p.gridPosition == gridPos);
        if (passenger != null)
        {
            levelData.passengers.Remove(passenger);
            return;
        }

        WallData wall = levelData.walls.FirstOrDefault(w => w.gridPosition == gridPos);
        if (wall != null)
        {
            levelData.walls.Remove(wall);
            return;
        }

        TunnelData tunnel = levelData.tunnels.FirstOrDefault(t => t.gridPosition == gridPos);
        if (tunnel != null)
        {
            levelData.tunnels.Remove(tunnel);
            return;
        }
    }

    private void PlaceSelectedAtPosition(Vector2Int gridPos)
    {
        List<string> traitsToAdd = new List<string>();
        for (int i = 0; i < selectedTraits.Length && i < availableTraits.Count; i++)
        {
            if (selectedTraits[i])
                traitsToAdd.Add(availableTraits[i]);
        }

        switch (selectedType)
        {
            case ObjectType.Passenger:
                levelData.passengers.Add(new PassengerData
                {
                    gridPosition = gridPos,
                    color = selectedColor,
                    traitTypes = traitsToAdd
                });
                break;
            case ObjectType.Wall:
                levelData.walls.Add(new WallData
                {
                    gridPosition = gridPos
                });
                break;
            case ObjectType.Tunnel:
                levelData.tunnels.Add(new TunnelData
                {
                    gridPosition = gridPos,
                    direction = GetDirectionVector(selectedSpawnDirection),
                    spawnTemplate = new PassengerData
                    {
                        color = selectedColor,
                        traitTypes = traitsToAdd
                    }
                });
                break;
        }
    }

    private Vector2Int GetDirectionVector(SpawnDirection d)
    {
        return d switch
        {
            SpawnDirection.Up => new Vector2Int(0, 1),
            SpawnDirection.Down => new Vector2Int(0, -1),
            SpawnDirection.Left => new Vector2Int(-1, 0),
            SpawnDirection.Right => new Vector2Int(1, 0),
            _ => new Vector2Int(0, 1)
        };
    }
}

public class GridLayoutWindow : EditorWindow
{
    private LevelData levelData;
    private Vector2 scrollPos;
    private int gridWidth = 10;
    private int gridHeight = 10;
    private ObjectType brushType = ObjectType.Passenger;
    private PassengerColor brushColor = PassengerColor.Red;
    private SpawnDirection brushDirection = SpawnDirection.Up;

    private enum ObjectType { Passenger, Wall, Tunnel }
    private enum SpawnDirection { Up, Down, Left, Right }

    public static void ShowWindow(LevelData data)
    {
        GridLayoutWindow window = GetWindow<GridLayoutWindow>("Grid Layout Editor");
        window.levelData = data;
        window.Show();
    }

    private void OnGUI()
    {
        if (levelData == null)
        {
            EditorGUILayout.HelpBox("No level data selected", MessageType.Warning);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        gridWidth = EditorGUILayout.IntField("Grid Width", gridWidth);
        gridHeight = EditorGUILayout.IntField("Grid Height", gridHeight);
        EditorGUILayout.EndHorizontal();

        brushType = (ObjectType)EditorGUILayout.EnumPopup("Brush Type", brushType);

        if (brushType != ObjectType.Wall)
            brushColor = (PassengerColor)EditorGUILayout.EnumPopup("Brush Color", brushColor);
        if (brushType == ObjectType.Tunnel)
            brushDirection = (SpawnDirection)EditorGUILayout.EnumPopup("Brush Direction", brushDirection);

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int y = gridHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                var obj = GetObjectAtPosition(gridPos);
                Color buttonColor = obj.HasValue ? obj.Value.Item2 : Color.white;
                GUI.backgroundColor = buttonColor;

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    if (obj.HasValue)
                        RemoveObjectAtPosition(gridPos, obj.Value.Item1);
                    else
                        PlaceBrushAtPosition(gridPos);
                    EditorUtility.SetDirty(levelData);
                }

                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private (string, Color)? GetObjectAtPosition(Vector2Int gridPos)
    {
        PassengerData passenger = levelData.passengers.FirstOrDefault(p => p.gridPosition == gridPos);
        if (passenger != null)
            return ("Passenger", GetColorFromEnum(passenger.color));

        WallData wall = levelData.walls.FirstOrDefault(w => w.gridPosition == gridPos);
        if (wall != null)
            return ("Wall", Color.gray);

        TunnelData tunnel = levelData.tunnels.FirstOrDefault(t => t.gridPosition == gridPos);
        if (tunnel != null)
            return ("Tunnel", Color.blue);

        return null;
    }

    private void RemoveObjectAtPosition(Vector2Int gridPos, string objType)
    {
        if (objType == "Passenger")
        {
            PassengerData passenger = levelData.passengers.FirstOrDefault(p => p.gridPosition == gridPos);
            if (passenger != null)
                levelData.passengers.Remove(passenger);
        }
        else if (objType == "Wall")
        {
            WallData wall = levelData.walls.FirstOrDefault(w => w.gridPosition == gridPos);
            if (wall != null)
                levelData.walls.Remove(wall);
        }
        else if (objType == "Tunnel")
        {
            TunnelData tunnel = levelData.tunnels.FirstOrDefault(t => t.gridPosition == gridPos);
            if (tunnel != null)
                levelData.tunnels.Remove(tunnel);
        }
    }

    private void PlaceBrushAtPosition(Vector2Int gridPos)
    {
        switch (brushType)
        {
            case ObjectType.Passenger:
                levelData.passengers.Add(new PassengerData
                {
                    gridPosition = gridPos,
                    color = brushColor,
                    traitTypes = new List<string>()
                });
                break;
            case ObjectType.Wall:
                levelData.walls.Add(new WallData
                {
                    gridPosition = gridPos
                });
                break;
            case ObjectType.Tunnel:
                levelData.tunnels.Add(new TunnelData
                {
                    gridPosition = gridPos,
                    direction = GetDirectionVector(brushDirection),
                    spawnTemplate = new PassengerData
                    {
                        color = brushColor,
                        traitTypes = new List<string>()
                    }
                });
                break;
        }
    }

    private Vector2Int GetDirectionVector(SpawnDirection d)
    {
        return d switch
        {
            SpawnDirection.Up => new Vector2Int(0, 1),
            SpawnDirection.Down => new Vector2Int(0, -1),
            SpawnDirection.Left => new Vector2Int(-1, 0),
            SpawnDirection.Right => new Vector2Int(1, 0),
            _ => new Vector2Int(0, 1)
        };
    }

    private Color GetColorFromEnum(PassengerColor passengerColor)
    {
        return passengerColor switch
        {
            PassengerColor.Red => Color.red,
            PassengerColor.Blue => Color.blue,
            PassengerColor.Green => Color.green,
            PassengerColor.Yellow => Color.yellow,
            PassengerColor.Purple => new Color(0.5f, 0f, 1f),
            _ => Color.white
        };
    }
}
#endif