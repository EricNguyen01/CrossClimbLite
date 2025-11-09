using UnityEngine;
using TMPro;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class RoundStatsTextUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI textMeshPro;

        private void Awake()
        {
            if(!textMeshPro)
            {
                TryGetComponent<TextMeshProUGUI>(out textMeshPro);
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
            if (!textMeshPro) return;

            string timeTakenText = HelperFunctions.FormatSecondsToString(GameManager.timeTakenThisRound);

            textMeshPro.text = "Time Taken: " + timeTakenText;
        }
    }
}
