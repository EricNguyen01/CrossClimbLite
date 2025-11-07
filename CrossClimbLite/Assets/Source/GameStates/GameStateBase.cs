using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameStateBase : MonoBehaviour
    {
        [Header("Required Game Prefabs")]

        [SerializeField]
        protected GameGrid gameGridPrefab;

        [SerializeField]
        protected GameAnswerConfig gameAnswerConfigPrefab;

        [Header("Game Scene Preset Components")]

        [SerializeField]
        protected GameGrid presetGameGridInScene;

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

        protected GameStateManager gameStateManagerParent;

        protected virtual void OnEnable()
        {
            if (!presetGameGridInScene)
            {
                presetGameGridInScene = FindAnyObjectByType<GameGrid>();

                if (!presetGameGridInScene && gameGridPrefab)
                {
                    GameObject gameGridObj = Instantiate(gameGridPrefab.gameObject, Vector3.zero, Quaternion.identity);

                    GameGrid gameGridComp = gameGridObj.GetComponent<GameGrid>();

                    if (gameGridComp) presetGameGridInScene = gameGridComp;
                    else presetGameGridInScene = gameGridObj.AddComponent<GameGrid>();
                }
            }

            if (!presetGameGridInScene)
            {
                Debug.LogError("Game Grid doesnt exist! Game will not start!");

                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            if (GameAnswerConfig.gameAnswerConfigInstance == null && !FindAnyObjectByType<GameAnswerConfig>())
            {
                if (gameAnswerConfigPrefab)
                {
                    Instantiate(gameAnswerConfigPrefab.gameObject, Vector3.zero, Quaternion.identity);
                }
                else
                {
                    Debug.LogError("Game Answer Config and game answer word set data could not be found and its prefab is not provided for instantiation. Game won't start!");

                    gameObject.SetActive(false);

                    enabled = false;

                    return;
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
            if (!enabled || !gameStateManagerParent) return false;

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
