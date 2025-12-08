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

        private CanvasGroup canvasGroup;

        public static HintGiverUI hintGiverInstance;

        private void Awake()
        {
            if(hintGiverInstance && hintGiverInstance != this)
            {
                enabled = false;

                Destroy(gameObject);

                return;
            }

            hintGiverInstance = this;

            GetGameGridLinkRef();

            if(!TryGetComponent<CanvasGroup>(out canvasGroup))
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private bool GetGameGridLinkRef()
        {
            if(gameGridLinked) return true;

            if (GameManager.GameManagerInstance)
            {
                if (GameManager.GameManagerInstance.gameGridModal)
                {
                    gameGridLinked = GameManager.GameManagerInstance.gameGridModal;

                    return true;
                }
            }

            gameGridLinked = FindAnyObjectByType<GameGrid>();

            if(gameGridLinked) return true; 

            return false;
        }
        
        public void OnHintGiverButtonPressed()
        {
            if (!CanGiveHints())
            {
                if(canvasGroup && canvasGroup.interactable)
                {
                    EnableHintGiverButtonUI(false);
                }

                return;
            }

            GameManager.hintsUsedThisRound++;

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

        public bool CanGiveHints()
        {
            if (!enabled) return false;

            if (!gameGridLinked)
            {
                if (!GetGameGridLinkRef()) return false;
            }

            if (GameManager.GameManagerInstance)
            {
                if (GameManager.GameManagerInstance.gameMainStatesManager)
                {
                    GameStateBase currentState = GameManager.GameManagerInstance.gameMainStatesManager.currentGameState;

                    if (currentState && currentState.GetType() == typeof(GameUpdateState))
                    {
                        GameUpdateState updateState = currentState as GameUpdateState;

                        if (updateState.allNonKeywordsCorrectNeedsReorder) return false;
                    }
                }
            }

            if (!gameGridLinked.currentPlankBeingSelected ||
                !gameGridLinked.currentPlankBeingSelected.isPlankRowSelected ||
                gameGridLinked.currentPlankBeingSelected.isPlankLocked)
                return false;

            if (gameGridLinked.currentPlankBeingSelected.letterSlotsInWordPlank == null ||
               gameGridLinked.currentPlankBeingSelected.letterSlotsInWordPlank.Length == 0) return false;

            return true;
        }

        public void EnableHintGiverButtonUI(bool enabled)
        {
            if (!this.enabled) return;

            if (!canvasGroup) return;

            if (enabled)
            {
                canvasGroup.alpha = 1.0f;

                canvasGroup.interactable = true;

                canvasGroup.blocksRaycasts = true;

                return;
            }

            canvasGroup.alpha = 0.4f;

            canvasGroup.interactable = false;

            canvasGroup.blocksRaycasts = false;
        }
    }
}
