using UnityEngine;
using UnityEngine.EventSystems;

namespace CrossClimbLite
{
    public class WordPlankDragHandleUI : GameElementUIBase, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public WordPlankRowUI parentWordPlankUI { get; private set; }

        public void InitDragHandleUI(WordPlankRowUI wordPlankToLink)
        {
            if (!wordPlankToLink) return;

            parentWordPlankUI = wordPlankToLink;
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
            if(parentWordPlankUI) parentWordPlankUI.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(parentWordPlankUI) parentWordPlankUI.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if(parentWordPlankUI) parentWordPlankUI.OnEndDrag(eventData);
        }
    }
}
