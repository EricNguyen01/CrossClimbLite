using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        [field: Min(4)]
        public int rowNum { get; private set; } = 5;

        [field: SerializeField]
        [field: Min(4)]
        public int columnNumMin { get; private set; } = 4;

        [field: SerializeField]
        [field: Min(5)]
        public int columnNumMax { get; private set; } = 5;

        [field: SerializeField]
        [field: ReadOnlyInspector]
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

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public GameGridUI gameGridUIInstance { get; private set; }

        [field: Header("Grid Runtime Data")]

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasGridGenerated { get; private set; } = false;

        //INTERNALS.................................................................

        [field: SerializeField]
        [field: HideInInspector]
        [field: FormerlySerializedAs("<wordPlankRowsInGrid>k__BackingField")]
        public WordPlankRow[] wordPlankRowsInGrid { get; private set; }

        public WordPlankRow currentPlankBeingSelected { get; private set; }

        public event Action<string> OnAWordPlankFilled;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (columnNumMax <= columnNumMin) columnNumMax = columnNumMin + 1;

            ValidateRowNumToLockOnStartData();

            if(wordPlankRowsInGrid != null && wordPlankRowsInGrid.Length > 0)
            {
                bool isAllEmptyElement = true;

                for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
                {
                    if (wordPlankRowsInGrid[i])
                    {
                        isAllEmptyElement = false;

                        break;
                    }
                }

                if(isAllEmptyElement)
                {
                    wordPlankRowsInGrid = null;
                }
            }
        }
