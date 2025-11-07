using UnityEngine;
using UnityEngine.Events;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameStartLoadUI : GameMenuUIBase
    {
        public static GameStartLoadUI gameStartLoadUIInstance;

        protected override void Awake()
        {
            if (gameStartLoadUIInstance)
            {
                gameObject.SetActive(false);

                enabled = false;

                Destroy(gameObject);

                return;
            }

            gameStartLoadUIInstance = this;

            base.Awake();
        }

        public override void DisplayUIPanel()
        {
            if (!enabled) return;

            if (UIFadeComponent && UIFadeComponent.IsTweenRunning())
                UIFadeComponent.StopAndResetUITweenImmediate();

            if (canvasGroup)
            {
                canvasGroup.alpha = 1.0f;

                canvasGroup.blocksRaycasts = true;
            }

            OnPanelUIDisplayed?.Invoke();
        }
    }
}
