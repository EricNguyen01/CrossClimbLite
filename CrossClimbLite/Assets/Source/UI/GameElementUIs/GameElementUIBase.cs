using UnityEngine;
using UnityEngine.EventSystems;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameElementUIBase : MonoBehaviour
    {
        [SerializeField]
        [ReadOnlyInspector]
        protected bool isSelected = false;

        [SerializeField]
        [ReadOnlyInspector]
        protected bool isLocked = false;

        protected CanvasGroup elementCanvasGroup;

        protected EventSystem eventSystem;

        private GameElementBase gameElementLinked;

        protected virtual void Awake()
        {
            if(!TryGetComponent<CanvasGroup>(out elementCanvasGroup))
            {
                elementCanvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            eventSystem = EventSystem.current;

            if (!eventSystem)
            {
                Debug.LogWarning("GameElementUI: " + name + " couldnt find any EventSystem in the scene. It might not work!");
            }
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
