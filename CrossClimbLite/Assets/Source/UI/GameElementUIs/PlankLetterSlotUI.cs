using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class PlankLetterSlotUI : GameElementUIBase
    {
        //the non-UI plank letter slot model (where slot logic is stored)
        private PlankLetterSlot linkedPlankLetterSlot;

        private TMP_InputField inputField;

        private string letter;

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

            if (linkedPlankLetterSlot.isSlotLocked) OnGameElementLocked(true);

            if (!inputField)
            {
                Debug.LogError("An TMP_InputField component ref is required for this plank letter slot UI: " + name +
                               " but was not found! Disabling object...");

                gameObject.SetActive(false);

                enabled = false;
            }
        }

        protected override void OnGameElementUpdated()
        {
            if(!enabled) return;

            if (!linkedPlankLetterSlot) return;

            if(linkedPlankLetterSlot.letter != letter)
            {
                letter = linkedPlankLetterSlot.letter;

                if (inputField)
                {
                    inputField.text = linkedPlankLetterSlot.letter;
                }
            }
        }

        protected override void OnGameElementLocked(bool isLocked)
        {
            base.OnGameElementLocked(isLocked);

            if (!enabled) return;

            if (!elementCanvasGroup) return;

            if (isLocked)
            {
                elementCanvasGroup.alpha = 0.2f;

                return;
            }

            elementCanvasGroup.alpha = 1.0f;
        }

        protected override void OnGameElementSelected(bool isSelected)
        {
            base.OnGameElementSelected(isSelected);

            if (!enabled) return;

            if (!eventSystem) return;

            if (!inputField) return;

            if (isSelected)
            {
                if (eventSystem.currentSelectedGameObject == inputField.gameObject) 
                    return;

                Debug.Log("Game Element UI Selected: " + name);
                
                eventSystem.SetSelectedGameObject(inputField.gameObject);

                return;
            }

            if (eventSystem.currentSelectedGameObject == inputField.gameObject)
            {
                Debug.Log("Game Element UI DeSelected: " + name);

                eventSystem.SetSelectedGameObject(null);
            }
        }

        //UNITY TMPro UI INPUT FIELD EVENT SUBS............................................................

        public void OnLetterSlotSelect(bool isSelected)
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            //if this line below is missing = CRAZY EVENT CALL INFINITE LOOP BUG!!!
            if (this.isSelected == isSelected) return;
            
            linkedPlankLetterSlot.SetSlotSelectedStatus(isSelected);
        }

        public void OnLetterSlotValueChanged()
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            if (!inputField) return;

            if (letter == string.Empty || string.IsNullOrWhiteSpace(letter))
            {
                linkedPlankLetterSlot.WriteLetterToSlot(inputField.text);
            }
        }
    }
}
