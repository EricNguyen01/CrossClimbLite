using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameManager : MonoBehaviour
    {
        [Header("Required Main Game Prefabs To Spawn On Start")]

        [SerializeField]
        private string mainGameSceneName = "GameScene";

        [SerializeField]
        private GameGrid gameGridModalPrefab;

        public GameGrid gameGridModal { get; private set; }

        [SerializeField]
        private GameAnswerConfig gameAnswerConfigPrefab;

        public GameAnswerConfig gameAnswerConfig { get; private set; }

        [SerializeField]
        private GameStateManager gameMainStatesManagerPrefab;

        public GameStateManager gameMainStatesManager { get; private set; }

        [SerializeField]
        private GameUICanvasInitializer gameUICanvasPrefab;

        public GameUICanvasInitializer gameUICanvas { get; private set; }

        [SerializeField]
        private GameMenuUICanvasInitializer gameMenuUICanvasPrefab;

        public GameMenuUICanvasInitializer gameMenuUICanvas { get; private set; }

        [SerializeField]
        private EventSystem eventSystemPrefab;

        [field: Header("Runtime Data")]

        [field: SerializeField]
        [field: ReadOnlyInspector]
        public bool hasFinishedInitGameComponents { get; private set; } = false;

        [Header("Runtime Player Stats")]

        [SerializeField]
        [ReadOnlyInspector]
        public static float timeTakenThisRound = 0.0f;

        [SerializeField]
        [ReadOnlyInspector]
        public static int hintsUsedThisRound = 0;

        public static GameManager GameManagerInstance;

        private void Awake()
        {
            if (GameManagerInstance && GameManagerInstance != this)
            {
                Destroy(gameObject);

                return;
            }

            GameManagerInstance = this;

            if(!gameGridModalPrefab || 
               !gameAnswerConfigPrefab || 
               !gameMainStatesManagerPrefab || 
               !gameUICanvasPrefab ||
               !gameMenuUICanvasPrefab)
            {
                Debug.LogError($"Game Manager {name}: One or more required core prefabs used for game inits are missing! " +
                               "The Game Might Not Work!");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameGridModalPrefab ||
               !gameAnswerConfigPrefab ||
               !gameMainStatesManagerPrefab ||
               !gameUICanvasPrefab ||
               !gameMenuUICanvasPrefab)
            {
                Debug.LogError($"Game Manager {name}: One or more required core prefabs used for game inits are missing! " +
                               "The Game Might Not Work!");
            }
        }
#endif

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnMainGameSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnMainGameSceneLoaded;
        }

        private void OnMainGameSceneLoaded(Scene sc, LoadSceneMode loadSceneMode)
        {
            if (string.IsNullOrEmpty(mainGameSceneName) || string.IsNullOrWhiteSpace(mainGameSceneName))
            {
                mainGameSceneName = "GameScene";
            }

            if (sc.name == mainGameSceneName || !sc.name.ToLower().Contains("menu"))
            {
                InitMainGameSceneComponents();
            }
        }

        private void InitMainGameSceneComponents()
        {
            if (!enabled || !Application.isPlaying) return;

            hasFinishedInitGameComponents = false;

            //The ORDER of init MUST NOT BE CHANGED UNLESS U KNOW WHAT UR DOING!!!

            if (!EventSystem.current)
            {
                EventSystem.current = SpawnGameObjectWithComponent<EventSystem>(eventSystemPrefab.gameObject, "Event System", null);
            }

            InitGameMenuUI();

            InitGameUI();

            InitGameGridModal();

            InitGameAnswerConfig();

            InitGameMainStatesManager();

            hasFinishedInitGameComponents = true;
        }

        private void InitGameGridModal()
        {
            gameGridModal = FindAnyObjectByType<GameGrid>();

            if (!gameGridModal)
            {
                gameGridModal = SpawnGameObjectWithComponent<GameGrid>(gameGridModalPrefab.gameObject, "GameGridModal", null);
            }
        }

        private void InitGameAnswerConfig()
        {
            gameAnswerConfig = FindAnyObjectByType<GameAnswerConfig>();

            if(!gameAnswerConfig)
            {
                gameAnswerConfig = SpawnGameObjectWithComponent<GameAnswerConfig>(gameAnswerConfigPrefab.gameObject, "GameAnswerConfig", null);
            }
        }

        private void InitGameMainStatesManager()
        {
            if(!gameMainStatesManager)
            {
                foreach (GameStateManager stateManager in FindObjectsByType<GameStateManager>(FindObjectsSortMode.None))
                {
                    if (stateManager.isMainStateManager || stateManager.name.ToLower().Contains("gamemainstate"))
                    {
                        gameMainStatesManager = stateManager;

                        break;
                    }
                }
            }

            if (gameMainStatesManager) return;

            gameMainStatesManager = SpawnGameObjectWithComponent<GameStateManager>(gameMainStatesManagerPrefab.gameObject, "GameMainStatesManager", null);
        }

        private void InitGameUI()
        {
            gameUICanvas = FindAnyObjectByType<GameUICanvasInitializer>();

            if (!gameUICanvas)
            {
                gameUICanvas = SpawnGameObjectWithComponent<GameUICanvasInitializer>(gameUICanvasPrefab.gameObject, "GameUICanvas", null);
            }
        }

        private void InitGameMenuUI()
        {
            gameMenuUICanvas = FindAnyObjectByType<GameMenuUICanvasInitializer>();

            if(!gameMenuUICanvas)
            {
                gameMenuUICanvas = SpawnGameObjectWithComponent<GameMenuUICanvasInitializer>(gameMenuUICanvasPrefab.gameObject, "GameMenuUICanvas", null);
            }

            if (gameMenuUICanvas && gameMenuUICanvas.gameStartLoadUI)
            {
                gameMenuUICanvas.gameStartLoadUI.DisplayUIPanel();
            }
        }

        public static T SpawnGameObjectWithComponent<T>(GameObject prefabGO, string objectName, Transform parentTransform) where T : MonoBehaviour
        {
            GameObject go = null;

            if (prefabGO)
            {
                if (parentTransform) go = Instantiate(prefabGO, parentTransform);
                else go = Instantiate(prefabGO);
            }
            else
            {
                if(string.IsNullOrEmpty(objectName) || string.IsNullOrWhiteSpace(objectName))
                {
                    objectName = "Required Game Component Object";
                }

                go = new GameObject(objectName);

                go.transform.position = Vector3.zero;

                if (parentTransform)
                {
                    go.transform.SetParent(parentTransform);

                    go.transform.localPosition = Vector3.zero;
                }
            }
            
            var comp = go.GetComponent<T>();

            if(!comp) comp = go.GetComponentInChildren<T>();

            if(!comp) comp = go.AddComponent<T>();

            return comp;
        }

        public static void ResetRuntimePlayerStats()
        {
            timeTakenThisRound = 0;

            hintsUsedThisRound = 0;
        }
    }
}
