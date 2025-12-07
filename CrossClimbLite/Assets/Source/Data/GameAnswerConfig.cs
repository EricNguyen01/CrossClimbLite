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

        private bool isLoadingWordSetDataSO = false;

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
        }

        private void OnEnable()
        {
            if (!isLoadingWordSetDataSO)
            {
                if (!wordSetDataToUse)
                {
                    Debug.LogError($"WordSetDataToUse is missing on {name} game object. Attempting to load one from resources...");

                    StartCoroutine(LoadAsyncWordSetDataSO_IfNull());
                }
            }
        }

        public IEnumerator GenerateNewAnswerConfig(GameGrid gameGridInUse)
        {
            if (!enabled) yield break;

            if (!wordSetDataToUse)
            {
                Debug.LogError("Couldn't generate new answer config. Invalid or null word sets data SO provided");

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

        public bool CheckWordPlankAnswersMatched(GameGrid gridWithPlanksToCompare, out bool allNonKeywordsMatchedButNotOrdered, bool shouldCheckKeyword = true)
        {
            allNonKeywordsMatchedButNotOrdered = false;

            if (!gridWithPlanksToCompare) return false;

            if (gridWithPlanksToCompare.wordPlankRowsInGrid == null || gridWithPlanksToCompare.wordPlankRowsInGrid.Length == 0) return false;

            if (wordPlankAnswersInOrder == null || wordPlankAnswersInOrder.Length == 0) return false;

            bool isMatched = true;

            int currentNonKeywordCorrectCount = 0;

            for (int i = 0; i < wordPlankAnswersInOrder.Length; i++)
            {
                if (i >= gridWithPlanksToCompare.wordPlankRowsInGrid.Length) break;

                if (!gridWithPlanksToCompare.wordPlankRowsInGrid[i])
                {
                    //Debug.Log($"{gridWithPlanksToCompare.wordPlankRowsInGrid[i]} Is NULL");

                    continue;
                }

                if (!shouldCheckKeyword && gridWithPlanksToCompare.wordPlankRowsInGrid[i].isPlankKeyword)
                {
                    //Debug.Log($"{gridWithPlanksToCompare.wordPlankRowsInGrid[i].name} Skip Keyword");

                    continue;
                }

                string correctWord = gridWithPlanksToCompare.wordPlankRowsInGrid[i].plankCorrectWord;

                if (string.IsNullOrEmpty(correctWord) || string.IsNullOrWhiteSpace(correctWord)) continue;

                string typedWord = gridWithPlanksToCompare.wordPlankRowsInGrid[i].GetPlankTypedWord();

                if (string.IsNullOrEmpty(typedWord) || string.IsNullOrWhiteSpace(typedWord))
                {
                    //Debug.Log($"{gridWithPlanksToCompare.wordPlankRowsInGrid[i].name} has NO Typed Word");

                    isMatched = false;

                    break;
                }

                if (typedWord.ToLower() != correctWord.ToLower())
                {
                    //Debug.Log($"{gridWithPlanksToCompare.wordPlankRowsInGrid[i].name} is INcorrect");

                    isMatched = false;

                    break;
                }

                //Debug.Log($"{gridWithPlanksToCompare.wordPlankRowsInGrid[i].name} is Correct");

                if (!shouldCheckKeyword && !gridWithPlanksToCompare.wordPlankRowsInGrid[i].isPlankKeyword)
                {
                    currentNonKeywordCorrectCount++;

                    //Debug.Log($"CorrectNK: {currentNonKeywordCorrectCount} | NumNonKeyword: {gridWithPlanksToCompare.numberOfNonKeywordPlanks}");

                    if (currentNonKeywordCorrectCount == gridWithPlanksToCompare.numberOfNonKeywordPlanks)
                    {
                        allNonKeywordsMatchedButNotOrdered = true;
                    }
                }

                if (isMatched && i != gridWithPlanksToCompare.wordPlankRowsInGrid[i].plankRowOrder)
                {
                    //Debug.Log($"{gridWithPlanksToCompare.wordPlankRowsInGrid[i].name} doesnt match order");

                    isMatched = false;
                }
            }

            return isMatched;
        }

        public WordSetDataSO GetWordSetDataSOInUse()
        {
            return wordSetDataToUse;
        }

        public IEnumerator LoadAsyncWordSetDataSO_IfNull()
        {
            if (wordSetDataToUse) yield break;

            if (isLoadingWordSetDataSO) yield break;

            isLoadingWordSetDataSO = true;

            yield return Resources.LoadAsync<WordSetDataSO>("Assets/Resources/ScriptableObject/WordSetDataSO.asset");

            if (!wordSetDataToUse)
            {
                Debug.LogError("Word Set Data SO Load Failed! No Word Set Data SO is assiged. Disabling Answer Config...");

                gameObject.SetActive(false);

                enabled = false;
            }

            isLoadingWordSetDataSO = false;
        }
    }
}
