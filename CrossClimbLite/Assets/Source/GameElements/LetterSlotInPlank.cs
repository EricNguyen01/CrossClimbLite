using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class LetterSlotInPlank : MonoBehaviour
    {
        public char letter { get; private set; }

        public int slotIndexInPlank { get; private set; }

        private WordPlankRow wordPlankOfSlot;

        public void InitSlot(WordPlankRow holdingWordPlank, int slotIndexInPlank)
        {
            if(!holdingWordPlank)
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
    }
}
