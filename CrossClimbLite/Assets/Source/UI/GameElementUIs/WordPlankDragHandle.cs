using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrossClimbLite
{
    public class WordPlankDragHandleUI : GameElementUIBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private WordPlankRowUI parentWordPlank;

        public void InitDragHandleUI(WordPlankRowUI wordPlankToLink)
        {
            if (!wordPlankToLink) return;

            parentWordPlank = wordPlankToLink;
        }

        public override void UpdateUI_OnGameElementModalLocked(bool isLocked)
        {
            base.UpdateUI_OnGameElementModalLocked(isLocked);

            if (!elementCanvasGroup) return;

            if (isLocked)
            {
                elementCanvasGroup.alpha = 0.0f;

                return;
            }

            elementCanvasGroup.alpha = 1.0f;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
        }
    }
}
