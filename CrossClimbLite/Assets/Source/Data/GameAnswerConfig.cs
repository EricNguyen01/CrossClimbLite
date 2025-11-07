using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CrossClimbLite
{
    public class GameAnswerConfig : MonoBehaviour
    {
        [SerializeField, NotNull, DisallowNull]
        private WordSetDataSO wordSetDataToUse;

        [SerializeField, ReadOnlyInspector]
        private List<WordSetDataSO.WordHintStruct> runtimeAnswerWordSetReadOnly;

        private string[] wordPlankAnswersInOrder;

        public List<WordSetDataSO.WordHintStruct> answerWordSet { get; private set; } = new List<WordSetDataSO.WordHintStruct>();

        public static GameAnswerConfig gameAnswerConfigInstance;

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

            answerWordSet.Clear();

            int wordCountRequired = gameGridInUse.rowNum;

            int wordLengthRequired = gameGridInUse.columnNum;

            List<WordSetDataSO.WordHintStruct> randomWordSet = wordSetDataToUse.GetRandomWordSetRuntime(wordLengthRequired, wordCountRequired);

            runtimeAnswerWordSetReadOnly = randomWordSet;

            if (randomWordSet == null || randomWordSet.Count == 0)
            {
                Debug.LogError("Trying to get random word set from word sets data SO, but none could be found. Please check word sets data setup in assigned WordSetDataSO in use.");

                yield break;
            }

            wordPlankAnswersInOrder = new string[wordCountRequired];

            for(int i = 0; i < wordPlankAnswersInOrder.Length; i++)
            {
                if (i >= randomWordSet.Count) break;

                wordPlankAnswersInOrder[i] = randomWordSet[i].word;

                answerWordSet.Add(randomWordSet[i]);
            }

            yield return new WaitForEndOfFrame();
        }

        public bool IsWordPlankAnswersMatched(GameGrid gridWithPlanksToCompare, bool shouldCheckKeyword = true)
        {
            if (!gridWithPlanksToCompare) return false;

            if (gridWithPlanksToCompare.wordPlankRowsInGrid == null || gridWithPlanksToCompare.wordPlankRowsInGrid.Length == 0) return false;

            if (wordPlankAnswersInOrder == null || wordPlankAnswersInOrder.Length == 0) return false;

            bool isMatched = true;

            for (int i = 0; i < wordPlankAnswersInOrder.Length; i++)
            {
                if (i >= gridWithPlanksToCompare.wordPlankRowsInGrid.Length) break;

                if (!gridWithPlanksToCompare.wordPlankRowsInGrid[i]) continue;

                if (!shouldCheckKeyword && gridWithPlanksToCompare.wordPlankRowsInGrid[i].isPlankKeyword) continue;

                string typedWord = gridWithPlanksToCompare.wordPlankRowsInGrid[i].GetPlankTypedWord();

                if (string.IsNullOrEmpty(typedWord) || string.IsNullOrWhiteSpace(typedWord))
                {
                    isMatched = false;

                    break;
                }

                if (wordPlankAnswersInOrder[i].ToLower() != typedWord.ToLower())
                {
                    isMatched = false;
                    
                    break;
                }

                if (i != gridWithPlanksToCompare.wordPlankRowsInGrid[i].plankRowOrder)
                {
                    isMatched = false;
                   
                    break;
                }
            }

            return isMatched;
        }
    }
}
