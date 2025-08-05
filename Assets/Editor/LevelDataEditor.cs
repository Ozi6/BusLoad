#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    private LevelData levelData;
    private Vector2 scrollPosition;
    private int selectedPassengerIndex = -1;
    private bool showPassengers = true;
    private bool showBuses = true;

    private bool gridEditMode = false;
    private PassengerColor selectedColor = PassengerColor.Red;
    private List<string> availableTraits = new List<string>();
    private bool[] selectedTraits = new bool[0];

    private void OnEnable()
    {
        levelData = (LevelData)target;
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
            EditorGUILayout.HelpBox("Click on grid cells in Scene view to place/remove passengers", MessageType.Info);

            selectedColor = (PassengerColor)EditorGUILayout.EnumPopup("Passenger Color", selectedColor);

            EditorGUILayout.LabelField("Traits:", EditorStyles.boldLabel);
            for (int i = 0; i < availableTraits.Count; i++)
            {
                if (i < selectedTraits.Length)
                    selectedTraits[i] = EditorGUILayout.Toggle(availableTraits[i], selectedTraits[i]);
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
                    TogglePassengerAtPosition(gridCell.gridPosition);
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
            Handles.color = Color.white;
        }
    }

    private void TogglePassengerAtPosition(Vector2Int gridPos)
    {
        PassengerData existingPassenger = levelData.passengers.FirstOrDefault(p => p.gridPosition == gridPos);

        if (existingPassenger != null)
            levelData.passengers.Remove(existingPassenger);
        else
        {
            List<string> traitsToAdd = new List<string>();
            for (int i = 0; i < selectedTraits.Length && i < availableTraits.Count; i++)
            {
                if (selectedTraits[i])
                    traitsToAdd.Add(availableTraits[i]);
            }

            levelData.passengers.Add(new PassengerData
            {
                gridPosition = gridPos,
                color = selectedColor,
                traitTypes = traitsToAdd
            });
        }

        RefreshGameManager();
        EditorUtility.SetDirty(levelData);
    }
}

public class GridLayoutWindow : EditorWindow
{
    private LevelData levelData;
    private Vector2 scrollPos;
    private int gridWidth = 10;
    private int gridHeight = 10;
    private PassengerColor brushColor = PassengerColor.Red;

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

        brushColor = (PassengerColor)EditorGUILayout.EnumPopup("Brush Color", brushColor);

        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        for (int y = gridHeight - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < gridWidth; x++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                PassengerData passenger = levelData.passengers.FirstOrDefault(p => p.gridPosition == gridPos);

                Color buttonColor = passenger != null ? GetColorFromEnum(passenger.color) : Color.gray;
                GUI.backgroundColor = buttonColor;

                if (GUILayout.Button("", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    if (passenger != null)
                    {
                        levelData.passengers.Remove(passenger);
                    }
                    else
                    {
                        levelData.passengers.Add(new PassengerData
                        {
                            gridPosition = gridPos,
                            color = brushColor,
                            traitTypes = new List<string>()
                        });
                    }
                    EditorUtility.SetDirty(levelData);
                }

                GUI.backgroundColor = Color.white;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
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