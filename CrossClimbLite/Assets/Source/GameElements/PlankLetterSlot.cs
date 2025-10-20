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

        public void WriteLetterToSlot_FromUIOnly(string letter)
        {
            if (!wordPlankOfSlot) return;

            this.letter = letter;

            wordPlankOfSlot.SelectNextLetterSlotIndexOnPreviousSlotFilled(slotIndexInPlank + 1);
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
