using System.Collections;
using UnityEngine;

namespace CrossClimbLite
{
    public class GameUpdateState : GameStateBase
    {
        [field: Header("Game Update State Runtime Data")]

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool allNonKeywordsCorrectNeedsReorder { get; private set; } = false;

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool allNonKeywordsCorrectAndOrdered { get; private set; } = false;

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasKeywordsUnlocked { get; private set; } = false;

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasWonGame { get; private set; } = false;

        private float timeAtUpdateCalled = 0.0f;

        public override bool OnStateEnter()
        {
            GameManager.ResetRuntimePlayerStats();

            if (!base.OnStateEnter()) return false;

            allNonKeywordsCorrectNeedsReorder = false;

            allNonKeywordsCorrectAndOrdered = false;

            hasKeywordsUnlocked = false;

            hasWonGame = false;

            if (presetGameGridInScene)
            {
                presetGameGridInScene.OnAWordPlankFilled += (string s) => OnPlankWordFilled();
            }

            //reset time at update called to the time at beginning of the first update call (end of state start func)
            timeAtUpdateCalled = Time.time;

            return true;
        }

        public override bool OnStateUpdate()
        {
            if (!base.OnStateUpdate()) return false;

            GameManager.timeTakenThisRound += (Time.time - timeAtUpdateCalled) * Time.timeScale;

            //Debug.Log($"TimeTaken: {GameManager.timeTakenThisRound} | Timescale: {Time.timeScale}");

            timeAtUpdateCalled = Time.time;

            return true;
        }

        public override bool OnStateExit()
        {
            if (!base.OnStateExit()) return false;

            if (presetGameGridInScene)
            {
                presetGameGridInScene.OnAWordPlankFilled -= (string s) => OnPlankWordFilled();
            }

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

            if (!GameAnswerConfig.gameAnswerConfigInstance.GetWordSetDataSOInUse())
            {
                Debug.LogError("Trying to check if typed word matches the answer, " +
                               "but there's no word set data SO ref assigned in GameAnswerConfig static object in scene!");

                return;
            }

            bool allNonKeywordsMatchedButNotOrdered;

            bool answersMatched = GameAnswerConfig.gameAnswerConfigInstance.CheckWordPlankAnswersMatched(presetGameGridInScene, out allNonKeywordsMatchedButNotOrdered, hasKeywordsUnlocked);

            if (!hasKeywordsUnlocked)
            {
                if (allNonKeywordsMatchedButNotOrdered)//this if processes keywords matched but not ordered yet
                {
                    allNonKeywordsCorrectNeedsReorder = true;

                    if (HintBoxUI.hintBoxUIInstance)
                    {
                        HintBoxUI.hintBoxUIInstance.EnablePlankReOrderNeededMessage(true);
                    }
                }
                else
                {
                    allNonKeywordsCorrectNeedsReorder = false;

                    if (HintBoxUI.hintBoxUIInstance)
                    {
                        HintBoxUI.hintBoxUIInstance.EnablePlankReOrderNeededMessage(false);
                    }
                }

                if (answersMatched)//answersMatched == all keywords matched + ordered
                {
                    hasKeywordsUnlocked = true;

                    allNonKeywordsCorrectNeedsReorder = false;

                    allNonKeywordsCorrectAndOrdered = true;

                    if (presetGameGridInScene)
                    {
                        presetGameGridInScene.UnlockNonKeywordPlanksInGrid(false);

                        presetGameGridInScene.UnlockKeywordPlanksInGrid();
                    }

                    if (HintBoxUI.hintBoxUIInstance)
                    {
                        HintBoxUI.hintBoxUIInstance.EnablePlankReOrderNeededMessage(false);
                    }

                    return;
                }
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
