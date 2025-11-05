using UnityEngine;
using UnityEngine.Events;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameStartLoadUI : MonoBehaviour
    {
        [SerializeField]
        private UIFade UIFadeComponent;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private UnityEvent OnLoadingPanelUIDisplayed;

        [SerializeField]
        private UnityEvent OnLoadingPanelUIHidden;

        public static GameStartLoadUI gameStartLoadUIInstance;

        private void Awake()
        {
            if (gameStartLoadUIInstance)
            {
                gameObject.SetActive(false);

                enabled = false;

                Destroy(gameObject);

                return;
            }

            gameStartLoadUIInstance = this;

            if (!canvasGroup)
            {
                if(!TryGetComponent<CanvasGroup>(out canvasGroup))
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

        public void OnGameStartsLoading()
        {
            if (!enabled) return;

            if (canvasGroup) canvasGroup.blocksRaycasts = true;

            if (UIFadeComponent)
            {
                UIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeIn);

                UIFadeComponent.isLooped = false;

                UIFadeComponent.RunTweenInternal();
            }

            OnLoadingPanelUIDisplayed?.Invoke();
        }

        public void OnGameStopLoading()
        {
            if (!enabled) return;

            if (canvasGroup) canvasGroup.blocksRaycasts = false;

            if (UIFadeComponent)
            {
                UIFadeComponent.SetFadeMode(UIFade.UIFadeMode.FadeOut);

                UIFadeComponent.isLooped = false;

                UIFadeComponent.RunTweenInternal();
            }

            OnLoadingPanelUIHidden?.Invoke();
        }
    }
}
