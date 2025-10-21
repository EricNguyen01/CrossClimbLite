using System;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the Modal) only representation of a word plank (AKA a row of letter slots) in the game grid.
     * This class is none UI.
     */
    public class WordPlankRow : GameElementBase
    {
        //the row number of the plank in the game grid - starting from 0
        private int plankRowOrder = 0;

        //total number of letter slots in the plank - default is 4
        private int totalLetterCountInPlank = 4;

        public PlankLetterSlot[] letterSlotsInWordPlank { get; private set; }

        //the letter index that is right after the last filled letter slot and has not been filled yet.
        private int currentLetterIndex = 0;

        public GameGrid gameGridHoldingPlank { get; private set; }

        public bool isPlankRowSelected { get; private set; } = false;

        public bool isPlankKeyword { get; private set; } = false;

        public bool isPlankLocked { get; private set; } = false;

        //plank dragable is determined by whether it's locked or not
        public bool isPlankDragable { get; private set; } = true;

        public void InitPlank(GameGrid parentGrid, int rowOrderOfPlank, int letterCountInPlank, bool isKeyword = false, bool isLocked = false)
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

            isPlankDragable = isLocked;

            isPlankKeyword = isKeyword;

            if (isKeyword)
            {
                if(gameElementUILinked && gameElementUILinked is WordPlankRowUI)
                {
                    WordPlankRowUI rowUI = gameElementUILinked as WordPlankRowUI;
                    
                    rowUI.UpdateUI_PlankModalIsKeyword(isKeyword);
                }
            }

            if(isLocked) SetGameElementLockedStatus(true, false);
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
                letterSlotsInWordPlank[currentLetterIndex].SetGameElementSelectionStatus(false, false);

                currentLetterIndex = slotIndexToSet;

                letterSlotsInWordPlank[currentLetterIndex].SetGameElementSelectionStatus(true, false);
            }
        }

        public override void SetGameElementSelectionStatus(bool isSelected, bool isFromUI)
        {
            if (!gameGridHoldingPlank) return;

            isPlankRowSelected = isSelected;

            if (isSelected)
            {
                gameGridHoldingPlank.SetCurrentPlankRowSelected(this);
            }

            if (!isFromUI && gameElementUILinked) gameElementUILinked.UpdateUI_OnGameElementModalSelected(isSelected);
        }

        public void SetPlankRowNum(int rowOrder)
        {
            plankRowOrder = rowOrder;
        }

        public void SetCurrentLetterSlotIndex(int slotIndexToSet)
        {
            if (slotIndexToSet < 0) slotIndexToSet = 0;

            if (slotIndexToSet >= totalLetterCountInPlank - 1) slotIndexToSet = totalLetterCountInPlank - 1;

            currentLetterIndex = slotIndexToSet;
        }

        public override void SetGameElementLockedStatus(bool isLocked, bool isFromUI)
        {
            isPlankLocked = isLocked;

            isPlankDragable = isLocked;

            if (letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;

            for (int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                if (!letterSlotsInWordPlank[i]) continue;

                letterSlotsInWordPlank[i].SetGameElementLockedStatus(isLocked, false);
            }

            if (!isFromUI && gameElementUILinked) gameElementUILinked.UpdateUI_OnGameElementModalLocked(isLocked);
        }

        public void PlankLetterSlotsSwapWith(WordPlankRow plankToSwap)
        {
            if (!plankToSwap) return;

            if (this == plankToSwap) return;
        }
    }
}
