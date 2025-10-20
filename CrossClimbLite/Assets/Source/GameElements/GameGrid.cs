using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the model) only representation of a game grid (where word planks or rows are placed).
     * This class is none UI.
     */
    public class GameGrid : GameElementBase
    {
        [field: Header("Game Grid Settings")]

        [field: SerializeField]
        [field: Min(1)]
        public int rowNum { get; private set; } = 5;

        [field: SerializeField]
        [field: Min(4)]
        public int columnNum { get; private set; } = 4;

        [Space]

        [field: HelpBox("Define the row number to lock on game start. Row num starts from 0 to RowNum - 1. " +
                 "Should not contain duplicate row nums.",
                 UnityEngine.UIElements.HelpBoxMessageType.Info)]

        [field: SerializeField]
        private List<int> rowNumToLockOnStart = new List<int>();

        //use for rowNumToLockOnStart above list data validation - editor only and not gameplay related!
        [SerializeField]
        [HideInInspector]
        private List<int> validRowNumToLock = new List<int>();

        [Space]

        [Header("Game Grid UI")]

        [SerializeField]
        private GameGridUI gameGridUIPrefabToSpawn;

        [SerializeField]
        private Canvas canvasToPlaceGameGridUI;

        private GameGridUI gameGridUIInstance;

        [field: SerializeField]
        [field: HideInInspector]
        public WordPlankRow[] wordPlankRowsInGrid { get; private set; }

        public WordPlankRow currentPlankBeingSelected { get; private set; }

        private void OnValidate()
        {
            ValidateRowNumToLockOnStartData();
        }

        private void Start()
        {
            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0)
            {
                InitGrid();
            }
        }

        public void InitGrid()
        {
            SpawnGameGridUI_IfNull();

            if(wordPlankRowsInGrid != null && wordPlankRowsInGrid.Length > 0)
            {
                RemoveGrid();
            }

            wordPlankRowsInGrid = new WordPlankRow[rowNum];
            
            for (int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                GameObject wordPlankRowObj = new GameObject("WordPlankRow_" + i);

                wordPlankRowObj.transform.SetParent(transform);

                wordPlankRowObj.transform.localPosition = Vector3.zero;

                WordPlankRow wordPlankRowComp = wordPlankRowObj.AddComponent<WordPlankRow>();

                wordPlankRowComp.InitPlank(this, i, columnNum);

                if (rowNumToLockOnStart != null && rowNumToLockOnStart.Count > 0)
                {
                    if (rowNumToLockOnStart.Contains(i))
                    {
                        wordPlankRowComp.SetGameElementLockedStatus(true, false);
                    }
                }

                wordPlankRowsInGrid[i] = wordPlankRowComp;
            }

            if (gameGridUIInstance) gameGridUIInstance.OnGameGridInitOrRemove();
        }

        public void RemoveGrid()
        {
            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if(wordPlankRowsInGrid[i])
                {
                    if (Application.isEditor) DestroyImmediate(wordPlankRowsInGrid[i].gameObject);

                    else if(Application.isPlaying) Destroy(wordPlankRowsInGrid[i].gameObject);
                }
            }

            wordPlankRowsInGrid = new WordPlankRow[rowNum];

            if (gameGridUIInstance) gameGridUIInstance.OnGameGridInitOrRemove();
        }

        public void SetCurrentPlankRowSelected(WordPlankRow selectedPlankRow)
        {
            if (!currentPlankBeingSelected)
            {
                currentPlankBeingSelected = selectedPlankRow;

                return;
            }

            if (currentPlankBeingSelected && selectedPlankRow != currentPlankBeingSelected)
            {
                currentPlankBeingSelected.SetGameElementSelectionStatus(false, false);

                currentPlankBeingSelected = selectedPlankRow;
            }
        }

        private void SpawnGameGridUI_IfNull()
        {
            if (!gameGridUIPrefabToSpawn) return;

            if (gameGridUIInstance) return;

            if (!canvasToPlaceGameGridUI)
            {
                canvasToPlaceGameGridUI = FindAnyObjectByType<Canvas>();
            }

            if (!canvasToPlaceGameGridUI)
            {
                Debug.LogError("Could not find a valid UI Canvas to spawn the game grid layout under!");

                return;
            }

            gameGridUIInstance = Instantiate(gameGridUIPrefabToSpawn, canvasToPlaceGameGridUI.transform);

            gameGridUIInstance.InitGameElementUI(this);
        }

        public override void SetGameElementSelectionStatus(bool isSelected, bool isFromUI) { }

        public override void SetGameElementLockedStatus(bool isLocked, bool isFromUI) { }

        //EDITOR FUNCS...............................................................................................

        private void ValidateRowNumToLockOnStartData()
        {
            if (rowNumToLockOnStart.Count == 0)
            {
                if (validRowNumToLock.Count != 0)
                    validRowNumToLock.Clear();

                return;
            }

            if (rowNumToLockOnStart.Count - validRowNumToLock.Count == 1)
            {
                if(rowNumToLockOnStart.Count == 1)
                {
                    if (validRowNumToLock.Count > 0) validRowNumToLock.Clear();

                    validRowNumToLock.Add(rowNumToLockOnStart[rowNumToLockOnStart.Count - 1]);

                    return;
                }

                int newRowNumToAdd = -1;

                for(int i = 0; i < rowNum; i++)
                {
                    if (rowNumToLockOnStart.Contains(i)) continue;

                    newRowNumToAdd = i;

                    break;
                }

                if (newRowNumToAdd == -1)
                {
                    rowNumToLockOnStart.Clear();

                    rowNumToLockOnStart.AddRange(validRowNumToLock);

                    return;
                }

                rowNumToLockOnStart[rowNumToLockOnStart.Count - 1] = newRowNumToAdd;

                validRowNumToLock.Add(rowNumToLockOnStart[rowNumToLockOnStart.Count - 1]);

                return;
            }

            if (rowNumToLockOnStart.Count == 1 && validRowNumToLock.Count == 1)
            {
                if (rowNumToLockOnStart[0] < 0) rowNumToLockOnStart[0] = 0;

                if (rowNumToLockOnStart[0] > rowNum - 1) rowNumToLockOnStart[0] = rowNum - 1;

                if (validRowNumToLock[0] != rowNumToLockOnStart[0])
                    validRowNumToLock[0] = rowNumToLockOnStart[0];

                return;
            }

            if (rowNumToLockOnStart.Count < validRowNumToLock.Count)
            {
                validRowNumToLock.Clear();

                validRowNumToLock.AddRange(rowNumToLockOnStart);

                return;
            }

            List<int> temp = new List<int>();

            for (int i = 0; i < rowNumToLockOnStart.Count; i++)
            {
                if (rowNumToLockOnStart[i] < 0) rowNumToLockOnStart[i] = 0;

                if (rowNumToLockOnStart[i] > rowNum - 1) rowNumToLockOnStart[i] = rowNum - 1;

                if (temp.Contains(rowNumToLockOnStart[i])) continue;

                temp.Add(rowNumToLockOnStart[i]);
            }

            if(temp.Count > 0)
            {
                validRowNumToLock.Clear();

                rowNumToLockOnStart.Clear();

                validRowNumToLock.AddRange(temp);

                rowNumToLockOnStart.AddRange(temp);
            }
        }

        //EDITOR CLASS...............................................................................

        [CustomEditor(typeof(GameGrid))]
        private class GameGridEditor : Editor
        {
            private GameGrid gameGrid;

            private void OnEnable()
            {
                gameGrid = (GameGrid)target;
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                EditorGUILayout.Space(12);

                EditorGUILayout.HelpBox("Generating the grid removes the current game grid and its layout", MessageType.Info);

                //Create a custom inspector button to execute the generation of the game grid layout in the editor.
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Generate Grid Model and UI View"))//On Generate Grid button pressed:...
                    {
                        gameGrid.InitGrid();
                    }

                    if (GUILayout.Button("Remove Grid Layout and UI"))//On Generate Grid button pressed:...
                    {
                        gameGrid.RemoveGrid();
                    }
                }
            }
        }
    }
}
