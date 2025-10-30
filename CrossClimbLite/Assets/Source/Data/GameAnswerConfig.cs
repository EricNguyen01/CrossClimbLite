using UnityEngine;

namespace CrossClimbLite
{
    public static class GameAnswerConfig
    {
        private static string[] wordPlankAnswersInOrder;

        public static void SetWordPlankAnswersInOrder(string[] wordPlankAnswersInOrder)
        {
            if (wordPlankAnswersInOrder == null || wordPlankAnswersInOrder.Length == 0) return;

            GameAnswerConfig.wordPlankAnswersInOrder = null;

            GameAnswerConfig.wordPlankAnswersInOrder = wordPlankAnswersInOrder;
        }

        public static bool IsWordPlankAnswersMatched(GameGrid gridWithPlanksToCompare, bool shouldCheckKeyword = true)
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
