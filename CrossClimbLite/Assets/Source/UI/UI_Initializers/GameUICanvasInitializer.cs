using UnityEngine;
using UnityEngine.UI;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class GameUICanvasInitializer : MonoBehaviour
    {
        [Header("Game UI Prefabs To Spawn On Start")]

        [SerializeField]
        private Image gameBackgroundUIPrefab;

        public Image gameBackgroundUI { get; private set; } 

        [SerializeField]
        private HintGiverUI hintGiverButtonUIPrefab;

        public HintGiverUI hintGiverButtonUI { get; private set; }

        [SerializeField]
        private HintBoxUI hintBoxUIPanelPrefab;

        public HintBoxUI hintBoxUI { get; private set; }

        [SerializeField]
        private ReplayButton startNewButtonUIPrefab;

        public ReplayButton startNewButtonUI { get; private set; }

        [SerializeField]
        private GameGridUI gameGridLayoutUIPrefab;

        public GameGridUI gameGridLayoutUI { get; private set; }

        public Canvas gameUICanvas { get; private set; }    

        public CanvasGroup gameUICanvasGroup { get; private set; }  

        private void OnEnable()
        {
            if (!gameBackgroundUIPrefab || !hintGiverButtonUIPrefab || !hintBoxUIPanelPrefab || !gameGridLayoutUIPrefab)
            {
                Debug.LogError("One or more game UI Canvas Component Object Prefabs are missing. " +
                               "This might cause the game to not work properly!");
            }

            if(!gameUICanvas) gameUICanvas = GetComponent<Canvas>();

            if (!gameUICanvas)
            {
                gameUICanvas = gameObject.AddComponent<Canvas>();
            }

            SetupGameUICanvas();

            if(!gameUICanvasGroup) gameUICanvasGroup = GetComponent<CanvasGroup>();

            if (!gameUICanvasGroup) gameUICanvasGroup = gameObject.AddComponent<CanvasGroup>();

            bool hasBackgroundUI = false;

            foreach(Image image in GetComponentsInChildren<Image>())
            {
                if(image.gameObject.name.ToLower().Contains("gamebackground") ||
                    image.gameObject.name.ToLower().Contains("game background"))
                {
                    hasBackgroundUI = true;

                    break;
                }
            }

            if(!hasBackgroundUI)
            {
                gameBackgroundUI = GameManager.SpawnGameObjectWithComponent<Image>(gameBackgroundUIPrefab.gameObject, "GameBackgroundUI", transform);
            }

            hintGiverButtonUI = GetComponentInChildren<HintGiverUI>();

            if (!hintGiverButtonUI)
            {
                hintGiverButtonUI = GameManager.SpawnGameObjectWithComponent<HintGiverUI>(hintGiverButtonUIPrefab.gameObject, "HintGiverButtonUI", transform);
            }

            hintBoxUI = GetComponentInChildren<HintBoxUI>();

            if (!hintBoxUI)
            {
                hintBoxUI = GameManager.SpawnGameObjectWithComponent<HintBoxUI>(hintBoxUIPanelPrefab.gameObject, "HintBoxUI", transform);
            }

            startNewButtonUI = GetComponentInChildren<ReplayButton>();

            if (!startNewButtonUI)
            {
                startNewButtonUI = GameManager.SpawnGameObjectWithComponent<ReplayButton>(startNewButtonUI.gameObject, "StartNewButton", transform);
            }

            gameGridLayoutUI = GetComponentInChildren<GameGridUI>();

            if (!gameGridLayoutUI)
            {
                gameGridLayoutUI = GameManager.SpawnGameObjectWithComponent<GameGridUI>(gameGridLayoutUIPrefab.gameObject, "GameGridLayoutUI", transform);
            }

            if (gameGridLayoutUI)
            {
                gameGridLayoutUI.transform.SetSiblingIndex(1);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!gameBackgroundUIPrefab || !hintGiverButtonUIPrefab || !hintBoxUIPanelPrefab || !gameGridLayoutUIPrefab)
            {
                Debug.LogError("One or more game UI Canvas Component Object Prefabs are missing. " +
                               "This might cause the game to not work properly!");
            }
        }
#endif

        public static GameUICanvasInitializer SpawnGameUICanvas()
        {
            GameObject go = new GameObject("GameUICanvas");

            go.transform.position = Vector3.zero;

            Canvas canvas = go.AddComponent<Canvas>();

            return go.AddComponent<GameUICanvasInitializer>();
        }

        private void SetupGameUICanvas()
        {
            if (!gameUICanvas) return;

            if(gameUICanvas.renderMode != RenderMode.ScreenSpaceCamera)
            {
                gameUICanvas.renderMode = RenderMode.ScreenSpaceCamera;
            }

            if (!gameUICanvas.worldCamera)
            {
                if (!Camera.main)
                {
                    GameObject camGO = new GameObject("Main Camera");

                    camGO.transform.position = Vector3.zero;

                    Camera cam = camGO.AddComponent<Camera>();

                    cam.orthographic = true;

                    gameUICanvas.worldCamera = cam;
                }
                else
                {
                    gameUICanvas.worldCamera = Camera.main;
                }
            }

            if (gameUICanvas.planeDistance != 10.0f) gameUICanvas.planeDistance = 10.0f;
        }

        public void DisableGameUICanvas(bool disable)
        {
            if (!gameUICanvasGroup) gameUICanvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (disable)
            {
                gameUICanvasGroup.interactable = false;

                gameUICanvasGroup.blocksRaycasts = false;

                return;
            }

            gameUICanvasGroup.interactable = true;

            gameUICanvasGroup.blocksRaycasts = true;
        }
    }
}
