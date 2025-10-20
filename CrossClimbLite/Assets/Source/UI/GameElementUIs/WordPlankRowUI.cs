using UnityEngine;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class WordPlankRowUI : GameElementUIBase
    {
        [SerializeField]
        private Color normalStateColor = Color.white;

        [SerializeField]
        private Color lockedColor = Color.yellow;

        [SerializeField]
        private Color selectedColor = Color.magenta;

        [field: SerializeField]
        public HorizontalLayoutGroup horizontalLayoutToSpawnLetterSlotsUnder { get; private set; }

        [SerializeField]
        private Image wordPlankRowBackgroundImage;

        //the non-UI word plank row model (where plank row logic is stored)
        private WordPlankRow wordPlankRowLinked;

        protected override void Awake()
        {
            base.Awake();

            if (!wordPlankRowBackgroundImage)
            {
                TryGetComponent<Image>(out wordPlankRowBackgroundImage);
            }

            if(!horizontalLayoutToSpawnLetterSlotsUnder)
            {
                horizontalLayoutToSpawnLetterSlotsUnder = GetComponent<HorizontalLayoutGroup>();

                if (!horizontalLayoutToSpawnLetterSlotsUnder)
                {
                    horizontalLayoutToSpawnLetterSlotsUnder = GetComponentInChildren<HorizontalLayoutGroup>();
                }
            }
        }

        public override void InitGameElementUI(GameElementBase wordPlankRowToLink)
        {
            base.InitGameElementUI(wordPlankRowToLink);

            if (!wordPlankRowToLink) return;

            if (wordPlankRowToLink is not WordPlankRow) return;

            wordPlankRowLinked = wordPlankRowToLink as WordPlankRow;

            if (wordPlankRowLinked.isPlankLocked) UpdateUI_OnGameElementModelLocked(true);
        }

        public override void UpdateUI_OnGameElementModelSelected(bool isSelected)
        {
            base.UpdateUI_OnGameElementModelSelected(isSelected);

            if (!enabled) return;

            if (!wordPlankRowBackgroundImage) return;

            if (isSelected)
            {
                wordPlankRowBackgroundImage.color = selectedColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }

        public override void UpdateUI_OnGameElementModelLocked(bool isLocked)
        {
            base.UpdateUI_OnGameElementModelLocked(isLocked);

            if (!enabled) return;

            if (!wordPlankRowBackgroundImage) return;

            if (isLocked)
            {
                wordPlankRowBackgroundImage.color = lockedColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }
    }
}
