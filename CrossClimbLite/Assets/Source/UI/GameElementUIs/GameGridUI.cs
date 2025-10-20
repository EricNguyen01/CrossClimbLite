using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameGridUI : GameElementUIBase
    {
        [Header("Required Game UI Prefabs")]

        [SerializeField]
        private WordPlankRowUI wordPlankUIPrefabToSpawn;

        private List<WordPlankRowUI> plankUISpawned = new List<WordPlankRowUI>();   

        [SerializeField]
        private PlankLetterSlotUI letterSlotUIPrefabToSpawn;

        private List<PlankLetterSlotUI> letterSlotUISpawned = new List<PlankLetterSlotUI>();

        [Header("Spawn Specifications")]

        [SerializeField]
        private VerticalLayoutGroup verticalGroupToSpawnPlanksUnder;

        private GameGrid gameGridLinked;

        protected override void Awake()
        {
            base.Awake();

            if (!verticalGroupToSpawnPlanksUnder)
            {
                TryGetComponent<VerticalLayoutGroup>(out verticalGroupToSpawnPlanksUnder);
            }

            if (!verticalGroupToSpawnPlanksUnder)
            {
                verticalGroupToSpawnPlanksUnder = GetComponentInChildren<VerticalLayoutGroup>();
            }

            if (!verticalGroupToSpawnPlanksUnder)
            {
                Debug.LogError("Warning, missing vertical layout group ref on: " + name + ". Game UI will not work correctly!");
            }
        }

        public override void InitGameElementUI(GameElementBase gameGridToLink)
        {
            base.InitGameElementUI(gameGridToLink);

            if (!gameGridToLink) return;

            if (gameGridToLink is not GameGrid) return;

            gameGridLinked = gameGridToLink as GameGrid;
        }

        public void OnGameGridInitOrRemove()
        {
            if (!gameGridLinked) return;

            if (gameGridLinked.wordPlankRowsInGrid == null || gameGridLinked.wordPlankRowsInGrid.Length == 0)
            {
                RemoveCurrentGridUILayout();

                return;
            }

            if (gameGridLinked.wordPlankRowsInGrid != null && gameGridLinked.wordPlankRowsInGrid.Length > 0)
            {
                SpawnNewGridUILayoutFollowingGridLinkedLayout();
            }
        }

        private void SpawnNewGridUILayoutFollowingGridLinkedLayout()
        {
            if (!verticalGroupToSpawnPlanksUnder) return;

            if(plankUISpawned == null) plankUISpawned = new List<WordPlankRowUI>();

            if (letterSlotUISpawned == null) letterSlotUISpawned = new List<PlankLetterSlotUI>();

            if (plankUISpawned != null && plankUISpawned.Count > 0)
            {
                RemoveCurrentGridUILayout();
            }

            if (gameGridLinked.wordPlankRowsInGrid == null || gameGridLinked.wordPlankRowsInGrid.Length == 0) return;

            if(gameGridLinked.wordPlankRowsInGrid.Length > 4)
            {
                verticalGroupToSpawnPlanksUnder.childForceExpandHeight = true;

                verticalGroupToSpawnPlanksUnder.childForceExpandWidth = true;

                verticalGroupToSpawnPlanksUnder.childControlHeight = true;

                verticalGroupToSpawnPlanksUnder.childControlWidth = true;
            }
            else
            {
                verticalGroupToSpawnPlanksUnder.childControlHeight = false;

                verticalGroupToSpawnPlanksUnder.childControlWidth = false;

                verticalGroupToSpawnPlanksUnder.childForceExpandHeight = false;

                verticalGroupToSpawnPlanksUnder.childForceExpandWidth = false;
            }

            for(int i = 0; i < gameGridLinked.wordPlankRowsInGrid.Length; i++)
            {
                if (!gameGridLinked.wordPlankRowsInGrid[i]) continue;

                WordPlankRow rowModal = gameGridLinked.wordPlankRowsInGrid[i];

                GameObject rowUIObj = Instantiate(wordPlankUIPrefabToSpawn.gameObject, verticalGroupToSpawnPlanksUnder.transform);

                rowUIObj.name = rowUIObj.name + "_" + i;

                WordPlankRowUI rowUI = rowUIObj.GetComponent<WordPlankRowUI>();

                if (rowUI) rowUI.InitGameElementUI(rowModal);

                if (gameGridLinked.wordPlankRowsInGrid[i].letterSlotsInWordPlank == null ||
                    gameGridLinked.wordPlankRowsInGrid[i].letterSlotsInWordPlank.Length == 0) continue;

                PlankLetterSlot[] letterSlotModals = gameGridLinked.wordPlankRowsInGrid[i].letterSlotsInWordPlank;

                for (int j = 0; j < letterSlotModals.Length; j++)
                {
                    if (!letterSlotModals[j]) continue;

                    GameObject letterSlotUIObj = null;

                    if (rowUI)
                    {
                        if(rowUI.horizontalLayoutToSpawnLetterSlotsUnder) 
                            letterSlotUIObj = Instantiate(letterSlotUIPrefabToSpawn.gameObject, rowUI.horizontalLayoutToSpawnLetterSlotsUnder.transform);

                        else letterSlotUIObj = Instantiate(letterSlotUIPrefabToSpawn.gameObject, rowUI.transform);
                    }

                    if(!letterSlotUIObj) continue;

                    letterSlotUIObj.name = letterSlotUIObj.name + "_" + j;

                    PlankLetterSlotUI letterSlotUI = letterSlotUIObj.GetComponent<PlankLetterSlotUI>();

                    if(letterSlotUI) letterSlotUI.InitGameElementUI(letterSlotModals[j]);
                }
            }
        }

        private void RemoveCurrentGridUILayout()
        {
            if (plankUISpawned == null || plankUISpawned.Count == 0) return;

            for (int i = 0; i < plankUISpawned.Count; i++)
            {
                if (!plankUISpawned[i]) continue;

                if (Application.isEditor) DestroyImmediate(plankUISpawned[i]);

                else if(Application.isPlaying) Destroy(plankUISpawned[i]);
            }
        }
    }
}
