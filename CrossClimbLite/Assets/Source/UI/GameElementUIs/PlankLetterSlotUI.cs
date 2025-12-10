using TMPro;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class PlankLetterSlotUI : GameElementUIBase
    {
        //the non-UI plank letter slot Modal (where slot logic is stored)
        public PlankLetterSlot linkedPlankLetterSlot {  get; private set; }

        public TMP_InputField inputField { get; private set; }

        private WordPlankRowUI parentPlankRowUI;

        //INHERITED FUNCS..................................................................................

        protected override void Awake()
        {
            base.Awake();

            if (!inputField)
            {
                inputField = GetComponent<TMP_InputField>();

                if (!inputField)
                {
                    inputField = GetComponentInChildren<TMP_InputField>();
                }
            }
        }

        private void Update()
        {
            if (!enabled) return;

            if (!eventSystem) return;

            if (!inputField) return;

            if (!inputField.IsActive() || !inputField.isFocused || !isSelected) return;
            
            if (string.IsNullOrEmpty(inputField.text) || string.IsNullOrWhiteSpace(inputField.text) || inputField.text == "" || inputField.text == " ")
            {
                if (Input.GetButtonDown("DeleteText"))
                {
                    linkedPlankLetterSlot.WriteLetterToSlot(null, false);
                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    linkedPlankLetterSlot.WriteLetterToSlot(" ", false);
                }
            }
        }

        public override void InitGameElementUI(GameElementBase letterSlotToLink, GameElementUIBase parentHoldingUIToLink)
        {
            base.InitGameElementUI(letterSlotToLink);

            if (!letterSlotToLink) return;

            if (letterSlotToLink is not PlankLetterSlot) return;

            linkedPlankLetterSlot = letterSlotToLink as PlankLetterSlot;

            linkedPlankLetterSlot.ConnectGameElementUI(this);

            if (linkedPlankLetterSlot.isSlotLocked) UpdateUI_OnGameElementModalLocked(true);

            if (!inputField)
            {
                Debug.LogError("An TMP_InputField component ref is required for this plank letter slot UI: " + name +
                               " but was not found! Disabling object...");

                gameObject.SetActive(false);

                enabled = false;
            }

            if(!parentHoldingUIToLink) return;

            if(parentHoldingUIToLink is not WordPlankRowUI) return;

            parentPlankRowUI = parentHoldingUIToLink as WordPlankRowUI;
        }

        public void UpdateUI_OnModalLetterChanged(string newLetter)
        {
            if (inputField)
            {
                inputField.text = newLetter;

                inputField.text = inputField.text.ToUpper();
            }
        }

        public override void UpdateUI_OnGameElementModalSelected(bool isSelected)
        {
            base.UpdateUI_OnGameElementModalSelected(isSelected);

            if (!enabled) return;

            if (!eventSystem) return;
            
            if (!inputField) return;

            if (isSelected)
            {
                inputField.Select();

                inputField.ActivateInputField();

                return;
            }

            inputField.ReleaseSelection();

            inputField.DeactivateInputField();
        }

        //UNITY TMPro UI INPUT FIELD EVENT SUBS............................................................

        public void OnLetterSlotSelect(bool isSelected)
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            this.isSelected = isSelected;
            
            if(isSelected) linkedPlankLetterSlot.SetGameElementSelectionStatus(isSelected, false);
        }

        string previousSlotValue;
        public void OnLetterSlotValueChanged()//Use this for TM_InputField's UnityEvent
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            if (!inputField) return;

            inputField.text = inputField.text.ToUpper();

            if(inputField.text == previousSlotValue) return;

            previousSlotValue = inputField.text;

            if(inputField.textComponent.fontStyle == TMPro.FontStyles.Strikethrough)
            {
                inputField.textComponent.fontStyle = TMPro.FontStyles.Normal;
            }

            linkedPlankLetterSlot.WriteLetterToSlot(inputField.text, false);
        }

        //Overload of the above func but do not use in letter slot UI's TM_InputField's UnityEvent.
        //USE ONLY for changing letters when swapping bt/ planks
        public void OnLetterSlotValueChanged(string newLetter)
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            if (!inputField) return;

            inputField.text = newLetter.ToUpper();

            linkedPlankLetterSlot.WriteLetterToSlot(inputField.text, false);
        }
    }
}
