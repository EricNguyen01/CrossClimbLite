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
        }

        public override void InitGameElementUI(GameElementBase wordPlankRowToLink)
        {
            base.InitGameElementUI(wordPlankRowToLink);

            if (!wordPlankRowToLink) return;

            if (wordPlankRowToLink is not WordPlankRow) return;

            wordPlankRowLinked = wordPlankRowToLink as WordPlankRow;

            if (wordPlankRowLinked.isPlankLocked) OnGameElementLocked(true);
        }

        protected override void OnGameElementUpdated()
        {
            
        }

        protected override void OnGameElementSelected(bool isSelected)
        {
            base.OnGameElementSelected(isSelected);

            if (!enabled) return;

            if (!wordPlankRowBackgroundImage) return;

            if (isSelected)
            {
                wordPlankRowBackgroundImage.color = selectedColor;

                return;
            }

            wordPlankRowBackgroundImage.color = normalStateColor;
        }

        protected override void OnGameElementLocked(bool isLocked)
        {
            base.OnGameElementLocked(isLocked);

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
