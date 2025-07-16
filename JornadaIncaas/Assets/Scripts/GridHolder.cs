using System;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Pipe Layout", menuName = "Level/Pipe Layout")]
public class GridHolder : ScriptableObject
{
    public int rows = 4; // Default rows
    public int columns = 4; // Default columns

    // Change Wrapper to hold PipeData instead of just PipeType
    public Wrapper<PipeData>[] grid;

    private void Awake()
    {
        // Ensure grid is initialized or reset when the object is loaded
        if (grid == null || grid.Length != rows)
        {
            ResetGrid();
        }
        else
        {
            for (int i = 0; i < rows; i++)
            {
                if (grid[i] == null || grid[i].values == null || grid[i].values.Length != columns)
                {
                    ResetGrid();
                    break;
                }
            }
        }
    }

    public void ResetGrid()
    {
        grid = new Wrapper<PipeData>[rows]; // Use PipeData
        for (int i = 0; i < rows; i++)
        {
            grid[i] = new Wrapper<PipeData>();
            grid[i].values = new PipeData[columns]; // Use PipeData
            // Initialize each PipeData element with default values
            for (int j = 0; j < columns; j++)
            {
                grid[i].values[j] = new PipeData(); // Default PipeType.Straight, rotation 0
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GridHolder))]
    public class GridHolderEditor : Editor
    {
        private SerializedProperty gridProperty;
        private SerializedProperty rowsProperty;
        private SerializedProperty columnsProperty;

        private int enumLength; // Total number of PipeType enums

        private void OnEnable()
        {
            gridProperty = serializedObject.FindProperty("grid");
            rowsProperty = serializedObject.FindProperty("rows");
            columnsProperty = serializedObject.FindProperty("columns");
            enumLength = Enum.GetValues(typeof(PipeType)).Length;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GridHolder script = (GridHolder)target;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rowsProperty);
            EditorGUILayout.PropertyField(columnsProperty);
            if (EditorGUI.EndChangeCheck())
            {
                script.ResetGrid();
            }

            DrawGrid(script);

            if (GUILayout.Button("Reset Grid"))
            {
                script.ResetGrid();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGrid(GridHolder script)
        {
            try
            {
                GUILayout.BeginVertical();
                for (int i = 0; i < script.rows; i++)
                {
                    GUILayout.BeginHorizontal();
                    if (gridProperty.arraySize > i && gridProperty.GetArrayElementAtIndex(i) != null)
                    {
                        SerializedProperty arrayElement = gridProperty.GetArrayElementAtIndex(i);
                        SerializedProperty valuesArray = arrayElement.FindPropertyRelative("values");

                        if (valuesArray != null && valuesArray.arraySize == script.columns)
                        {
                            for (int j = 0; j < script.columns; j++)
                            {
                                SerializedProperty pipeDataProp = valuesArray.GetArrayElementAtIndex(j);
                                SerializedProperty pipeTypeProp = pipeDataProp.FindPropertyRelative("pipeType");
                                SerializedProperty initialRotationProp = pipeDataProp.FindPropertyRelative("initialRotationIndex");

                                // Display PipeType dropdown
                                EditorGUILayout.PropertyField(pipeTypeProp, GUIContent.none, GUILayout.MaxWidth(70));

                                // Display rotation button
                                PipeType currentPipeType = (PipeType)pipeTypeProp.intValue;
                                int currentRotation = initialRotationProp.intValue;
                                if (GUILayout.Button(currentRotation.ToString(), GUILayout.MaxWidth(30))) // Display current rotation number
                                {
                                    initialRotationProp.intValue = (currentRotation + 1) % 4; // Rotate by clicking
                                }
                            }
                        }
                        else
                        {
                            script.ResetGrid();
                        }
                    }
                    else
                    {
                        script.ResetGrid();
                        break;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Error drawing grid: " + e.Message);
            }
        }

        // Removed NextIndex as we're handling rotation directly for PipeData
    }
#endif

    [Serializable]
    public class Wrapper<T>
    {
        public T[] values;
    }

    // New class to hold PipeType and its initial rotation
    [Serializable]
    public class PipeData
    {
        public PipeType pipeType = PipeType.Straight; // Default type
        public int initialRotationIndex = 0; // Default rotation (0, 90, 180, 270 degrees)
    }
}
