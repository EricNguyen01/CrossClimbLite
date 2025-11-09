using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class GameMenuUICanvasInitializer : MonoBehaviour
    {
        [Header("Game Menu Prefabs To Spawn On Start")]

        [SerializeField]
        private GameStartLoadUI gameStartLoadUIPrefab;

        public GameStartLoadUI gameStartLoadUI { get; private set; }

        [SerializeField]
        private GameEndUI gameEndUIPrefab;

        public GameEndUI gameEndUI { get; private set; }

        public Canvas gameMenuUICanvas { get; private set; }

        private void OnEnable()
        {
            if(!gameStartLoadUIPrefab || !gameEndUIPrefab)
            {
                Debug.LogError("One or more game Menu UI Canvas Component Object Prefabs are missing. " +
                               "This might cause the game to not work properly!");
            }

            if (!gameMenuUICanvas) gameMenuUICanvas = GetComponent<Canvas>();

            if (!gameMenuUICanvas)
            {
                gameMenuUICanvas = gameObject.AddComponent<Canvas>();
            }

            SetupGameMenuUICanvas();

            gameStartLoadUI = GetComponentInChildren<GameStartLoadUI>();

            if (!gameStartLoadUI)
            {
                gameStartLoadUI = GameManager.SpawnGameObjectWithComponent<GameStartLoadUI>(gameStartLoadUIPrefab.gameObject, "GameStartLoadUI", transform);
            }

            gameEndUI = GetComponentInChildren<GameEndUI>();

            if (!gameEndUI)
            {
                gameEndUI = GameManager.SpawnGameObjectWithComponent<GameEndUI>(gameEndUIPrefab.gameObject, "GameEndUI", transform);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameStartLoadUIPrefab || !gameEndUIPrefab)
            {
                Debug.LogError("One or more game Menu UI Canvas Component Object Prefabs are missing. " +
                               "This might cause the game to not work properly!");
            }
        }
#endif

        private void SetupGameMenuUICanvas()
        {
            if (!gameMenuUICanvas) return;

            if (gameMenuUICanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                gameMenuUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        public static GameMenuUICanvasInitializer SpawnGameMenuUICanvas()
        {
            GameObject go = new GameObject("GameMenuUICanvas");

            go.transform.position = Vector3.zero;

            Canvas canvas = go.AddComponent<Canvas>();

            return go.AddComponent<GameMenuUICanvasInitializer>();
        }
    }
}
