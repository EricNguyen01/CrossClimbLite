using UnityEngine;
using UnityEngine.Events;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameMenuUIBase : MonoBehaviour
    {
        [Header("Game Menu UI Components")]

        [SerializeField]
        protected UIFade UIFadeComponent;

        [SerializeField]
        protected CanvasGroup canvasGroup;

        [SerializeField]
        protected UnityEvent OnPanelUIDisplayed;

        [SerializeField]
        protected UnityEvent OnPanelUIHidden;

        [Header("Game Menu UI Settings")]

        [SerializeField]
        [Min(0.1f)]
        protected float displayFadeInDuration = 1.0f;

        [SerializeField]
        [Min(0.1f)]
        protected float hideFadeOutDuration = 1.5f;

        protected virtual void Awake()
        {
            if (!canvasGroup)
            {
                if (!TryGetComponent<CanvasGroup>(out canvasGroup))
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            canvasGroup.interactable = false;

            canvasGroup.blocksRaycasts = false;

            canvasGroup.alpha = 0.0f;

            if (!UIFadeComponent)
            {
                if (!TryGetComponent<UIFade>(out UIFadeComponent))
                {
                    UIFadeComponent = gameObject.AddComponent<UIFade>();
                }
            }

            UIFadeComponent.SetTweenExecuteMode(UITweenBase.UITweenExecuteMode.Internal);

            UIFadeComponent.SetUITweenCanvasGroup(canvasGroup);
        }

        public virtual void DisplayUIPanel()
        {
            if (!UIFadeComponent) return;

            if (UIFadeComponent.IsTweenRunning())
                UIFadeComponent.StopAndResetUITweenImmediate();

            UIFadeComponent.SetTweenDuration(displayFadeInDuration);

            UIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeIn);

            UIFadeComponent.isLooped = false;

            UIFadeComponent.RunTweenInternal();

            canvasGroup.interactable = true;

            canvasGroup.blocksRaycasts = true;

            OnPanelUIDisplayed?.Invoke();
        }

        public virtual void HideUIPanel()
        {
            if (!UIFadeComponent) return;

            if (UIFadeComponent.IsTweenRunning())
                UIFadeComponent.StopAndResetUITweenImmediate();

            UIFadeComponent.SetTweenDuration(hideFadeOutDuration);

            UIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeOut);

            UIFadeComponent.isLooped = false;

            UIFadeComponent.RunTweenInternal();

            canvasGroup.interactable = false;

            canvasGroup.blocksRaycasts = false;
            
            OnPanelUIHidden?.Invoke();
        }
    }
}
