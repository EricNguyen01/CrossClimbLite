using System;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the model) only representation of a letter slot within a word plank (AKA a row of letter slots) in the game grid.
     * This class is none UI.
     */
    public class PlankLetterSlot : GameElementBase
    {
        public string letter { get; private set; }

        public int slotIndexInPlank { get; private set; }

        public WordPlankRow wordPlankOfSlot { get; private set; }

        public bool isSlotSelected { get; private set; }

        public bool isSlotLocked { get; private set; } = false;

        public void InitSlot(WordPlankRow holdingWordPlank, int slotIndexInPlank)
        {
            if (!holdingWordPlank)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            wordPlankOfSlot = holdingWordPlank;

            this.slotIndexInPlank = slotIndexInPlank;
        }

        public void WriteLetterToSlot(string newLetter, bool isFromUI)
        {
            if (!wordPlankOfSlot) return;

            letter = newLetter;

            wordPlankOfSlot.SelectNextLetterSlotIndexOnPreviousSlotFilled(slotIndexInPlank + 1);

            if (!isFromUI && gameElementUILinked)
            {
                if(gameElementUILinked is PlankLetterSlotUI)
                {
                    PlankLetterSlotUI letterSlotUI = gameElementUILinked as PlankLetterSlotUI;

                    letterSlotUI.UpdateUI_OnModelLetterChanged(letter);
                }
            }
        }

        public override void SetGameElementSelectionStatus(bool isSelected, bool isFromUI)
        {
            isSlotSelected = isSelected;

            if (wordPlankOfSlot)
            {
                if (isSelected && !wordPlankOfSlot.isPlankRowSelected) wordPlankOfSlot.SetGameElementSelectionStatus(true, false);

                wordPlankOfSlot.SetCurrentLetterSlotIndex(slotIndexInPlank);
            }

            if (!isFromUI && gameElementUILinked) gameElementUILinked.UpdateUI_OnGameElementModelSelected(isSelected);
        }

        public override void SetGameElementLockedStatus(bool isLocked, bool isFromUI)
        {
            if (wordPlankOfSlot)
            {
                if (!wordPlankOfSlot.isPlankLocked && isLocked) return;

                else if (wordPlankOfSlot.isPlankLocked && !isLocked) return;
            }

            isSlotLocked = isLocked;

            if (!isFromUI && gameElementUILinked) gameElementUILinked.UpdateUI_OnGameElementModelLocked(isLocked);
        }   
    }
}
