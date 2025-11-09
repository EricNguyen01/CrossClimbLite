using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameStateBase : MonoBehaviour
    {
        [Header("State Data")]

        [SerializeField]
        protected GameStateBase nextState;

        [Header("State Data Runtime")]

        [SerializeField]
        [ReadOnlyInspector]
        protected bool stateEntered = false;

        [SerializeField]
        [ReadOnlyInspector]
        protected bool stateBeingUpdated = false;

        [SerializeField]
        [ReadOnlyInspector]
        protected bool stateExited = false;

        [SerializeField]
        [ReadOnlyInspector]
        protected GameGrid presetGameGridInScene;

        protected GameStateManager gameStateManagerParent;

        protected virtual void OnEnable()
        {
            if (!presetGameGridInScene)
            {
                if (GameManager.GameManagerInstance)
                {
                    presetGameGridInScene = GameManager.GameManagerInstance.gameGridModal;
                }

                if (!presetGameGridInScene)
                {
                    presetGameGridInScene = FindAnyObjectByType<GameGrid>();
                }
            }
        }

        public virtual void InitializeState(GameStateManager gameStateManagerHoldingState)
        {
            if (!gameStateManagerHoldingState)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            gameStateManagerParent = gameStateManagerHoldingState;

            stateEntered = false; 
            
            stateBeingUpdated = false; 
            
            stateExited = false;
        }

        public virtual bool OnStateEnter()
        {
            if (!gameStateManagerParent) return false;

            if (!enabled)
            {
                if (nextState)
                {
                    gameStateManagerParent.TransitionToGameState(nextState);

                    return false;
                }

                return false;
            }

            if (gameStateManagerParent.currentGameState != this)
            {
                gameStateManagerParent.TransitionToGameState(this);

                return false;
            }

            stateEntered = true;

            stateBeingUpdated = false;

            stateExited = false;

            return true;
        }

        public virtual bool OnStateUpdate()
        {
            if (!gameStateManagerParent) return false;

            if (!enabled)
            {
                if (nextState)
                {
                    gameStateManagerParent.TransitionToGameState(nextState);

                    return false;
                }

                return false;
            }

            if (gameStateManagerParent.currentGameState != this)
            {
                gameStateManagerParent.TransitionToGameState(this);

                return false;
            }

            stateBeingUpdated = true;

            return true;
        }

        public virtual bool OnStateExit()
        {
            if (!gameStateManagerParent) return false;

            if (gameStateManagerParent.currentGameState != this)
            {
                gameStateManagerParent.StopStateUpdateCoroutine();
            }

            stateEntered = false;

            stateBeingUpdated = false;

            stateExited = true;

            return true;
        }
    }
}
