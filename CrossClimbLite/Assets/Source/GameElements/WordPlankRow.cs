using System;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the model) only representation of a word plank (AKA a row of letter slots) in the game grid.
     * This class is none UI.
     */
    public class WordPlankRow : GameElementBase
    {
        //the row number of the plank in the game grid - starting from 0
        private int plankRowOrder = 0;

        //total number of letter slots in the plank - default is 4
        private int totalLetterCountInPlank = 4;

        private PlankLetterSlot[] letterSlotsInWordPlank;

        //the letter index that is right after the last filled letter slot and has not been filled yet.
        private int currentLetterIndex = 0;

        private GameGrid gameGridHoldingPlank;

        public bool isPlankRowSelected { get; private set; } = false;

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

            letterSlotsInWordPlank = new PlankLetterSlot[totalLetterCountInPlank];

            for (int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                GameObject letterSlotObj = new GameObject("LetterSlot_" + i);

                letterSlotObj.transform.SetParent(transform);

                letterSlotObj.transform.localPosition = Vector3.zero;

                PlankLetterSlot letterSlotComp = letterSlotObj.AddComponent<PlankLetterSlot>();

                letterSlotComp.InitSlot(this, i);

                letterSlotsInWordPlank[i] = letterSlotComp;
            }

            currentLetterIndex = 0;

            isPlankLocked = isLocked;
        }

        public void SelectNextLetterSlotIndexOnPreviousSlotFilled(int slotIndexToSet)
        {
            if (letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0)
            {
                currentLetterIndex = 0;

                return;
            }

            if(slotIndexToSet < 0) slotIndexToSet = 0;

            if (slotIndexToSet >= totalLetterCountInPlank - 1) slotIndexToSet = totalLetterCountInPlank - 1;

            if(currentLetterIndex != slotIndexToSet)
            {
                letterSlotsInWordPlank[currentLetterIndex].SetSlotSelectedStatus(false);

                currentLetterIndex = slotIndexToSet;

                letterSlotsInWordPlank[currentLetterIndex].SetSlotSelectedStatus(true);
            }
        }

        public void SetPlankRowSelectedStatus(bool isSelected)
        {
            if (!gameGridHoldingPlank) return;

            if (isSelected)
            {
                gameGridHoldingPlank.SetCurrentPlankRowSelected(this);
            }

            isPlankRowSelected = isSelected;

            InvokeOnElementSelectedEvent(isSelected);
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
                if (!letterSlotsInWordPlank[i]) continue;

                letterSlotsInWordPlank[i].SetSlotLockStatus(isLocked);
            }

            InvokeOnElementLockedEvent(isLocked);
        }
    }
}
