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

            if (!enabled) return false;

            hasFinishedGameStartLoad = false;

            if (!GameAnswerConfig.gameAnswerConfigInstance || !presetGameGridInScene) return false;

            StartCoroutine(GameStartLoadProcess());

            return true;
        }

        public override bool OnStateUpdate()
        {
            if(!base.OnStateUpdate()) return false;

            if(!enabled) return false;

            if (hasFinishedGameStartLoad)
            {
                if (gameStateManagerParent) 
                    gameStateManagerParent.TransitionToGameState(nextState);
            }

            return true;
        }

        private IEnumerator GameStartLoadProcess()
        {
            if (GameStartLoadUI.gameStartLoadUIInstance)
            {
                GameStartLoadUI.gameStartLoadUIInstance.OnGameStartsLoading();
            }

            yield return presetGameGridInScene.InitGridOnGameStartLoad();

            if(presetGameGridInScene.hasGridGenerated)
                presetGameGridInScene.SetGameElementLockedStatus(true, true);

            yield return GameAnswerConfig.gameAnswerConfigInstance.GenerateNewAnswerConfig(presetGameGridInScene);

            yield return new WaitForSecondsRealtime(3.4f);

            yield return new WaitForEndOfFrame();

            if (presetGameGridInScene.hasGridGenerated)
            {
                presetGameGridInScene.SetPlanksBasedOnWordSet(GameAnswerConfig.gameAnswerConfigInstance.answerWordSet);

                presetGameGridInScene.SetGameElementLockedStatus(false, true);
            }

            if (GameStartLoadUI.gameStartLoadUIInstance)
            {
                GameStartLoadUI.gameStartLoadUIInstance.OnGameStopLoading();
            }

            hasFinishedGameStartLoad = true;
        }
    }
}
