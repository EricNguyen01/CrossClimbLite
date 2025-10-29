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

        public static void CompareWordPlankAnswers(GameGrid gridWithPlanksToCompare)
        {
            if (!gridWithPlanksToCompare) return;

            if (gridWithPlanksToCompare.wordPlankRowsInGrid == null || gridWithPlanksToCompare.wordPlankRowsInGrid.Length == 0) return;


        }
    }
}
