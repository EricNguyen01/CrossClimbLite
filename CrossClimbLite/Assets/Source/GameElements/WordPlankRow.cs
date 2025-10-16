using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the model) only representation of a word plank (AKA a row of letter slots) in the game grid.
     * This class is none UI.
     */
    public class WordPlankRow : MonoBehaviour
    {
        //the row number of the plank in the game grid - starting from 0
        private int plankRowOrder = 0;

        //total number of letter slots in the plank - default is 4
        private int totalLetterCountInPlank = 4;

        private LetterSlotInPlank[] letterSlotsInWordPlank;

        //the letter index that is right after the last filled letter slot and has not been filled yet.
        private int currentLetterIndex = 0;

        private GameGrid gameGridHoldingPlank;

        public bool isPlankLocked { get; private set; } = false;

        public void InitPlank(GameGrid parentGrid, int rowOrderOfPlank, int letterCountInPlank, bool isLocked = false)
        {
            if (!parentGrid)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            gameGridHoldingPlank = parentGrid;

            plankRowOrder = rowOrderOfPlank;

            totalLetterCountInPlank = letterCountInPlank;

            letterSlotsInWordPlank = new LetterSlotInPlank[totalLetterCountInPlank];

            for (int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                GameObject letterSlotObj = new GameObject("LetterSlot_" + i);

                letterSlotObj.transform.SetParent(transform);

                letterSlotObj.transform.localPosition = Vector3.zero;

                LetterSlotInPlank letterSlotComp = letterSlotObj.AddComponent<LetterSlotInPlank>();

                letterSlotComp.InitSlot(this, i);

                letterSlotsInWordPlank[i] = letterSlotComp;
            }

            currentLetterIndex = 0;

            isPlankLocked = isLocked;
        }

        public void WriteLetterToPlankSlot(char letter, int slotIndexToWriteTo = -1)
        {
            if (letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;

            if (slotIndexToWriteTo >= totalLetterCountInPlank) slotIndexToWriteTo = totalLetterCountInPlank - 1;

            currentLetterIndex = slotIndexToWriteTo;

            letterSlotsInWordPlank[currentLetterIndex].WriteLetterToSlot(letter);

            if (currentLetterIndex == letterSlotsInWordPlank.Length - 1) return;

            currentLetterIndex++;
        }

        public void SetPlankRowNum(int rowOrder)
        {
            plankRowOrder = rowOrder;
        }

        public void SetPlankLockStatus(bool isLocked)
        {
            isPlankLocked = isLocked;

            if (letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;

            for (int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                letterSlotsInWordPlank[i].SetSlotLockStatus(isLocked);
            }
        }
    }
}
