using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CrossClimbLite
{
    public class GameAnswerConfig : MonoBehaviour
    {
        [SerializeField, NotNull, DisallowNull]
        private WordSetDataSO wordSetDataToUse;

        private string[] wordPlankAnswersInOrder;

        public List<WordSetDataSO.WordHintStruct> randomAnswerWordSet { get; private set; } = new List<WordSetDataSO.WordHintStruct>();

        private event Action OnGeneratingAnswerConfigStarted;

        private event Action<bool> OnGeneratingAnswerConfigFinishedSuccessful;

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
        }

        public IEnumerator GenerateNewAnswerConfig(GameGrid gameGridInUse)
        {
            if (!enabled) yield break;

            if (!wordSetDataToUse)
            {
                Debug.LogError("Couldn't generate new answer config. Invalid word sets data SO provided");

                yield break;
            }

            if (!gameGridInUse)
            {
                Debug.LogError("Couldn't generate new answer config. Invalid game grid in use provided!");

                yield break;
            }

            OnGeneratingAnswerConfigStarted?.Invoke();

            randomAnswerWordSet.Clear();

            int wordCountRequired = gameGridInUse.rowNum;

            int wordLengthRequired = gameGridInUse.columnNum;

            List<WordSetDataSO.WordHintStruct> randomWordSet = wordSetDataToUse.GetRandomWordSetRuntime(wordLengthRequired, wordCountRequired);

            if (randomWordSet == null || randomWordSet.Count == 0)
            {
                Debug.LogError("Trying to get random word set from word sets data SO, but none could be found. Please check word sets data setup in assigned WordSetDataSO in use.");

                OnGeneratingAnswerConfigFinishedSuccessful?.Invoke(false);

                yield break;
            }

            wordPlankAnswersInOrder = new string[wordCountRequired];

            for(int i = 0; i < wordPlankAnswersInOrder.Length; i++)
            {
                if (i >= randomWordSet.Count) break;

                wordPlankAnswersInOrder[i] = randomWordSet[i].word;

                randomAnswerWordSet.Add(randomWordSet[i]);
            }

            yield return new WaitForSecondsRealtime(0.1f);

            OnGeneratingAnswerConfigFinishedSuccessful?.Invoke(true);
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
