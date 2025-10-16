using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    /*
     * This class stores the code and data (AKA the model) only representation of a letter slot within a word plank (AKA a row of letter slots) in the game grid.
     * This class is none UI.
     */
    public class LetterSlotInPlank : MonoBehaviour
    {
        public char letter { get; private set; }

        public int slotIndexInPlank { get; private set; }

        private WordPlankRow wordPlankOfSlot;

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

        public void WriteLetterToSlot(char letter)
        {
            this.letter = letter;
        }

        public void SetSlotLockStatus(bool isLocked)
        {
            isSlotLocked = isLocked;
        }   
    }
}
