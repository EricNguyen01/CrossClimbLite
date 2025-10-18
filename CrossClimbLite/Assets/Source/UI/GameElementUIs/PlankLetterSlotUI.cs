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

            TryGetComponent<TMP_InputField>(out inputField);
        }

        protected override void InitGameElementUI<T>(T letterSlotToLink)
        {
            base.InitGameElementUI<T>(letterSlotToLink);

            if (!letterSlotToLink) return;

            if (letterSlotToLink is not PlankLetterSlot) return;

            linkedPlankLetterSlot = letterSlotToLink as PlankLetterSlot;
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

        protected override void OnGameElementSelected(bool isSelected)
        {
            if (!enabled) return;

            if (!eventSystem) return;

            if (isSelected)
            {
                eventSystem.SetSelectedGameObject(gameObject);

                return;
            }

            if (eventSystem.currentSelectedGameObject == gameObject)
            {
                eventSystem.SetSelectedGameObject(null);
            }
        }

        //UNITY TMP INPUT FIELD EVENT SUBS............................................................

        protected void OnLetterSlotSelect(bool isSelected)
        {
            if (!enabled) return;

            if (!linkedPlankLetterSlot) return;

            linkedPlankLetterSlot.SetSlotSelectedStatus(isSelected);
        }

        protected void OnLetterSlotValueChanged(string letter)
        {
            if (!enabled) return;

            this.letter = letter;

            if (!linkedPlankLetterSlot) return;

            if (letter == string.Empty || string.IsNullOrWhiteSpace(letter))
            {
                if (inputField)
                {
                    linkedPlankLetterSlot.WriteLetterToSlot(inputField.text);

                    return;
                }
            }

            linkedPlankLetterSlot.WriteLetterToSlot(letter);
        }
    }
}
