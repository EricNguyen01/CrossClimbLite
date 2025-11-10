using UnityEngine;

namespace CrossClimbLite
{
    public class GameEndUI : GameMenuUIBase
    {
        public static GameEndUI gameEndUIInstance;

        protected override void Awake()
        {
            if (gameEndUIInstance)
            {
                gameObject.SetActive(false);

                enabled = false;

                Destroy(gameObject);

                return;
            }

            gameEndUIInstance = this;

            base.Awake();
        }

        public override void DisplayUIPanel()
        {
            base.DisplayUIPanel();

            if (TimerUI.timerUIInstance) TimerUI.timerUIInstance.StopTimer(true);
        }

        public override void HideUIPanel()
        {
            base.HideUIPanel();

            if (TimerUI.timerUIInstance) TimerUI.timerUIInstance.StopTimer(false);
        }
    }
}
