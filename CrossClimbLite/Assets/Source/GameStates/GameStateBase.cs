using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameStateBase : MonoBehaviour
    {
        private GameStateManager gameStateManagerParent;

        public virtual void InitializeState(GameStateManager gameStateManagerHoldingState)
        {
            if (!gameStateManagerHoldingState)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            gameStateManagerParent = gameStateManagerHoldingState;
        }

        public abstract void OnStateEnter();

        public virtual void OnStateUpdate() { }

        public abstract void OnStateExit();
    }
}
