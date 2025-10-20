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

        private TMP_InputField inputField;

        //INHERITED FUNCS..................................................................................

        protected override void Awake()
        {
            base.Awake();

            if (!inputField)
            {
                if (!TryGetComponent<TMP_InputField>(out inputField))
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
            if(inputField) inputField.text = newLetter;
        }

        public override void UpdateUI_OnGameElementModalSelected(bool isSelected)
        {
            base.UpdateUI_OnGameElementModalSelected(isSelected);

            if (!enabled) return;

            if (!eventSystem) return;

            if (!inputField) return;

            if (isSelected)
            {
                if (eventSystem.currentSelectedGameObject == inputField.gameObject) 
                    return;

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
           
            linkedPlankLetterSlot.WriteLetterToSlot(inputField.text, true);
        }
    }
}
