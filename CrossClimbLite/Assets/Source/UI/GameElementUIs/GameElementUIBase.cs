using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameElementUIBase : MonoBehaviour
    {
        [SerializeField]
        [ReadOnlyInspector]
        private bool isSelected = false;

        [SerializeField]
        [ReadOnlyInspector]
        private bool isLocked = false;

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

            gameElementLinked.OnElementUpdated += OnGameElementUpdated;

            gameElementLinked.OnElementSelected += OnGameElementSelected;

            gameElementLinked.OnElementLocked += OnGameElementLocked;
        }

        protected virtual void OnDisable()
        {
            if (gameElementLinked)
            {
                gameElementLinked.OnElementUpdated -= OnGameElementUpdated;

                gameElementLinked.OnElementSelected -= OnGameElementSelected;

                gameElementLinked.OnElementLocked -= OnGameElementLocked;
            }
        }

        protected abstract void OnGameElementUpdated();

        protected virtual void OnGameElementSelected(bool isSelected)
        {
            if (!enabled) return;

            this.isSelected = isSelected;
        }

        protected virtual void OnGameElementLocked(bool isLocked)
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
