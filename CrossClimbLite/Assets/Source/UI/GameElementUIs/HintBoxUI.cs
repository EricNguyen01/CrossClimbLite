using TMPro;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class HintBoxUI : GameElementUIBase
    {
        [SerializeField]
        private TextMeshProUGUI hintTextBox;

        [SerializeField]
        private Color hintTextColor = Color.white;

        [SerializeField]
        private Color messageTextColor = Color.yellow;

        [SerializeField]
        private string plankReOrderMessage = "Re-order the words so each only differs from the above and below by 1 letter!";

        [SerializeField]
        [ReadOnlyInspector]
        private bool isPlankReOrderMessageActive = false;

        private string lastHintText;

        private float baseTextSize = 0.0f;

        public enum HintBoxTextType
        {
            Hint = 1,
            Message = 2,
        }

        public static HintBoxUI hintBoxUIInstance;

        protected override void Awake()
        {
            if (hintBoxUIInstance && hintBoxUIInstance != this)
            {
                gameObject.SetActive(false);

                enabled = false;

                Destroy(gameObject);

                return;
            }

            hintBoxUIInstance = this;

            base.Awake();

            if (!hintTextBox)
            {
                if(!TryGetComponent<TextMeshProUGUI>(out hintTextBox))
                {
                    hintTextBox = GetComponentInChildren<TextMeshProUGUI>();    
                }
            }

            if(!hintTextBox)
            {
                Debug.LogError("Hint Box: " + name + " doesnt have a valid text box ref to display hint. Disabling hint box!");

                enabled = false;

                return;
            }

            baseTextSize = hintTextBox.fontSize;
        }

        public void SetHintTextToDisplay(string hint, HintBoxTextType hintBoxTextType = HintBoxTextType.Hint)
        {
            lastHintText = hint;

            if (isPlankReOrderMessageActive)
            {
                SetHintTextToDisplayInternal(plankReOrderMessage);

                return;
            }

            SetHintTextToDisplayInternal(hint, hintBoxTextType);
        }

        private void SetHintTextToDisplayInternal(string hint, HintBoxTextType hintBoxTextType = HintBoxTextType.Hint)
        {
            if (!enabled || !hintTextBox) return;

            if (!string.IsNullOrEmpty(hint) || !string.IsNullOrWhiteSpace(hint))
            {
                if (HelperFunctions.GetWordCountFromString(hint) > 40)
                {
                    hintTextBox.fontSize = baseTextSize - 7.0f;
                }
                else
                {
                    hintTextBox.fontSize = baseTextSize;
                }
            }

            if (hintBoxTextType == HintBoxTextType.Hint)
            {
                hintTextBox.color = hintTextColor;
            }
            else if(hintBoxTextType == HintBoxTextType.Message)
            {
                hintTextBox.color = messageTextColor;
            }

            hintTextBox.text = hint;
        }

        public void RemoveHintText()
        {
            if (!enabled || !hintTextBox) return;

            hintTextBox.text = string.Empty;
        }

        public void EnablePlankReOrderNeededMessage(bool enabled)
        {
            if (enabled)
            {
                isPlankReOrderMessageActive = true;

                SetHintTextToDisplayInternal(plankReOrderMessage, HintBoxTextType.Message);

                return;
            }

            isPlankReOrderMessageActive = false;

            SetHintTextToDisplayInternal(lastHintText, HintBoxTextType.Hint);
        }
    }
}
