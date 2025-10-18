using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameElementUIBase : MonoBehaviour
    {
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

        protected virtual void InitGameElementUI<T>(T gameElementToLink) where T : GameElementBase
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

        protected virtual void OnDisable()
        {
            if (gameElementLinked)
            {
                gameElementLinked.OnElementSelected -= OnGameElementSelected;

                gameElementLinked.OnElementLocked -= OnGameElementLocked;
            }
        }

        protected abstract void OnGameElementUpdated();

        protected abstract void OnGameElementSelected(bool isSelected);

        protected virtual void OnGameElementLocked(bool isLocked)
        {
            if (!enabled) return;

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
