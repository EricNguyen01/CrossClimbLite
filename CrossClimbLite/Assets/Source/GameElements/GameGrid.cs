using UnityEngine;
using UnityEditor;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameGrid : MonoBehaviour
    {
        [SerializeField]
        private WordPlankRow WordPlankRowPrefab;

        [field: SerializeField]
        public int rowNum { get; private set; } = 5;

        [field: SerializeField]
        public int columnNum { get; private set; } = 4;

        public void InitGrid()
        {
            if (!WordPlankRowPrefab)
            {
                gameObject.SetActive(false);

                enabled = false;

                return;
            }
        }
    }
}
