using TMPro;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class HintBoxUI : GameElementUIBase
    {
        [SerializeField]
        private TextMeshProUGUI hintTextBox;

        private float baseTextSize = 0.0f;

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

        public void SetHintTextToDisplay(string hint)
        {
            if (!enabled || !hintTextBox) return;

            if(!string.IsNullOrEmpty(hint) || !string.IsNullOrWhiteSpace(hint))
            {
                if(HelperFunctions.GetWordCountFromString(hint) > 40)
                {
                    hintTextBox.fontSize = baseTextSize - 7.0f;
                }
                else
                {
                    hintTextBox.fontSize = baseTextSize;
                }
            }

            hintTextBox.text = hint;
        }

        public void RemoveHintText()
        {
            if (!enabled || !hintTextBox) return;

            hintTextBox.text = string.Empty;
        }
    }
}
