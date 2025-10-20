using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Unity.Mathematics;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the Modal) only representation of a game grid (where word planks or rows are placed).
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

        [Tooltip("Modify the specified row num's data (isLocked, isKeyword, etc.) on start. " +
                        "Row num starts from 0 to RowNum - 1. " +
                        "Should not contain duplicate row nums.")]

        [SerializeField]
        private List<RowNumDataOnStart> rowNumDataToModifyOnStart = new List<RowNumDataOnStart>();

        [Serializable]
        private class RowNumDataOnStart
        {
            [field: SerializeField]
            public int rowNum { get; private set; }

            [field: SerializeField]
            public bool isLocked { get; private set; } = true;

            [field: SerializeField]
            public bool isKeyword { get; private set; } = true;

            public void SetRowNum(int num)
            {
                rowNum = num;
            }

            public void SetIsLocked(bool isLocked)
            {
                this.isLocked = isLocked;
            }

            public void SetIsKeyword(bool isKeyword)
            {
                this.isKeyword = isKeyword;
            }
        }

        [SerializeField]
        [HideInInspector]
        private List<int> validRowNum = new List<int>();

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

                bool alreadyInitWordPlankComp = false;

                if (rowNumDataToModifyOnStart != null && rowNumDataToModifyOnStart.Count > 0)
                {
                    List<RowNumDataOnStart> rowNumDataToModifyList = new List<RowNumDataOnStart>();

                    rowNumDataToModifyList.AddRange(rowNumDataToModifyOnStart);

                    for(int j = 0; j < rowNumDataToModifyList.Count; j++)
                    {
                        if (rowNumDataToModifyList[j].rowNum == i)
                        {
                            bool isKeyword = rowNumDataToModifyList[j].isKeyword;

                            bool isLocked = rowNumDataToModifyList[j].isLocked;

                            wordPlankRowComp.InitPlank(this, i, columnNum, isKeyword, isLocked);

                            alreadyInitWordPlankComp = true;

                            rowNumDataToModifyList.RemoveAt(j);

                            break;
                        }
                    }
                }

                if(!alreadyInitWordPlankComp) wordPlankRowComp.InitPlank(this, i, columnNum);

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
            if (rowNumDataToModifyOnStart == null || rowNumDataToModifyOnStart.Count == 0)
            {
                validRowNum.Clear();
            }

            if (rowNum < 1)
            {
                rowNumDataToModifyOnStart.Clear();

                validRowNum.Clear();

                return;
            }

            if(rowNum == 1)
            {
                RowNumDataOnStart temp = new RowNumDataOnStart();

                temp.SetRowNum(rowNumDataToModifyOnStart[0].rowNum);

                temp.SetIsLocked(rowNumDataToModifyOnStart[0].isLocked);

                temp.SetIsKeyword(rowNumDataToModifyOnStart[0].isKeyword);

                if(rowNumDataToModifyOnStart.Count > 1)
                {
                    rowNumDataToModifyOnStart.Clear();

                    validRowNum.Clear();

                    rowNumDataToModifyOnStart.Add(temp);

                    validRowNum.Add(temp.rowNum);
                }

                return;
            }

            if(rowNumDataToModifyOnStart.Count - validRowNum.Count == 1)
            {
                if(validRowNum.Count == 0 && rowNumDataToModifyOnStart.Count == 1)
                {
                    validRowNum.Add(rowNumDataToModifyOnStart[0].rowNum);

                    return;
                }

                for(int i = 0; i < rowNum; i++)
                {
                    if (validRowNum.Contains(i)) continue;

                    rowNumDataToModifyOnStart[rowNumDataToModifyOnStart.Count - 1].SetRowNum(i);

                    validRowNum.Add(i);

                    return;
                }

                rowNumDataToModifyOnStart.RemoveAt(rowNumDataToModifyOnStart.Count - 1);

                return;
            }

            if(rowNumDataToModifyOnStart.Count < validRowNum.Count)
            {
                validRowNum.Clear();

                for(int i = 0; i < rowNumDataToModifyOnStart.Count; i++)
                {
                    validRowNum.Add(rowNumDataToModifyOnStart[i].rowNum);
                }

                return;
            }

            List<int> existingRowNum = new List<int>();

            for(int i = 0; i < rowNumDataToModifyOnStart.Count; i++)
            {
                if (!existingRowNum.Contains(rowNumDataToModifyOnStart[i].rowNum))
                {
                    existingRowNum.Add(rowNumDataToModifyOnStart[i].rowNum);

                    continue;
                }

                rowNumDataToModifyOnStart.RemoveAt(i);
            }

            if (rowNumDataToModifyOnStart.Count == 0) return;

            validRowNum.Clear();

            if(rowNumDataToModifyOnStart.Count == 1)
            {
                validRowNum.Add(rowNumDataToModifyOnStart[0].rowNum);

                return;
            }

            for(int i = 0; i < rowNumDataToModifyOnStart.Count; i++)
            {
                validRowNum.Add(rowNumDataToModifyOnStart[i].rowNum);
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
                    if (GUILayout.Button("Generate Grid Modal and UI View"))//On Generate Grid button pressed:...
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
