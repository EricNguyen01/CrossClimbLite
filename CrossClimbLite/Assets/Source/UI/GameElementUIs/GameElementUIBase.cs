using UnityEngine;
using UnityEngine.EventSystems;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameElementUIBase : MonoBehaviour
    {
        protected CanvasGroup elementCanvasGroup;

        protected EventSystem eventSystem;

        private GameElementBase gameElementLinked;

        public virtual void Awake()
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

        public virtual void InitGameElementUI<T>(T gameElementToLink) where T : GameElementBase
        {
            if (!gameElementLinked)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            gameElementLinked = gameElementToLink;

            gameElementLinked.OnElementSelected += OnGameElementSelected;

            gameElementLinked.OnElementLocked += OnGameElementLocked;
        }

        public virtual void OnDisable()
        {
            if (gameElementLinked)
            {
                gameElementLinked.OnElementSelected -= OnGameElementSelected;

                gameElementLinked.OnElementLocked -= OnGameElementLocked;
            }
        }

        public abstract void OnGameElementUpdated();

        public abstract void OnGameElementSelected(bool isSelected);

        public virtual void OnGameElementLocked(bool isLocked)
        {
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
