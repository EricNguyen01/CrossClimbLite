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

        [field: SerializeField]
        private List<int> rowNumToLockOnStart = new List<int>();

        [Space]

        [Header("Game Grid UI")]

        [SerializeField]
        private GameGridUI gameGridUIPrefabToSpawn;

        [SerializeField]
        private Canvas canvasToPlaceGameGridUI;

        private GameGridUI gameGridUIInstance;

        private WordPlankRow[] wordPlankRowsInGrid;

        public WordPlankRow currentPlankBeingSelected { get; private set; }

        public void InitGrid()
        {
            SpawnGameGridUI_IfNull();

            if(wordPlankRowsInGrid != null && wordPlankRowsInGrid.Length > 0)
            {
                RemoveGrid();
            }

            wordPlankRowsInGrid = new WordPlankRow[rowNum];

            for (int i = 0; i < rowNum; i++)
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
                        wordPlankRowComp.SetPlankLockStatus(true);
                    }
                }

                wordPlankRowsInGrid[i] = wordPlankRowComp;
            }

            if (gameGridUIInstance) gameGridUIInstance.InitGridUIGameElements();
        }

        public void RemoveGrid()
        {
            if(wordPlankRowsInGrid == null || wordPlankRowsInGrid.Length == 0) return;

            for(int i = 0; i < wordPlankRowsInGrid.Length; i++)
            {
                if(wordPlankRowsInGrid[i])
                {
                    Destroy(wordPlankRowsInGrid[i].gameObject);
                }
            }

            if (gameGridUIInstance) gameGridUIInstance.RemoveGridUIGameElements();
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
                currentPlankBeingSelected.SetPlankRowSelectedStatus(false);

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

            if (!canvasToPlaceGameGridUI) return;

            gameGridUIInstance = Instantiate(gameGridUIPrefabToSpawn, canvasToPlaceGameGridUI.transform);
        }
    }
}
