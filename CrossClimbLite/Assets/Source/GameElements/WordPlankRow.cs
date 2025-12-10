using System.Text;
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
        [field: Header("Plank Data")]

        [field: SerializeField]
        [field: ReadOnlyInspectorPlayMode]
        public string plankCorrectWord { get; private set; } = "";

        [ReadOnlyInspector]
        [SerializeField]
        private string plankTypedWord = "";

        private StringBuilder plankTypedWordStrBuilder = new StringBuilder();

        [field: SerializeField]
        [field: ReadOnlyInspectorPlayMode]
        public string plankHint { get; private set; } = "";

        [field: Space]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        //the row number of the plank in the game grid - starting from 0
        public int plankRowOrder { get; private set; } = 0;

        [field: Space]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public bool isPlankKeyword { get; private set; } = false;

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public bool isPlankRowSelected { get; private set; } = false;

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public bool isPlankLocked { get; private set; } = false;

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public bool isPlankFilled { get; private set; } = false;

        //total number of letter slots in the plank - default is 4
        private int totalLetterCountInPlank = 4;

        [field: SerializeField]
        [field: HideInInspector]
        public PlankLetterSlot[] letterSlotsInWordPlank { get; private set; }

        //the letter index that is right after the last filled letter slot and has not been filled yet.
        private int currentLetterIndex = 0;

        [field: Space]

        [field: ReadOnlyInspector]
        [field: SerializeField]
        public GameGrid gameGridHoldingPlank { get; private set; }

        [ReadOnlyInspector]
        [SerializeField]
        private WordPlankRowUI wordPlankRowUILinked;

        private WordPlankRow plankRowAbove;

        private WordPlankRow plankRowBelow;

        private void Awake()
        {
            if (plankHint == string.Empty || string.IsNullOrEmpty(plankHint))
            {
                plankHint = "Plank Row_" + plankRowOrder + "'s Hint";
            }
        }

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

            isPlankKeyword = isKeyword;

            if (isKeyword)
            {
                if(gameElementUILinked && gameElementUILinked is WordPlankRowUI)
                {
                    WordPlankRowUI rowUI = gameElementUILinked as WordPlankRowUI;
                    
                    rowUI.UpdateUI_PlankModalIsKeyword(isKeyword);
                }
            }

            if(isLocked) SetGameElementLockedStatus(true, true);

            if(plankHint == string.Empty || string.IsNullOrEmpty(plankHint))
            {
                plankHint = "Plank Row_" + plankRowOrder + "'s Hint";
            }
        }

        public void SetPlankAboveThisPlank(WordPlankRow plankAbove)
        {
            plankRowAbove = plankAbove;
        }

        public void SetPlankBelowThisPlank(WordPlankRow plankBelow)
        {
            plankRowBelow = plankBelow;
        }

        public override void ConnectGameElementUI(GameElementUIBase gameElementUIToLinked)
        {
            base.ConnectGameElementUI(gameElementUIToLinked);

            if(gameElementUILinked && gameElementUILinked is WordPlankRowUI)
            {
                wordPlankRowUILinked = gameElementUILinked as WordPlankRowUI;
            }
        }

        public void SelectNextLetterSlotIndexOnPreviousSlotFilled(int slotIndexToSet)
        {
            if (letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0)
            {
                currentLetterIndex = 0;

                return;
            }

            if(slotIndexToSet < 0)
            {
                slotIndexToSet = 0;

                if (plankRowAbove && !plankRowAbove.isPlankKeyword && plankRowAbove != this)
                {
                    letterSlotsInWordPlank[currentLetterIndex].SetGameElementSelectionStatus(false, true);

                    currentLetterIndex = slotIndexToSet;

                    plankRowAbove.letterSlotsInWordPlank[plankRowAbove.letterSlotsInWordPlank.Length - 1].SetGameElementSelectionStatus(true, true);

                    return;
                }
            }

            if (slotIndexToSet > totalLetterCountInPlank - 1)
            {
                slotIndexToSet = totalLetterCountInPlank - 1;

                if (plankRowBelow && !plankRowBelow.isPlankKeyword && plankRowBelow != this)
                {
                    letterSlotsInWordPlank[currentLetterIndex].SetGameElementSelectionStatus(false, true);

                    currentLetterIndex = slotIndexToSet;

                    plankRowBelow.letterSlotsInWordPlank[0].SetGameElementSelectionStatus(true, true);

                    return;
                }
            }

            if(currentLetterIndex != slotIndexToSet)
            {
                letterSlotsInWordPlank[currentLetterIndex].SetGameElementSelectionStatus(false, true);

                currentLetterIndex = slotIndexToSet;

                letterSlotsInWordPlank[currentLetterIndex].SetGameElementSelectionStatus(true, true);
            }
        }

        public override void SetGameElementSelectionStatus(bool isSelected, bool shouldUpdateUI)
        {
            if (!gameGridHoldingPlank) return;

            if(isPlankLocked) return;

            isPlankRowSelected = isSelected;

            if (isSelected)
            {
                gameGridHoldingPlank.SetCurrentPlankRowSelected(this);

                if (HintBoxUI.hintBoxUIInstance) HintBoxUI.hintBoxUIInstance.SetHintTextToDisplay(plankHint);
            }
            else
            {
                if (HintBoxUI.hintBoxUIInstance) HintBoxUI.hintBoxUIInstance.RemoveHintText();
            }

            if (shouldUpdateUI && wordPlankRowUILinked)
            {
                wordPlankRowUILinked.UpdateUI_OnGameElementModalSelected(isSelected);
            }
        }

        public void SetPlankRowOrder(int rowOrder)
        {
            plankRowOrder = rowOrder;
        }

        public void SetCurrentLetterSlotIndex(int slotIndexToSet)
        {
            if (slotIndexToSet < 0) slotIndexToSet = 0;

            if (slotIndexToSet >= totalLetterCountInPlank - 1) slotIndexToSet = totalLetterCountInPlank - 1;

            currentLetterIndex = slotIndexToSet;
        }

        public void ClearPlankTypedWord()
        {
            if (string.IsNullOrEmpty(plankTypedWord) || string.IsNullOrWhiteSpace(plankTypedWord)) return;

            if(letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;

            for(int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                if (!letterSlotsInWordPlank[i]) continue;

                letterSlotsInWordPlank[i].WriteLetterToSlot("", true);
            }
        }

        public override void SetGameElementLockedStatus(bool isLocked, bool shouldUpdateUI)
        {
            isPlankLocked = isLocked;

            SetPlankRowChildrenLetterSlotUILockedStatus(isLocked, shouldUpdateUI);

            if (shouldUpdateUI && wordPlankRowUILinked) wordPlankRowUILinked.UpdateUI_OnGameElementModalLocked(isLocked);
        }

        public void SetPlankRowChildrenLetterSlotUILockedStatus(bool isLocked, bool shouldUpdateUI = true)
        {
            if (letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;
            
            for (int i = 0; i < letterSlotsInWordPlank.Length; i++)
            {
                if (!letterSlotsInWordPlank[i]) continue;

                letterSlotsInWordPlank[i].SetGameElementLockedStatus(isLocked, shouldUpdateUI);
            }
        }

        public void PlankLetterSlotsSwapWith(WordPlankRow plankToSwap)
        {
            if (!plankToSwap) return;

            if (this == plankToSwap) return;
        }

        public string GetPlankTypedWord()
        {
            return plankTypedWord;
        }

        public void SetPlankCorrectWord(string correctWord)
        {
            plankCorrectWord = correctWord;
        }

        public void SetPlankHint(string hint)
        {
            if (hint == string.Empty || string.IsNullOrEmpty(hint) || hint == "")
            {
                hint = "Plank Row_" + plankRowOrder + "'s Hint";
            }

            plankHint = hint;
        }

        public void UpdatePlankTypedWordAtLetterSlot(PlankLetterSlot letterSlot)
        {
            if (!letterSlot) return;

            if(!letterSlot.wordPlankOfSlot || letterSlot.wordPlankOfSlot != this) return;

            if(letterSlotsInWordPlank == null || letterSlotsInWordPlank.Length == 0) return;

            if(plankTypedWordStrBuilder == null) plankTypedWordStrBuilder = new StringBuilder();

            if(plankTypedWordStrBuilder.Length == 0)
            {
                char[] letters = new char[letterSlotsInWordPlank.Length];

                for(int i = 0; i < letters.Length; i++)
                {
                    letters[i] = ' ';
                }

                plankTypedWordStrBuilder.Append(letters);
            }
            else if (plankTypedWordStrBuilder.Length > letterSlotsInWordPlank.Length)
            {
                int charDiff = plankTypedWordStrBuilder.Length - letterSlotsInWordPlank.Length;

                plankTypedWordStrBuilder.Remove(plankTypedWordStrBuilder.Length - charDiff, charDiff);
            }

            if (letterSlot.slotIndexInPlank < plankTypedWordStrBuilder.Length)
            {
                if (letterSlot.letter != string.Empty && !string.IsNullOrWhiteSpace(letterSlot.letter))
                {
                    plankTypedWordStrBuilder[letterSlot.slotIndexInPlank] = letterSlot.letter[0];
                }
                else
                {
                    plankTypedWordStrBuilder[letterSlot.slotIndexInPlank] = ' ';
                }

                goto FinalizeTypedWord;
            }
            
            plankTypedWordStrBuilder.Append(letterSlot.letter[0]);

        FinalizeTypedWord:

            plankTypedWord = plankTypedWordStrBuilder.ToString();

            if (wordPlankRowUILinked) wordPlankRowUILinked.UpdateUIInternalData_UpdatePlankUILetter(plankTypedWord);

            if(string.IsNullOrEmpty(plankTypedWord) || string.IsNullOrWhiteSpace(plankTypedWord))
            {
                isPlankFilled = false;

                return;
            }

            if (gameGridHoldingPlank && plankTypedWord.Length == gameGridHoldingPlank.columnNum)
            {
                for (int i = 0; i < plankTypedWord.Length; i++)
                {
                    if (plankTypedWord[i] == ' ' || plankTypedWord[i] == char.MinValue)
                    {
                        isPlankFilled = false;

                        return;
                    }
                }

                isPlankFilled = true;

                gameGridHoldingPlank.InvokeOnAWordPlankFilledEvent(plankTypedWord);
            }
        }
    }
}
