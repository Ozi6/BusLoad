#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    private int gridEditBombCountdown = 5;
    private bool gridEditCloakInitial = true;
    private int gridEditRopeLength = 3;

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
        availableTraits = new List<string> { "RopedTrait", "BombedTrait", "CloakedTrait" };
        selectedTraits = new bool[availableTraits.Count];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Level Data Editor", EditorStyles.boldLabel);

        DrawGridSettingsSection();
        EditorGUILayout.Space(10);
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

    private void DrawGridSettingsSection()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

        levelData.gridWidth = EditorGUILayout.IntField("Grid Width", levelData.gridWidth);
        levelData.gridHeight = EditorGUILayout.IntField("Grid Height", levelData.gridHeight);
        levelData.gridSpacing = EditorGUILayout.FloatField("Grid Spacing", levelData.gridSpacing);

        EditorGUILayout.EndVertical();
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
                        if (i < selectedTraits.Length)
                            selectedTraits[i] = EditorGUILayout.Toggle(availableTraits[i], selectedTraits[i]);

                    EditorGUILayout.LabelField("Trait Configuration:", EditorStyles.boldLabel);
                    DrawGridEditTraitConfigs();

                    if (selectedType == ObjectType.Tunnel)
                        selectedSpawnDirection = (SpawnDirection)EditorGUILayout.EnumPopup("Spawn Direction", selectedSpawnDirection);
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
                var configToRemove = passenger.traitConfigs.FirstOrDefault(c => c.traitType == passenger.traitTypes[t]);
                if (configToRemove != null)
                    passenger.traitConfigs.Remove(configToRemove);
                t--;
            }
            EditorGUILayout.EndHorizontal();
            DrawTraitConfiguration(passenger, passenger.traitTypes[t]);
        }

        if (GUILayout.Button("Add Trait"))
            passenger.traitTypes.Add("");

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawTraitConfiguration(PassengerData passenger, string traitType)
    {
        if (string.IsNullOrEmpty(traitType))
            return;

        var config = passenger.traitConfigs.FirstOrDefault(c => c.traitType == traitType);
        if (config == null)
        {
            config = new TraitConfiguration { traitType = traitType };
            passenger.traitConfigs.Add(config);
        }

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");

        switch (traitType)
        {
            case "BombedTrait":
                EditorGUILayout.LabelField("Bomb Configuration:", EditorStyles.miniLabel);
                config.intValue = EditorGUILayout.IntField("Initial Countdown", config.intValue == 0 ? 5 : config.intValue);
                break;

            case "CloakedTrait":
                EditorGUILayout.LabelField("Cloak Configuration:", EditorStyles.miniLabel);
                config.boolValue = EditorGUILayout.Toggle("Initially Cloaked", config.boolValue);
                break;

            case "ReservedTrait":
                EditorGUILayout.LabelField("Reserved Configuration:", EditorStyles.miniLabel);
                break;

            case "RopedTrait":
                EditorGUILayout.LabelField("Rope Configuration:", EditorStyles.miniLabel);
                config.intValue = EditorGUILayout.IntField("Rope Length", config.intValue == 0 ? 3 : config.intValue);
                break;

            default:
                EditorGUILayout.LabelField("Generic Configuration:", EditorStyles.miniLabel);
                config.intValue = EditorGUILayout.IntField("Int Value", config.intValue);
                config.boolValue = EditorGUILayout.Toggle("Bool Value", config.boolValue);
                config.floatValue = EditorGUILayout.FloatField("Float Value", config.floatValue);
                break;
        }

        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
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
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(300));

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
                    passengerQueue = new List<PassengerData>()
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

        if (tunnel.passengerQueue == null)
            tunnel.passengerQueue = new List<PassengerData>();

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

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Passenger Queue ({tunnel.passengerQueue.Count} passengers):", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        for (int p = 0; p < tunnel.passengerQueue.Count; p++)
        {
            PassengerData pass = tunnel.passengerQueue[p];
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = GetColorFromEnum(pass.color);
            EditorGUILayout.LabelField("", EditorStyles.helpBox, GUILayout.Width(15), GUILayout.Height(15));
            GUI.backgroundColor = Color.white;

            EditorGUILayout.LabelField($"Passenger {p + 1}", EditorStyles.miniLabel);

            if (GUILayout.Button("▲", GUILayout.Width(20)) && p > 0)
            {
                PassengerData temp = tunnel.passengerQueue[p];
                tunnel.passengerQueue[p] = tunnel.passengerQueue[p - 1];
                tunnel.passengerQueue[p - 1] = temp;
                EditorUtility.SetDirty(levelData);
            }

            if (GUILayout.Button("▼", GUILayout.Width(20)) && p < tunnel.passengerQueue.Count - 1)
            {
                PassengerData temp = tunnel.passengerQueue[p];
                tunnel.passengerQueue[p] = tunnel.passengerQueue[p + 1];
                tunnel.passengerQueue[p + 1] = temp;
                EditorUtility.SetDirty(levelData);
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                tunnel.passengerQueue.RemoveAt(p);
                EditorUtility.SetDirty(levelData);
                p--;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                continue;
            }

            EditorGUILayout.EndHorizontal();

            pass.color = (PassengerColor)EditorGUILayout.EnumPopup("Color", pass.color);

            EditorGUILayout.LabelField("Traits:", EditorStyles.miniLabel);
            EditorGUI.indentLevel++;
            for (int t = 0; t < pass.traitTypes.Count; t++)
            {
                EditorGUILayout.BeginHorizontal();
                pass.traitTypes[t] = EditorGUILayout.TextField(pass.traitTypes[t]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    pass.traitTypes.RemoveAt(t);
                    var configToRemove = pass.traitConfigs.FirstOrDefault(c => c.traitType == pass.traitTypes[t]);
                    if (configToRemove != null)
                        pass.traitConfigs.Remove(configToRemove);
                    t--;
                }
                EditorGUILayout.EndHorizontal();
                DrawTraitConfiguration(pass, pass.traitTypes[t]);
            }

            if (GUILayout.Button("Add Trait", GUILayout.Height(16)))
                pass.traitTypes.Add("");

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
        EditorGUI.indentLevel--;

        if (GUILayout.Button("Add Passenger to Queue"))
        {
            tunnel.passengerQueue.Add(new PassengerData
            {
                color = PassengerColor.Red,
                traitTypes = new List<string>()
            });
            EditorUtility.SetDirty(levelData);
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBusesSection()
    {
        EditorGUILayout.BeginVertical("box");
        showBuses = EditorGUILayout.Foldout(showBuses, $"Buses ({levelData.buses.Count})", true);

        if (showBuses)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(200));

            for (int i = 0; i < levelData.buses.Count; i++)
                DrawBusEntry(i);

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Bus"))
            {
                levelData.buses.Add(new BusData
                {
                    color = PassengerColor.Red,
                    traitTypes = new List<string>()
                });
            }

            if (GUILayout.Button("Clear All Buses"))
            {
                if (EditorUtility.DisplayDialog("Clear Buses", "Are you sure you want to remove all buses?", "Yes", "Cancel"))
                {
                    levelData.buses.Clear();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndVertical();
    }

    private void DrawBusEntry(int index)
    {
        BusData bus = levelData.buses[index];

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = GetColorFromEnum(bus.color);
        EditorGUILayout.LabelField("", EditorStyles.helpBox, GUILayout.Width(20), GUILayout.Height(20));
        GUI.backgroundColor = Color.white;

        EditorGUILayout.LabelField($"Bus {index}", EditorStyles.boldLabel);

        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            levelData.buses.RemoveAt(index);
            return;
        }

        EditorGUILayout.EndHorizontal();

        bus.color = (PassengerColor)EditorGUILayout.EnumPopup("Color", bus.color);

        EditorGUILayout.LabelField("Traits:");
        EditorGUI.indentLevel++;
        for (int t = 0; t < bus.traitTypes.Count; t++)
        {
            EditorGUILayout.BeginHorizontal();
            bus.traitTypes[t] = EditorGUILayout.TextField(bus.traitTypes[t]);
            if (GUILayout.Button("-", GUILayout.Width(25)))
            {
                bus.traitTypes.RemoveAt(t);
                var configToRemove = bus.traitConfigs.FirstOrDefault(c => c.traitType == bus.traitTypes[t]);
                if (configToRemove != null)
                    bus.traitConfigs.Remove(configToRemove);
                t--;
            }
            EditorGUILayout.EndHorizontal();
            DrawBusTraitConfiguration(bus, bus.traitTypes[t]);
        }

        if (GUILayout.Button("Add Trait"))
            bus.traitTypes.Add("");

        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawBusTraitConfiguration(BusData bus, string traitType)
    {
        if (string.IsNullOrEmpty(traitType))
            return;

        var config = bus.traitConfigs.FirstOrDefault(c => c.traitType == traitType);
        if (config == null)
        {
            config = new TraitConfiguration { traitType = traitType };
            bus.traitConfigs.Add(config);
        }

        EditorGUI.indentLevel++;
        EditorGUILayout.BeginVertical("box");

        switch (traitType)
        {
            case "ReservedBusTrait":
                EditorGUILayout.LabelField("Reserved Bus Configuration:", EditorStyles.miniLabel);
                config.intValue = EditorGUILayout.IntField("Reserved Capacity", config.intValue == 0 ? 1 : config.intValue);
                break;

            default:
                EditorGUILayout.LabelField("Generic Configuration:", EditorStyles.miniLabel);
                config.intValue = EditorGUILayout.IntField("Int Value", config.intValue);
                config.boolValue = EditorGUILayout.Toggle("Bool Value", config.boolValue);
                config.floatValue = EditorGUILayout.FloatField("Float Value", config.floatValue);
                break;
        }

        EditorGUILayout.EndVertical();
        EditorGUI.indentLevel--;
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
            RemoveAtPosition(gridPos);
        else if (!HasAnyAtPosition(gridPos))
            PlaceSelectedAtPosition(gridPos);
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
        List<TraitConfiguration> configsToAdd = new List<TraitConfiguration>();

        for (int i = 0; i < selectedTraits.Length && i < availableTraits.Count; i++)
        {
            if (selectedTraits[i])
            {
                string trait = availableTraits[i];
                traitsToAdd.Add(trait);

                TraitConfiguration config = new TraitConfiguration { traitType = trait };
                switch (trait)
                {
                    case "BombedTrait":
                        config.intValue = gridEditBombCountdown;
                        break;
                    case "CloakedTrait":
                        config.boolValue = gridEditCloakInitial;
                        break;
                    case "RopedTrait":
                        config.intValue = gridEditRopeLength;
                        break;
                }
                configsToAdd.Add(config);
            }
        }

        switch (selectedType)
        {
            case ObjectType.Passenger:
                levelData.passengers.Add(new PassengerData
                {
                    gridPosition = gridPos,
                    color = selectedColor,
                    traitTypes = traitsToAdd,
                    traitConfigs = configsToAdd
                });
                break;
            case ObjectType.Wall:
                levelData.walls.Add(new WallData
                {
                    gridPosition = gridPos
                });
                break;
            case ObjectType.Tunnel:
                List<PassengerData> initialQueue = new List<PassengerData>
            {
                new PassengerData
                {
                    color = selectedColor,
                    traitTypes = traitsToAdd,
                    traitConfigs = configsToAdd
                }
            };

                levelData.tunnels.Add(new TunnelData
                {
                    gridPosition = gridPos,
                    direction = GetDirectionVector(selectedSpawnDirection),
                    passengerQueue = initialQueue
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

    private void DrawGridEditTraitConfigs()
    {
        for (int i = 0; i < selectedTraits.Length && i < availableTraits.Count; i++)
        {
            if (selectedTraits[i])
            {
                string trait = availableTraits[i];
                switch (trait)
                {
                    case "BombedTrait":
                        gridEditBombCountdown = EditorGUILayout.IntField("Bomb Countdown", gridEditBombCountdown);
                        break;
                    case "CloakedTrait":
                        gridEditCloakInitial = EditorGUILayout.Toggle("Initially Cloaked", gridEditCloakInitial);
                        break;
                    case "RopedTrait":
                        gridEditRopeLength = EditorGUILayout.IntField("Rope Length", gridEditRopeLength);
                        break;
                }
            }
        }
    }
}

public class GridLayoutWindow : EditorWindow
{
    private LevelData levelData;
    private Vector2 scrollPos;
    private int minX = 0;
    private int minY = 0;
    private int viewWidth = 10;
    private int viewHeight = 10;
    private ObjectType brushType = ObjectType.Passenger;
    private PassengerColor brushColor = PassengerColor.Red;
    private SpawnDirection brushDirection = SpawnDirection.Up;

    private bool selectionMode = false;
    private SelectionShape selectionShape = SelectionShape.Rectangle;
    private Vector2Int? startPos = null;
    private Vector2Int? endPos = null;
    private HashSet<Vector2Int> selectedCells = new HashSet<Vector2Int>();

    private enum ObjectType { Passenger, Wall, Tunnel }
    private enum SpawnDirection { Up, Down, Left, Right }
    private enum SelectionShape { Rectangle, Line }

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
        minX = EditorGUILayout.IntField("Min X", minX);
        viewWidth = EditorGUILayout.IntField("View Width", viewWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        minY = EditorGUILayout.IntField("Min Y", minY);
        viewHeight = EditorGUILayout.IntField("View Height", viewHeight);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Auto Fit View"))
            AutoFitView();

        brushType = (ObjectType)EditorGUILayout.EnumPopup("Brush Type", brushType);

        if (brushType != ObjectType.Wall)
            brushColor = (PassengerColor)EditorGUILayout.EnumPopup("Brush Color", brushColor);
        if (brushType == ObjectType.Tunnel)
            brushDirection = (SpawnDirection)EditorGUILayout.EnumPopup("Brush Direction", brushDirection);

        selectionMode = EditorGUILayout.Toggle("Selection Mode", selectionMode);
        if (selectionMode)
            selectionShape = (SelectionShape)EditorGUILayout.EnumPopup("Selection Shape", selectionShape);

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int y = minY + viewHeight - 1; y >= minY; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = minX; x < minX + viewWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                var objInfo = GetObjectAtPosition(gridPos);
                string objType = objInfo?.Item1;
                Color buttonColor = objInfo.HasValue ? objInfo.Value.Item2 : Color.white;
                Vector2Int? dir = objInfo?.Item3;
                string symbol = "";
                if (objType == "Passenger") symbol = "P";
                else if (objType == "Wall") symbol = "W";
                else if (objType == "Tunnel" && dir.HasValue) symbol = GetArrowFromDir(dir.Value);
                string label = symbol;
                if (selectedCells.Contains(gridPos))
                {
                    label = "*" + symbol;
                    if (string.IsNullOrEmpty(symbol)) label = "*";
                }

                GUI.backgroundColor = buttonColor;

                if (GUILayout.Button(label, GUILayout.Width(30), GUILayout.Height(30)))
                {
                    if (selectionMode)
                    {
                        if (!startPos.HasValue)
                            startPos = gridPos;
                        else if (!endPos.HasValue)
                        {
                            endPos = gridPos;
                            UpdateSelectedCells();
                        }
                    }
                    else
                    {
                        if (objInfo.HasValue)
                            RemoveObjectAtPosition(gridPos, objInfo.Value.Item1);
                        else
                            PlaceBrushAtPosition(gridPos);
                        EditorUtility.SetDirty(levelData);
                    }
                }

                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();

        if (selectionMode && endPos.HasValue)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Move Up")) MoveSelected(new Vector2Int(0, 1));
            if (GUILayout.Button("Move Down")) MoveSelected(new Vector2Int(0, -1));
            if (GUILayout.Button("Move Left")) MoveSelected(new Vector2Int(-1, 0));
            if (GUILayout.Button("Move Right")) MoveSelected(new Vector2Int(1, 0));
            if (GUILayout.Button("Clear Selection"))
            {
                startPos = null;
                endPos = null;
                selectedCells.Clear();
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void AutoFitView()
    {
        if (levelData == null) return;

        int min_x = int.MaxValue, min_y = int.MaxValue, max_x = int.MinValue, max_y = int.MinValue;

        Action<Vector2Int> update = (pos) =>
        {
            min_x = Mathf.Min(min_x, pos.x);
            min_y = Mathf.Min(min_y, pos.y);
            max_x = Mathf.Max(max_x, pos.x);
            max_y = Mathf.Max(max_y, pos.y);
        };

        foreach (var p in levelData.passengers) update(p.gridPosition);
        foreach (var w in levelData.walls) update(w.gridPosition);
        foreach (var t in levelData.tunnels) update(t.gridPosition);

        if (min_x == int.MaxValue)
        { // no objects
            minX = 0; minY = 0; viewWidth = 10; viewHeight = 10; return;
        }

        minX = min_x - 1;
        minY = min_y - 1;
        viewWidth = max_x - min_x + 3;
        viewHeight = max_y - min_y + 3;
    }

    private (string, Color, Vector2Int?)? GetObjectAtPosition(Vector2Int gridPos)
    {
        PassengerData passenger = levelData.passengers.FirstOrDefault(p => p.gridPosition == gridPos);
        if (passenger != null)
            return ("Passenger", GetColorFromEnum(passenger.color), null);

        WallData wall = levelData.walls.FirstOrDefault(w => w.gridPosition == gridPos);
        if (wall != null)
            return ("Wall", Color.gray, null);

        TunnelData tunnel = levelData.tunnels.FirstOrDefault(t => t.gridPosition == gridPos);
        if (tunnel != null)
            return ("Tunnel", Color.blue, tunnel.direction);

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
                List<PassengerData> initialQueue = new List<PassengerData>
            {
                new PassengerData
                {
                    color = brushColor,
                    traitTypes = new List<string>()
                }
            };

                levelData.tunnels.Add(new TunnelData
                {
                    gridPosition = gridPos,
                    direction = GetDirectionVector(brushDirection),
                    passengerQueue = initialQueue
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

    private string GetArrowFromDir(Vector2Int dir)
    {
        if (dir == new Vector2Int(0, 1)) return "↑";
        if (dir == new Vector2Int(0, -1)) return "↓";
        if (dir == new Vector2Int(-1, 0)) return "←";
        if (dir == new Vector2Int(1, 0)) return "→";
        return "?";
    }

    private void UpdateSelectedCells()
    {
        if (startPos.HasValue && endPos.HasValue)
        {
            var cells = GetSelectedCells(startPos.Value, endPos.Value, selectionShape);
            selectedCells = new HashSet<Vector2Int>(cells);
        }
    }

    private List<Vector2Int> GetSelectedCells(Vector2Int start, Vector2Int end, SelectionShape sh)
    {
        if (sh == SelectionShape.Rectangle)
        {
            List<Vector2Int> cells = new List<Vector2Int>();
            int minX = Mathf.Min(start.x, end.x);
            int maxX = Mathf.Max(start.x, end.x);
            int minY = Mathf.Min(start.y, end.y);
            int maxY = Mathf.Max(start.y, end.y);
            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    cells.Add(new Vector2Int(x, y));
            return cells;
        }
        else
        {
            List<Vector2Int> points = new List<Vector2Int>();
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;
            int err = dx - dy;
            int x = start.x;
            int y = start.y;
            while (true)
            {
                points.Add(new Vector2Int(x, y));
                if (x == end.x && y == end.y) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
            return points;
        }
    }

    private void MoveSelected(Vector2Int delta)
    {
        foreach (var pos in selectedCells)
        {
            var p = levelData.passengers.FirstOrDefault(pp => pp.gridPosition == pos);
            if (p != null) p.gridPosition += delta;

            var w = levelData.walls.FirstOrDefault(ww => ww.gridPosition == pos);
            if (w != null) w.gridPosition += delta;

            var t = levelData.tunnels.FirstOrDefault(tt => tt.gridPosition == pos);
            if (t != null) t.gridPosition += delta;
        }

        var newSelected = new HashSet<Vector2Int>();
        foreach (var cell in selectedCells)
            newSelected.Add(cell + delta);
        selectedCells = newSelected;

        EditorUtility.SetDirty(levelData);
    }
}
#endif