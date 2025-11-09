using UnityEngine;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class ReplayButton : MonoBehaviour
    {
        private GameStateManager gameMainStatesManager;

        private GameStartState gameStartState;

        private void Start()
        {
            GetGameMainStateManagerAndGameStartStateRefs();
        }

        public void OnReplayButtonPressed()
        {
            if(gameMainStatesManager && gameStartState)
            {
                gameMainStatesManager.TransitionToGameState(gameStartState);

                return;
            }

            if (!GetGameMainStateManagerAndGameStartStateRefs()) return;

            if (gameMainStatesManager && gameStartState)
            {
                gameMainStatesManager.TransitionToGameState(gameStartState);
            }
        }

        private bool GetGameMainStateManagerAndGameStartStateRefs()
        {
            if (GameManager.GameManagerInstance && GameManager.GameManagerInstance.gameMainStatesManager)
            {
                gameMainStatesManager = GameManager.GameManagerInstance.gameMainStatesManager;

                if (gameMainStatesManager.allGameStates != null && gameMainStatesManager.allGameStates.Count > 0)
                {
                    for (int i = 0; i < gameMainStatesManager.allGameStates.Count; i++)
                    {
                        if (!gameMainStatesManager.allGameStates[i]) continue;

                        if (gameMainStatesManager.allGameStates[i].GetType() == typeof(GameStartState))
                        {
                            gameStartState = gameMainStatesManager.allGameStates[i] as GameStartState;

                            return true;
                        }
                    }
                }
            }

            foreach (GameStateManager stateManager in FindObjectsByType<GameStateManager>(FindObjectsSortMode.None))
            {
                GameStartState startState = stateManager.GetComponentInChildren<GameStartState>();

                if (stateManager.isMainStateManager || stateManager.name.ToLower().Contains("gamemainstate"))
                {
                    if (startState)
                    {
                        gameMainStatesManager = stateManager;

                        gameStartState = startState;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