#endif

        private void Start()
        {
            if (!Application.isPlaying) return;

            if (!FindAnyObjectByType<GameStartState>())
            {
                StartCoroutine(InitGridOnGameStartLoad());
            }
        }

        public IEnumerator InitGridOnGameStartLoad()
        {
            if (!Application.isPlaying) yield break;

            columnNum = UnityEngine.Random.Range(columnNumMin, columnNumMax + 1);

            if (currentPlankBeingSelected) currentPlankBeingSelected = null;

            InitGrid();

            ShuffleWordPlankOrderInGrid();

            if (!gameGridUIInstance) SpawnGameGridUI_IfNull();

            gameGridUIInstance.SpawnNewGridUILayoutFollowingGridLinkedLayout();

            SetActiveFirstCharSlotOfFirstNonKeywordRow();

            yield return new WaitForEndOfFrame();
        }

        private void InitGrid()
        {
            if(wordPlankRowsInGrid != null && wordPlankRowsInGrid.Length > 0)
            {
                RemoveGrid();
            }

            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length != rowNum) 
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

                hasGridGenerated = true;
            }
        }

        private void ShuffleWordPlankOrderInGrid()
        {
            if (wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            List<int> wordPlankRowsKeywordsIndexes = new List<int>();

            List<int> wordPlankRowsNoKeywordsIndexes = new List<int>();

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (!wordPlankRowsInGrid[i]) continue;

                wordPlankRowsInGrid[i].SetPlankRowOrder(i);

                wordPlankRowsInGrid[i].SetPlankHint("");

                if (wordPlankRowsInGrid[i].isPlankKeyword)
                {
                    wordPlankRowsKeywordsIndexes.Add(i);

                    continue;
                }

                wordPlankRowsNoKeywordsIndexes.Add(i);
            }

            if(wordPlankRowsNoKeywordsIndexes == null || wordPlankRowsNoKeywordsIndexes.Count == 0) return;

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (wordPlankRowsInGrid[i] == null) continue;

                int randOrderToSwap = i;

                if (wordPlankRowsInGrid[i].isPlankKeyword)
                {
                    if(wordPlankRowsKeywordsIndexes.Count > 0)
                    {
                        randOrderToSwap = wordPlankRowsKeywordsIndexes[UnityEngine.Random.Range(0, wordPlankRowsKeywordsIndexes.Count)];
                    }

                    if(wordPlankRowsKeywordsIndexes.Count > 0 && wordPlankRowsKeywordsIndexes.Count <= 2)
                    {
                        if (randOrderToSwap != i && wordPlankRowsKeywordsIndexes.Contains(i))
                            wordPlankRowsKeywordsIndexes.Remove(i);

                        wordPlankRowsKeywordsIndexes.Remove(randOrderToSwap);
                    }
                }
                else
                {
                    if(wordPlankRowsNoKeywordsIndexes.Count > 0)
                    {
                        randOrderToSwap = wordPlankRowsNoKeywordsIndexes[UnityEngine.Random.Range(0, wordPlankRowsNoKeywordsIndexes.Count)];
                    }

                    if(wordPlankRowsNoKeywordsIndexes.Count > 0 && wordPlankRowsNoKeywordsIndexes.Count <= 2)
                    {
                        if (randOrderToSwap != i && wordPlankRowsNoKeywordsIndexes.Contains(i))
                            wordPlankRowsNoKeywordsIndexes.Remove(i);

                        wordPlankRowsNoKeywordsIndexes.Remove(randOrderToSwap);
                    }
                }

                if (randOrderToSwap == i) continue;

                WordPlankRow plankToSwap = wordPlankRowsInGrid[randOrderToSwap];

                if(plankToSwap == null) continue;

                int currentPlankSiblingIndex = wordPlankRowsInGrid[i].transform.GetSiblingIndex();

                int plankToSwapSiblingIndex = wordPlankRowsInGrid[randOrderToSwap].transform.GetSiblingIndex();

                wordPlankRowsInGrid[i].transform.SetSiblingIndex(plankToSwapSiblingIndex);

                wordPlankRowsInGrid[randOrderToSwap].transform.SetSiblingIndex(currentPlankSiblingIndex);

                wordPlankRowsInGrid[randOrderToSwap] = wordPlankRowsInGrid[i];

                wordPlankRowsInGrid[i] = plankToSwap;
            }
        }

        public void RemoveGrid()
        {
            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0)
            {
                if(transform.childCount > 0)
                {
                    for(int i = 0; i < transform.childCount; i++)
                    {
                        if (Application.isEditor) DestroyImmediate(transform.GetChild(i).gameObject);

                        else if (Application.isPlaying) Destroy(transform.GetChild(i).gameObject);
                    }
                }

                return;
            }

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if(wordPlankRowsInGrid[i])
                {
                    if (Application.isEditor) DestroyImmediate(wordPlankRowsInGrid[i].gameObject);

                    else if(Application.isPlaying) Destroy(wordPlankRowsInGrid[i].gameObject);
                }
            }

            currentPlankBeingSelected = null;

            wordPlankRowsInGrid = null;
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
                currentPlankBeingSelected.SetGameElementSelectionStatus(false, true);

                currentPlankBeingSelected = selectedPlankRow;
            }
        }

        public void SwapPlankOrderInWordPlanksArray(WordPlankRow plankFrom, WordPlankRow plankTo)
        {
            if(!plankFrom || !plankTo) return;

            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            if(!wordPlankRowsInGrid.Contains(plankFrom) || !wordPlankRowsInGrid.Contains(plankTo)) return;

            int plankFromIndex = Array.IndexOf(wordPlankRowsInGrid, plankFrom);

            int plankToIndex = Array.IndexOf(wordPlankRowsInGrid, plankTo); 

            wordPlankRowsInGrid[plankFromIndex] = plankTo;

            wordPlankRowsInGrid[plankToIndex] = plankFrom;

            OnAWordPlankFilled?.Invoke(plankTo.GetPlankTypedWord());
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

        public void SetPlanksWordsBasedOnWordSet(List<WordSetDataSO.WordHintStruct> wordSetToAssignToPlanks)
        {
            if (wordSetToAssignToPlanks == null || wordSetToAssignToPlanks.Count == 0) return;

            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (!wordPlankRowsInGrid[i]) continue;

                int plankOrder = wordPlankRowsInGrid[i].plankRowOrder;

                if(plankOrder >= wordSetToAssignToPlanks.Count) continue;

                wordPlankRowsInGrid[i].SetPlankCorrectWord(wordSetToAssignToPlanks[plankOrder].word);

                wordPlankRowsInGrid[i].SetPlankHint(wordSetToAssignToPlanks[plankOrder].hint);
            }

            DisplayHintOfCurrentSelectedPlank();
        }

        public void DisplayHintOfCurrentSelectedPlank()
        {
            if(!currentPlankBeingSelected) return;

            string hint = currentPlankBeingSelected.plankHint;

            if (HintBoxUI.hintBoxUIInstance) HintBoxUI.hintBoxUIInstance.SetHintTextToDisplay(hint);
        }

        public void UnlockKeywordPlanksInGrid(bool unlock = true, bool selectFirstKeywordPlankOnUnlocked = true)
        {
            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            bool hasSelectedFirst = false;

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (!wordPlankRowsInGrid[i]) continue;

                if (!wordPlankRowsInGrid[i].isPlankKeyword) continue;

                if (unlock)
                {
                    if (wordPlankRowsInGrid[i].isPlankLocked)
                    {
                        wordPlankRowsInGrid[i].SetGameElementLockedStatus(false, true);

                        if (selectFirstKeywordPlankOnUnlocked && !hasSelectedFirst)
                        {
                            wordPlankRowsInGrid[i].SetGameElementSelectionStatus(true, true);

                            if (wordPlankRowsInGrid[i].letterSlotsInWordPlank != null && wordPlankRowsInGrid[i].letterSlotsInWordPlank.Length > 0)
                            {
                                for(int j = 0; j < wordPlankRowsInGrid[j].letterSlotsInWordPlank.Length; j++)
                                {
                                    if (!wordPlankRowsInGrid[i].letterSlotsInWordPlank[j]) continue;

                                    wordPlankRowsInGrid[i].letterSlotsInWordPlank[j].SetGameElementSelectionStatus(true, true);

                                    break;
                                }
                            }

                            hasSelectedFirst = true;
                        }
                    }
                }
                else
                {
                    if (currentPlankBeingSelected && currentPlankBeingSelected == wordPlankRowsInGrid[i])
                    {
                        wordPlankRowsInGrid[i].SetGameElementSelectionStatus(false, true);

                        SetCurrentPlankRowSelected(null);
                    }

                    if (!wordPlankRowsInGrid[i].isPlankLocked)
                    {
                        wordPlankRowsInGrid[i].SetGameElementLockedStatus(true, true);
                    }
                }
            }
        }

        public void UnlockNonKeywordPlanksInGrid(bool unlock = true, bool selectFirstNonKeywordPlankOnUnlocked = true)
        {
            if (wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            bool hasSelectedFirst = false;

            for (int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (!wordPlankRowsInGrid[i]) continue;

                if (wordPlankRowsInGrid[i].isPlankKeyword) continue;

                if (unlock)
                {
                    if (wordPlankRowsInGrid[i].isPlankLocked)
                    {
                        wordPlankRowsInGrid[i].SetGameElementLockedStatus(false, true);

                        if (selectFirstNonKeywordPlankOnUnlocked && !hasSelectedFirst)
                        {
                            wordPlankRowsInGrid[i].SetGameElementSelectionStatus(true, true);

                            hasSelectedFirst = true;
                        }
                    }
                }
                else
                {
                    if (currentPlankBeingSelected && currentPlankBeingSelected == wordPlankRowsInGrid[i])
                    {
                        wordPlankRowsInGrid[i].SetGameElementSelectionStatus(false, true);

                        SetCurrentPlankRowSelected(null);
                    }

                    if (!wordPlankRowsInGrid[i].isPlankLocked)
                    {
                        wordPlankRowsInGrid[i].SetGameElementLockedStatus(true, true);
                    }
                }
            }
        }

        public void ClearAllPlanksTypedWords()
        {
            if (wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (!wordPlankRowsInGrid[i]) continue;

                wordPlankRowsInGrid[i].ClearPlankTypedWord();
            }
        }

        public void SetActiveFirstCharSlotOfFirstNonKeywordRow()
        {
            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            for (int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if (!wordPlankRowsInGrid[i]) continue;

                if (wordPlankRowsInGrid[i].isPlankKeyword || wordPlankRowsInGrid[i].isPlankLocked) continue;

                PlankLetterSlot[] letterSlotsInCurrentPlank = wordPlankRowsInGrid[i].letterSlotsInWordPlank;

                if (letterSlotsInCurrentPlank == null || letterSlotsInCurrentPlank.Length == 0) continue;

                for (int j = 0; j < letterSlotsInCurrentPlank.Length; j++)
                {
                    if (!letterSlotsInCurrentPlank[j]) continue;

                    letterSlotsInCurrentPlank[j].SetGameElementSelectionStatus(true, true);

                    return;
                }
            }
        }

        public void InvokeOnAWordPlankFilledEvent(string wordPlankFilledWord)
        {
            OnAWordPlankFilled?.Invoke(wordPlankFilledWord);
        }

        public override void SetGameElementSelectionStatus(bool isSelected, bool shouldUpdateUI) { }

        public override void SetGameElementLockedStatus(bool isLocked, bool shouldUpdateUI)
        {
            if (shouldUpdateUI && gameGridUIInstance) gameGridUIInstance.UpdateUI_OnGameElementModalLocked(isLocked);
        }

        //EDITOR FUNCS...............................................................................................

#if UNITY_EDITOR

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

            private SerializedProperty wordPlankRowsProp;

            private bool wordPlankRowsFoldout = true;

            private void OnEnable()
            {
                gameGrid = (GameGrid)target;

                wordPlankRowsProp = serializedObject.FindProperty("wordPlankRowsInGrid");

                if (wordPlankRowsProp == null) wordPlankRowsProp = serializedObject.FindProperty("<wordPlankRowsInGrid>k__BackingField");
            }

            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();

                EditorGUILayout.Space(15);

                DrawReadOnlyList(wordPlankRowsProp, ref wordPlankRowsFoldout);

                EditorGUILayout.Space(15);

                EditorGUILayout.HelpBox("Generating the grid removes the current game grid and its layout", MessageType.Info);

                //Create a custom inspector button to execute the generation of the game grid layout in the editor.
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    if (GUILayout.Button("Generate Grid Modal and UI View"))//On Generate Grid button pressed:...
                    {
                        gameGrid.InitGrid();

                        serializedObject.Update();

                        EditorUtility.SetDirty(this);

                        serializedObject.ApplyModifiedProperties();
                    }

                    if (GUILayout.Button("Remove Grid Layout and UI"))//On Generate Grid button pressed:...
                    {
                        gameGrid.RemoveGrid();

                        serializedObject.Update();

                        EditorUtility.SetDirty(this);

                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            private void DrawReadOnlyList(SerializedProperty listProp, ref bool foldout)
            {
                if (listProp == null) return;

                // Toggle foldout
                foldout = EditorGUILayout.Foldout(foldout, $"{listProp.displayName} Read-Only", true);

                if (!foldout) return;

                EditorGUI.indentLevel++;

                int size = listProp.arraySize;

                if (size == 0)
                {
                    EditorGUILayout.LabelField("List is empty.");

                    EditorGUI.indentLevel--;

                    return;
                }

                for (int i = 0; i < listProp.arraySize; i++)
                {
                    var element = listProp.GetArrayElementAtIndex(i);

                    if (element == null) continue;

                    GUI.enabled = false;

                    // Draw box-like element
                    EditorGUILayout.BeginVertical("box");

                    EditorGUILayout.PropertyField(element);

                    EditorGUILayout.EndVertical();

                    GUI.enabled = true;
                }

                EditorGUI.indentLevel--;
            }
        }
#endif
    }
}
