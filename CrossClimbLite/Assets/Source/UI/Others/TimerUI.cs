using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RoundStatsTextUI))]
    public class TimerUI : MonoBehaviour
    {
        [SerializeField]
        private RoundStatsTextUI timerTimeTextUI;

        private bool isStopped = false;

        private float currentTime = 0.0f;

        public static TimerUI timerUIInstance;

        private void Awake()
        {
            if(timerUIInstance && this != timerUIInstance)
            {
                Destroy(gameObject);

                return;
            }

            timerUIInstance = this;

            if (!timerTimeTextUI)
            {
                if(!TryGetComponent<RoundStatsTextUI>(out timerTimeTextUI))
                {
                    timerTimeTextUI = gameObject.AddComponent<RoundStatsTextUI>();
                }
            }
        }

        private void Update()
        {
            if (!enabled) return;

            if (isStopped) return;

            if (GameManager.GameManagerInstance)
            {
                //if GameManager intance exists, use GameManager's timeTakenThisRound value 
                timerTimeTextUI.UpdateTimeTakenTextCustom(null, GameManager.timeTakenThisRound);

                return;
            }

            //else use delta time value
            timerTimeTextUI.UpdateTimeTakenTextCustom(null, currentTime += Time.deltaTime);
        }

        public void StopTimer(bool isStopped)
        {
            this.isStopped = isStopped;
        }

        public void ResetTimer()
        {
            //reset timer for delta time value
            currentTime = 0.0f;
        }
    }
}
