using System;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the Modal) only representation of a letter slot within a word plank (AKA a row of letter slots) in the game grid.
     * This class is none UI.
     */
    public class PlankLetterSlot : GameElementBase
    {
        [field: Header("Plank Letter Slot Data")]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public string letter { get; private set; }

        [field: Space]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public int slotIndexInPlank { get; private set; }

        [field: Space]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public bool isSlotSelected { get; private set; }

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public bool isSlotLocked { get; private set; } = false;

        [field: Space]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public WordPlankRow wordPlankOfSlot { get; private set; }

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public PlankLetterSlotUI letterSlotUILinked { get; private set; }

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

        public override void ConnectGameElementUI(GameElementUIBase gameElementUIToLinked)
        {
            base.ConnectGameElementUI(gameElementUIToLinked);

            if(gameElementUILinked && gameElementUILinked is PlankLetterSlotUI)
            {
                letterSlotUILinked = gameElementUILinked as PlankLetterSlotUI;
            }
        }

        public void WriteLetterToSlot(string newLetter, bool shouldUpdateUI)
        {
            if (!wordPlankOfSlot) return;
            
            letter = newLetter;

            wordPlankOfSlot.UpdatePlankTypedWordAtLetterSlot(this);
            
            wordPlankOfSlot.SelectNextLetterSlotIndexOnPreviousSlotFilled(slotIndexInPlank + 1);

            if (shouldUpdateUI && gameElementUILinked)
            {
                if(gameElementUILinked is PlankLetterSlotUI)
                {
                    PlankLetterSlotUI letterSlotUI = gameElementUILinked as PlankLetterSlotUI;

                    letterSlotUI.UpdateUI_OnModalLetterChanged(letter);
                }
            }
        }

        public override void SetGameElementSelectionStatus(bool isSelected, bool shouldUpdateUI)
        {
            isSlotSelected = isSelected;
            
            if (wordPlankOfSlot)
            {
                wordPlankOfSlot.SetGameElementSelectionStatus(isSelected, true);

                wordPlankOfSlot.SetCurrentLetterSlotIndex(slotIndexInPlank);
            }

            if (shouldUpdateUI && letterSlotUILinked)
            {
                letterSlotUILinked.UpdateUI_OnGameElementModalSelected(isSelected);
            }
        }

        public override void SetGameElementLockedStatus(bool isLocked, bool shouldUpdateUI)
        {
            if (wordPlankOfSlot)
            {
                if (!wordPlankOfSlot.isPlankLocked && isLocked) return;

                else if (wordPlankOfSlot.isPlankLocked && !isLocked) return;
            }

            isSlotLocked = isLocked;

            if (!shouldUpdateUI && letterSlotUILinked) letterSlotUILinked.UpdateUI_OnGameElementModalLocked(isLocked);
        }   
    }
}
