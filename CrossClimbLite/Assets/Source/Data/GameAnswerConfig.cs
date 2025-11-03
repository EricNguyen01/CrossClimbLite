using UnityEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CrossClimbLite
{
    public class GameAnswerConfig : MonoBehaviour
    {
        [SerializeField, NotNull, DisallowNull]
        private WordSetDataSO wordSetDataToUse;

        private List<WordSetDataSO.SingleWordChainWrapper> allWordSets = new List<WordSetDataSO.SingleWordChainWrapper>();

        private string[] wordPlankAnswersInOrder;

        private event Action OnGeneratingAnswerConfigStarted;

        private event Action OnGeneratingAnswerConfigFinished;

        private static GameAnswerConfig gameAnswerConfigInstance;

        private void Awake()
        {
            if (gameAnswerConfigInstance && gameAnswerConfigInstance != this)
            {
                gameObject.SetActive(false);

                enabled = false;

                Destroy(gameObject);

                return;
            }

            gameAnswerConfigInstance = this;

            if (!wordSetDataToUse)
            {
                Debug.LogError($"WordSetDataToUse is missing on {name} game object. " +
                                                    "A WordSetData SO is required to provide the answer words and hints data!");

                enabled = false;

                return;
            }

            allWordSets = wordSetDataToUse.GetAllWordChains();
        }

        private void SetWordPlankAnswersInOrder(string[] wordPlankAnswersInOrder)
        {
            if (wordPlankAnswersInOrder == null || wordPlankAnswersInOrder.Length == 0) return;

            wordPlankAnswersInOrder = null;

            this.wordPlankAnswersInOrder = wordPlankAnswersInOrder;
        }

        public void GenerateNewAnswerConfig(GameGrid gameGridInUse)
        {
            if (!enabled || !wordSetDataToUse) return;

            if (!gameGridInUse)
            {
                Debug.LogError("Couldn't generate new answer config. Invalid game grid in use provided!");

                return;
            }

            if (allWordSets == null || allWordSets.Count == 0)
            {
                Debug.LogError("Couldn't generate a new answer config. Either a word set data SO is not assigned or the word sets data in the assigned word set data SO was not properly set!");

                return;
            }

            OnGeneratingAnswerConfigStarted?.Invoke();

            int wordNumRequired = gameGridInUse.rowNum;

            int wordLengthRequired = gameGridInUse.columnNum;

            OnGeneratingAnswerConfigFinished?.Invoke();
        }

        public bool IsWordPlankAnswersMatched(GameGrid gridWithPlanksToCompare, bool shouldCheckKeyword = true)
        {
            if (!gridWithPlanksToCompare) return false;

            if (gridWithPlanksToCompare.wordPlankRowsInGrid == null || gridWithPlanksToCompare.wordPlankRowsInGrid.Length == 0) return false;

            if(wordPlankAnswersInOrder == null || wordPlankAnswersInOrder.Length == 0) return false;

            bool isMatched = false;

            for(int i = 0; i < wordPlankAnswersInOrder.Length; i++)
            {
                if(i >= gridWithPlanksToCompare.wordPlankRowsInGrid.Length) break;

                if (!gridWithPlanksToCompare.wordPlankRowsInGrid[i]) break;

                if(!shouldCheckKeyword && gridWithPlanksToCompare.wordPlankRowsInGrid[i].isPlankKeyword) continue;

                if (wordPlankAnswersInOrder[i] != gridWithPlanksToCompare.wordPlankRowsInGrid[i].GetPlankTypedWord()) break;

                if(i != gridWithPlanksToCompare.wordPlankRowsInGrid[i].plankRowOrder) break;

                if (i == wordPlankAnswersInOrder.Length - 1 && wordPlankAnswersInOrder[i] == gridWithPlanksToCompare.wordPlankRowsInGrid[i].GetPlankTypedWord())
                {
                    isMatched = true;
                }
            }

            return isMatched;
        }
    }
}
