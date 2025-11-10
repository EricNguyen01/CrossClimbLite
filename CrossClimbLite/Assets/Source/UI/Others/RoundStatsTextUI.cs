using UnityEngine;
using TMPro;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RoundStatsTextUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMeshPro;

        private void Awake()
        {
            if(!textMeshPro)
            {
                if(!TryGetComponent<TextMeshProUGUI>(out textMeshPro))
                {
                    textMeshPro = gameObject.AddComponent<TextMeshProUGUI>();
                }
            }
        }

        public void UpdateHintsUsedText()
        {
            if (!textMeshPro) return;

            string hintsUsedText = GameManager.hintsUsedThisRound.ToString();

            textMeshPro.text = $"Hints Used: {hintsUsedText}";
        }

        public void UpdateTimeTakenText()
        {
            UpdateTimeTakenTextCustom("Time Taken: ", GameManager.timeTakenThisRound);
        }

        public void UpdateTimeTakenTextCustom(string prefixText, float timeTakenCustom)
        {
            if (!textMeshPro) return;

            string timeTakenText = HelperFunctions.FormatSecondsToString(timeTakenCustom);

            if(string.IsNullOrEmpty(prefixText) || string.IsNullOrWhiteSpace(prefixText))
            {
                textMeshPro.text = timeTakenText;

                return;
            }

            textMeshPro.text = prefixText + timeTakenText;
        }
    }
}
