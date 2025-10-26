using UnityEngine;
using UnityEngine.EventSystems;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class GameElementUIBase : MonoBehaviour
    {
        [SerializeField]
        [ReadOnlyInspector]
        protected bool isSelected = false;

        [SerializeField]
        [ReadOnlyInspector]
        protected bool isLocked = false;

        protected RectTransform gameElementUIRect;

        protected CanvasGroup elementCanvasGroup;

        protected EventSystem eventSystem;

        protected Canvas parentRootCanvas;

        protected RectTransform parentRootCanvasRect;

        private GameElementBase gameElementLinked;

        private GameElementUIBase parentHoldingUI;

        protected virtual void Awake()
        {
            TryGetComponent<RectTransform>(out gameElementUIRect);

            if(!TryGetComponent<CanvasGroup>(out elementCanvasGroup))
            {
                elementCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (!EventSystem.current)
            {
                if(Application.isPlaying)
                {
                    new GameObject("EventSystem").AddComponent<EventSystem>();
                }
            }

            eventSystem = EventSystem.current;

            parentRootCanvas = GetComponentInParent<Canvas>();

            parentRootCanvas.TryGetComponent<RectTransform>(out parentRootCanvasRect);

            if (!parentRootCanvas.worldCamera) parentRootCanvas.worldCamera = Camera.main;
        }

        public virtual void InitGameElementUI(GameElementBase gameElementToLink)
        {
            if (!gameElementToLink)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            gameElementLinked = gameElementToLink;

            gameElementLinked.ConnectGameElementUI(this);

            if(!parentHoldingUI) parentHoldingUI = transform.parent.GetComponentInParent<GameElementUIBase>();
        }

        public virtual void InitGameElementUI(GameElementBase gameElementToLink, GameElementUIBase parentHoldingUIToLink)
        {
            InitGameElementUI(gameElementToLink);

            if (!parentHoldingUIToLink)
            {
                parentHoldingUI = transform.parent.GetComponentInParent<GameElementUIBase>();

                return;
            }

            parentHoldingUI = parentHoldingUIToLink;
        }

        public virtual void UpdateUI_OnGameElementModalSelected(bool isSelected)
        {
            if (!enabled) return;

            this.isSelected = isSelected;
        }

        public virtual void UpdateUI_OnGameElementModalLocked(bool isLocked)
        {
            if (!enabled) return;

            this.isLocked = isLocked;

            if (isLocked)
            {
                elementCanvasGroup.blocksRaycasts = false;

                elementCanvasGroup.interactable = false;

                return;
            }

            elementCanvasGroup.blocksRaycasts = true;

            elementCanvasGroup.interactable = true;
        }
    }
}
