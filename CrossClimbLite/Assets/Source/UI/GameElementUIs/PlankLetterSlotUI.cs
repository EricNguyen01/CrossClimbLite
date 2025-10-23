using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class PlankLetterSlotUI : GameElementUIBase
    {
        //the non-UI plank letter slot Modal (where slot logic is stored)
        private PlankLetterSlot linkedPlankLetterSlot;

        public TMP_InputField inputField { get; private set; }

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

        public override void InitGameElementUI(GameElementBase letterSlotToLink)
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
                //Debug.Log("Game Element UI Selected: " + name);

                inputField.Select();
                
                //eventSystem.SetSelectedGameObject(inputField.gameObject);

                return;
            }

            inputField.ReleaseSelection();
        }

        //UNITY TMPro UI INPUT FIELD EVENT SUBS............................................................

        public void OnLetterSlotSelect(bool isSelected)
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            linkedPlankLetterSlot.SetGameElementSelectionStatus(isSelected, true);
        }

        public void OnLetterSlotValueChanged()
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            if (!inputField) return;

            inputField.text = inputField.text.ToUpper();

            linkedPlankLetterSlot.WriteLetterToSlot(inputField.text, true);
        }
    }
}
