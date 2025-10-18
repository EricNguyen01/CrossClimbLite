using UnityEngine;
using System.Collections.Generic;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameGridUI : GameElementUIBase
    {
        [SerializeField]
        private WordPlankRowUI wordPlankUIPrefabToSpawn;

        private List<WordPlankRowUI> plankUISpawned = new List<WordPlankRowUI>();   

        [SerializeField]
        private PlankLetterSlotUI letterSlotUIPrefabToSpawn;

        private List<PlankLetterSlotUI> letterSlotUISpawned = new List<PlankLetterSlotUI>();

        private GameGrid gameGridLinked;

        protected override void InitGameElementUI<T>(T gameGridToLink)
        {
            base.InitGameElementUI(gameGridToLink);

            if (!gameGridToLink) return;

            if (gameGridToLink is not GameGrid) return;

            gameGridLinked = gameGridToLink as GameGrid;
        }

        public void InitGridUIGameElements()
        {

        }

        public void RemoveGridUIGameElements()
        {

        }

        protected override void OnGameElementSelected(bool isSelected)
        {
            
        }

        protected override void OnGameElementUpdated()
        {
            
        }
    }
}
