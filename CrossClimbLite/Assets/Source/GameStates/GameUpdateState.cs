using System.Collections;
using UnityEngine;

namespace CrossClimbLite
{
    public class GameUpdateState : GameStateBase
    {
        [field: Header("Game Update State Runtime Data")]

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasKeywordsUnlocked { get; private set; } = false;

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasWonGame { get; private set; } = false;

        public override bool OnStateEnter()
        {
            if (!base.OnStateEnter()) return false;

            hasKeywordsUnlocked = false;

            hasWonGame = false;

            if (presetGameGridInScene)
            {
                presetGameGridInScene.OnAWordPlankFilled += (string s) => OnPlankWordFilled();
            }

            return true;
        }

        public override bool OnStateExit()
        {
            if (!base.OnStateExit()) return false;

            if (presetGameGridInScene)
            {
                presetGameGridInScene.OnAWordPlankFilled -= (string s) => OnPlankWordFilled();
            }

            hasKeywordsUnlocked = false;

            hasWonGame = false;

            return true;
        }

        private void OnPlankWordFilled()
        {
            if (!GameAnswerConfig.gameAnswerConfigInstance)
            {
                Debug.LogError("Trying to check if typed word matches the answer, " +
                               "but GameAnswerConfig static object is missing in scene!");

                return;
            }

            bool answersMatched = GameAnswerConfig.gameAnswerConfigInstance.IsWordPlankAnswersMatched(presetGameGridInScene, hasKeywordsUnlocked);
            
            if(!hasKeywordsUnlocked && answersMatched)
            {
                hasKeywordsUnlocked = true;

                if (presetGameGridInScene)
                {
                    presetGameGridInScene.UnlockNonKeywordPlanksInGrid(false);

                    presetGameGridInScene.UnlockKeywordPlanksInGrid();
                }

                return;
            }

            if(answersMatched)
            {
                hasWonGame = true;

                if (presetGameGridInScene)
                {
                    presetGameGridInScene.SetGameElementLockedStatus(true, true);
                }

                gameStateManagerParent.TransitionToStateDelay(nextState, 0.5f);
            }
        }
    }
}
