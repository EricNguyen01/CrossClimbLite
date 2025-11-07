using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class HintGiverUI : MonoBehaviour
    {
        [SerializeField]
        [NotNull, DisallowNull]
        private GameGrid gameGridLinked;

        private void Awake()
        {
            if (!gameGridLinked)
            {
                gameGridLinked = FindAnyObjectByType<GameGrid>();
            }
        }
        
        public void OnHintGiverButtonPressed()
        {
            if (!enabled) return;

            if(!gameGridLinked) return;

            if (!gameGridLinked.currentPlankBeingSelected) return;

            if(gameGridLinked.currentPlankBeingSelected.letterSlotsInWordPlank == null || 
               gameGridLinked.currentPlankBeingSelected.letterSlotsInWordPlank.Length == 0) return;

            WordPlankRow selectedPlank = gameGridLinked.currentPlankBeingSelected;

            if (string.IsNullOrEmpty(selectedPlank.plankCorrectWord)) return;

            string correctWord = selectedPlank.plankCorrectWord.ToLower();

            string playerTypedWord = selectedPlank.GetPlankTypedWord();

            if (string.IsNullOrEmpty(playerTypedWord)) playerTypedWord = "";
            
            if (playerTypedWord == correctWord) return;

            bool hasCrossedOutAnyLetter = false;

            if (!string.IsNullOrEmpty(playerTypedWord))
            {
                playerTypedWord = playerTypedWord.ToLower();
                
                for (int i = 0; i < playerTypedWord.Length; i++)
                {
                    if (string.IsNullOrEmpty(playerTypedWord[i].ToString()) || playerTypedWord[i] == ' ') continue;

                    if (playerTypedWord[i] != correctWord[i] || i >= correctWord.Length)
                    {
                        if (selectedPlank.letterSlotsInWordPlank[i].letterSlotUILinked)
                        {
                            selectedPlank.letterSlotsInWordPlank[i].letterSlotUILinked.inputField.textComponent.fontStyle = TMPro.FontStyles.Strikethrough;
                        }

                        hasCrossedOutAnyLetter = true;
                    }
                }

                if (!hasCrossedOutAnyLetter)
                {
                    for(int i = 0; i < playerTypedWord.Length; i++)
                    {
                        if (string.IsNullOrEmpty(playerTypedWord[i].ToString()) || playerTypedWord[i] == ' ')
                        {
                            selectedPlank.letterSlotsInWordPlank[i].WriteLetterToSlot(correctWord[i].ToString(), true);

                            break;
                        }
                    }
                }
                
                return;
            }

            selectedPlank.letterSlotsInWordPlank[0].WriteLetterToSlot(correctWord[0].ToString(), true);
        }
    }
}
