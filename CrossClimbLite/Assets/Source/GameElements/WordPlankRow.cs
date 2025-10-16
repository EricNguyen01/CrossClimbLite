using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class WordPlankRow : MonoBehaviour
    {
        [field: SerializeField]
        private LetterSlotInPlank LetterSlotPrefab;

        [field: SerializeField]
        [field: Min(1.0f)]
        public float plankSlotSize { get; private set; } = 1.0f;

        //Have a UI Slot Prefab here as well

        //the row number of the plank in the game grid - starting from 0
        private int plankRowNum = 0;

        //total number of letter slots in the plank - default is 4
        private int totalLetterCountInPlank = 4;

        private LetterSlotInPlank[] letterSlotsInWordPlank;

        //the letter index that is right after the last filled letter slot and has not been filled yet.
        private int currentLetterIndex = 0;

        private GameGrid gameGridHoldingPlank;

        public void InitPlank(GameGrid parentGrid, int rowNumOfPlank, int letterCountInPlank)
        {
            if (!parentGrid || !LetterSlotPrefab)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            gameGridHoldingPlank = parentGrid;

            plankRowNum = rowNumOfPlank;

            totalLetterCountInPlank = letterCountInPlank;

            letterSlotsInWordPlank = new LetterSlotInPlank[totalLetterCountInPlank];

            for(int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                letterSlotsInWordPlank[i].InitSlot(this, i);//NO INSTANCE OF SLOT INSTANTIATED -> WILL CAUSE NULL REFERENCE HERE -> FIX ASAP!!!
            }

            currentLetterIndex = 0;
        }

        public void WriteLetterToPlankSlot(char letter, int slotIndexToWriteTo = -1)
        {
            if(letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;

            if (slotIndexToWriteTo >= totalLetterCountInPlank) slotIndexToWriteTo = totalLetterCountInPlank - 1;

            currentLetterIndex = slotIndexToWriteTo;

            letterSlotsInWordPlank[currentLetterIndex].WriteLetterToSlot(letter);

            if (currentLetterIndex == letterSlotsInWordPlank.Length - 1) return;

            currentLetterIndex++;
        }

        public void SetPlankRowNum(int rowNum)
        {
            plankRowNum = rowNum;
        }
    }
}
