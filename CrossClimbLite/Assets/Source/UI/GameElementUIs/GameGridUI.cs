using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameGridUI : GameElementUIBase
    {
        [field: Header("Required Word Plank UI Prefabs")]

        [field: SerializeField]
        public WordPlankRowUI wordPlankUIPrefabToSpawn { get; private set; }

        public List<WordPlankRowUI> plankUISpawned { get; private set; } = new List<WordPlankRowUI>();

        [field: Header("Required Plank UIs Spawn Vertical Group")]

        [field: SerializeField]
        public VerticalLayoutGroup verticalGroupToSpawnPlanksUnder { get; private set; }

        public GameGrid gameGridLinked { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            if (!verticalGroupToSpawnPlanksUnder)
            {
                verticalGroupToSpawnPlanksUnder = GetComponent<VerticalLayoutGroup>();
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

            gameGridLinked.ConnectGameElementUI(this);

            if (!verticalGroupToSpawnPlanksUnder)
            {
                verticalGroupToSpawnPlanksUnder = GetComponent<VerticalLayoutGroup>();
            }

            if (!verticalGroupToSpawnPlanksUnder)
            {
                verticalGroupToSpawnPlanksUnder = GetComponentInChildren<VerticalLayoutGroup>();
            }
        }

        public void UpdateUI_OnGameGridModalInitOrRemove()
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

        public void SetAllPlankUIsCanvasGroupData(bool blockRaycast, bool interactable, float enableAlpha = 1.0f, float disableAlpha = 0.0f)
        {
            if(plankUISpawned == null || plankUISpawned.Count == 0) return; 

            for(int i = 0; i < plankUISpawned.Count; i++)
            {
                if (!plankUISpawned[i]) continue;

                plankUISpawned[i].SetPlankUICanvasGroupData(blockRaycast, interactable, enableAlpha, disableAlpha);
            }
        }

        private void SpawnNewGridUILayoutFollowingGridLinkedLayout()
        {
            if (!wordPlankUIPrefabToSpawn)
            {
                Debug.LogError("Trying to spawn word plank UI children for game grid layout: " + name + 
                               " but no word plank UI prefab ref is assigned! Cannot spawn word plank UI children.");

                return;
            }

            if(plankUISpawned == null) plankUISpawned = new List<WordPlankRowUI>();

            else plankUISpawned.Clear();

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

                GameObject rowUIObj;

                if(verticalGroupToSpawnPlanksUnder)
                    rowUIObj = Instantiate(wordPlankUIPrefabToSpawn.gameObject, verticalGroupToSpawnPlanksUnder.transform);

                else rowUIObj = Instantiate(wordPlankUIPrefabToSpawn.gameObject, transform);

                rowUIObj.name = rowUIObj.name + "_" + i;

                WordPlankRowUI rowUI = rowUIObj.GetComponent<WordPlankRowUI>();

                if (rowUI)
                {
                    rowUI.InitGameElementUI(rowModal, this);

                    plankUISpawned.Add(rowUI);
                }
            }
        }

        private void RemoveCurrentGridUILayout()
        {
            if (plankUISpawned == null || plankUISpawned.Count == 0) return;

            for (int i = 0; i < plankUISpawned.Count; i++)
            {
                if (!plankUISpawned[i]) continue;

                if (Application.isEditor) DestroyImmediate(plankUISpawned[i].gameObject);

                else if(Application.isPlaying) Destroy(plankUISpawned[i].gameObject);
            }
        }
    }
}
