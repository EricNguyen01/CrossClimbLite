using UnityEngine;

namespace CrossClimbLite
{
    public class GameUpdateState : GameStateBase
    {
        [field: Header("Runtime Data")]

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasKeywordsUnlocked { get; private set; } = false;

        public override bool OnStateEnter()
        {
            if (!base.OnStateEnter()) return false;

            if (!enabled) return false;

            hasKeywordsUnlocked = false;

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

                if (presetGameGridInScene) presetGameGridInScene.UnlockKeywordPlanksInGrid();

                return;
            }

            //Transition to game end (won) state here...
        }
    }
}
