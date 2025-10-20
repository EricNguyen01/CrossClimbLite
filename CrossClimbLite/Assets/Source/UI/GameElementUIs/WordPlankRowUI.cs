using UnityEngine;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class WordPlankRowUI : GameElementUIBase
    {
        [Header("Plank State Colors")]
        [SerializeField]
        private Color normalStateColor = Color.white;

        [SerializeField]
        private Color lockedColor = Color.gray;

        [SerializeField]
        private Color selectedColor = Color.magenta;

        [SerializeField]
        private Color keywordColor = Color.yellow;
        
        [Header("Plank Visual")]

        [SerializeField]
        private Image wordPlankRowBackgroundImage;

        [Header("Drag Handles Visuals")]

        [SerializeField]
        private Image leftDragHandleImage;

        [SerializeField]
        private Image rightDragHandleImage;

        [field: Header("Letter Slots Horizontal Layout Group")]

        [field: SerializeField]
        public HorizontalLayoutGroup horizontalLayoutToSpawnLetterSlotsUnder { get; private set; }

        //the non-UI word plank row Modal (where plank row logic is stored)
        private WordPlankRow wordPlankRowLinked;

        private bool isKeyword = false;

        private bool isDragable = false;

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

            if (wordPlankRowLinked.isPlankLocked) UpdateUI_OnGameElementModalLocked(true);
        }

        public void UpdateUI_PlankModalIsKeyword(bool isKeyword)
        {
            this.isKeyword = isKeyword;

            if (!wordPlankRowBackgroundImage) return;

            if (isLocked)
            {
                wordPlankRowBackgroundImage.color = lockedColor;

                return;
            }

            if (isKeyword)
            {
                wordPlankRowBackgroundImage.color = keywordColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }

        public override void UpdateUI_OnGameElementModalSelected(bool isSelected)
        {
            base.UpdateUI_OnGameElementModalSelected(isSelected);

            if (!enabled) return;

            if (!wordPlankRowBackgroundImage) return;

            if (wordPlankRowLinked && wordPlankRowLinked.isPlankKeyword)
            {
                if(wordPlankRowBackgroundImage.color != keywordColor)
                    wordPlankRowBackgroundImage.color = keywordColor;

                return;
            }

            if (isSelected)
            {
                wordPlankRowBackgroundImage.color = selectedColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }

        public override void UpdateUI_OnGameElementModalLocked(bool isLocked)
        {
            base.UpdateUI_OnGameElementModalLocked(isLocked);

            isDragable = isLocked;

            if (!enabled) return;

            Color dragHandleColor;

            if (isLocked)
            {
                if (wordPlankRowBackgroundImage)
                    wordPlankRowBackgroundImage.color = lockedColor;

                if (rightDragHandleImage)
                {
                    dragHandleColor = rightDragHandleImage.color;

                    dragHandleColor.a = 0.0f;

                    rightDragHandleImage.color = dragHandleColor;
                }

                if (leftDragHandleImage)
                {
                    dragHandleColor = leftDragHandleImage.color;

                    dragHandleColor.a = 0.0f;

                    leftDragHandleImage.color = dragHandleColor;
                }

                return;
            }

            if (wordPlankRowBackgroundImage)
            {
                if (wordPlankRowLinked.isPlankKeyword)
                    wordPlankRowBackgroundImage.color = keywordColor;

                else
                    wordPlankRowBackgroundImage.color = normalStateColor;
            }

            if (rightDragHandleImage)
            {
                dragHandleColor = rightDragHandleImage.color;

                dragHandleColor.a = 255.0f;

                rightDragHandleImage.color = dragHandleColor;
            }

            if (leftDragHandleImage)
            {
                dragHandleColor = leftDragHandleImage.color;

                dragHandleColor.a = 255.0f;

                leftDragHandleImage.color = dragHandleColor;
            }
        }
    }
}
