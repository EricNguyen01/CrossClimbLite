using TMPro;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class HintBoxUI : GameElementUIBase
    {
        [SerializeField]
        private TextMeshProUGUI hintTextBox;

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
            }
        }

        public void SetHintTextToDisplay(string hint)
        {
            if (!enabled || !hintTextBox) return;

            hintTextBox.text = hint;
        }

        public void RemoveHintText()
        {
            if (!enabled || !hintTextBox) return;

            hintTextBox.text = string.Empty;
        }
    }
}
