using System.Collections;
using UnityEngine;

namespace CrossClimbLite
{
    public class GameStartState : GameStateBase
    {
        [Header("Game Start State Runtime Data")]

        [SerializeField]
        [ReadOnlyInspector]
        private bool hasFinishedGameStartLoad = false;

        public override bool OnStateEnter()
        {
            if (!base.OnStateEnter()) return false;

            hasFinishedGameStartLoad = false;

            if (!presetGameGridInScene)
            {
                Debug.LogError("Game Grid doesnt exist! Game will not start!");

                if (gameStateManagerParent) gameStateManagerParent.TransitionToGameState(null);

                return false;
            }

            StartCoroutine(GameStartLoadProcess());

            return true;
        }

        public override bool OnStateUpdate()
        {
            if(!base.OnStateUpdate()) return false;

            if (hasFinishedGameStartLoad)
            {
                if (gameStateManagerParent) 
                    gameStateManagerParent.TransitionToGameState(nextState);
            }

            return true;
        }

        public override bool OnStateExit()
        {
            if (!base.OnStateExit()) return false;

            //This "if" block HAS TO go above the "presetGameGridInScene.SetActiveFirstCharSlotOfFirstNonKeywordRow();" below
            //in order for the first selected plank's first char slot's caret to show up on start...
            if (GameManager.GameManagerInstance)
            {
                if (GameManager.GameManagerInstance.gameUICanvas)
                    GameManager.GameManagerInstance.gameUICanvas.DisableGameUICanvas(false);
            }

            if (presetGameGridInScene && presetGameGridInScene.hasGridGenerated)
            {
                presetGameGridInScene.SetGameElementLockedStatus(false, true);

                presetGameGridInScene.SetActiveFirstCharSlotOfFirstNonKeywordRow();
            }

            if (GameStartLoadUI.gameStartLoadUIInstance)
            {
                GameStartLoadUI.gameStartLoadUIInstance.HideUIPanel();
            }

            return true;
        }

        private IEnumerator GameStartLoadProcess()
        {
            if (GameStartLoadUI.gameStartLoadUIInstance)
            {
                GameStartLoadUI.gameStartLoadUIInstance.DisplayUIPanel();
            }

            if (GameManager.GameManagerInstance)
            {
                if (GameManager.GameManagerInstance.gameUICanvas)
                    GameManager.GameManagerInstance.gameUICanvas.DisableGameUICanvas(true);
            }

            yield return presetGameGridInScene.InitGridOnGameStartLoad();

            if(presetGameGridInScene && presetGameGridInScene.hasGridGenerated)
                presetGameGridInScene.SetGameElementLockedStatus(true, true);

            yield return GameAnswerConfig.gameAnswerConfigInstance.GenerateNewAnswerConfig(presetGameGridInScene);

            if(presetGameGridInScene) 
                presetGameGridInScene.SetPlanksWordsBasedOnWordSet(GameAnswerConfig.gameAnswerConfigInstance.answerWordSet);

            yield return new WaitForSecondsRealtime(3.0f);

            yield return new WaitForEndOfFrame();

            hasFinishedGameStartLoad = true;
        }
    }
}
