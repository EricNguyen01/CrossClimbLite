using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class HintGiverUI : MonoBehaviour
    {
        [SerializeField]
        [NotNull, DisallowNull]
        private GameGrid gameGridLinked;

        [SerializeField]
        [Min(1.0f)]
        private float hintCooldownTime = 10.0f;

        [SerializeField]
        private Slider hintCooldownSlider;

        private bool isInCooldown;

        private bool isDisabled;

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

            if (!hintCooldownSlider) hintCooldownSlider = GetComponentInChildren<Slider>();
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

            StartHintCooldownProcess();

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
                            if (!selectedPlank.letterSlotsInWordPlank[i] || selectedPlank.letterSlotsInWordPlank[i].isSlotLocked) return;

                            selectedPlank.letterSlotsInWordPlank[i].WriteLetterToSlot(correctWord[i].ToString(), true);

                            break;
                        }
                    }
                }

                GameManager.hintsUsedThisRound++;

                return;
            }

            if (!selectedPlank.letterSlotsInWordPlank[0] || selectedPlank.letterSlotsInWordPlank[0].isSlotLocked) return;
            
            selectedPlank.letterSlotsInWordPlank[0].WriteLetterToSlot(correctWord[0].ToString(), true);

            GameManager.hintsUsedThisRound++;
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

            if (isInCooldown) return false;

            return true;
        }

        private void StartHintCooldownProcess()
        {
            if (isInCooldown)
            {
                StopCoroutine(HintCooldownCoroutine());

                isInCooldown = false;
            }

            StartCoroutine(HintCooldownCoroutine());
        }

        private IEnumerator HintCooldownCoroutine()
        {
            if (isInCooldown) yield break;

            isInCooldown = true;

            canvasGroup.blocksRaycasts = false;

            if (hintCooldownSlider)
            {
                hintCooldownSlider.value = 1.0f;

                yield return hintCooldownSlider.DOValue(0.0f, hintCooldownTime).SetEase(Ease.Linear).WaitForCompletion();
            }
            else
            {
                yield return new WaitForSeconds(hintCooldownTime);
            }

            isInCooldown = false;

            if(!isDisabled) EnableHintGiverButtonUI(true);
        }

        public void EnableHintGiverButtonUI(bool enabled)
        {
            if (!this.enabled) return;

            if (!canvasGroup) return;

            if (enabled)
            {
                if (isInCooldown) return;

                isDisabled = false;

                canvasGroup.interactable = true;

                canvasGroup.blocksRaycasts = true;

                return;
            }

            isDisabled = true;

            canvasGroup.interactable = false;

            canvasGroup.blocksRaycasts = false;
        }
    }
}
