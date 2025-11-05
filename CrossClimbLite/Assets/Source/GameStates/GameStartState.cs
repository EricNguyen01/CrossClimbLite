using System.Collections;
using UnityEngine;

namespace CrossClimbLite
{
    public class GameStartState : GameStateBase
    {
        [Header("Game Start Prefabs")]

        [SerializeField]
        private GameGrid gameGridPrefab;

        [SerializeField]
        private GameAnswerConfig gameAnswerConfigPrefab;

        [Header("Game Start Scene Preset Components")]

        [SerializeField]
        private GameGrid presetGameGridInScene;

        private void OnEnable()
        {
            if (!presetGameGridInScene)
            {
                presetGameGridInScene = FindAnyObjectByType<GameGrid>();

                if (!presetGameGridInScene && gameGridPrefab)
                {
                    GameObject gameGridObj = Instantiate(gameGridPrefab.gameObject, Vector3.zero, Quaternion.identity);

                    GameGrid gameGridComp = gameGridObj.GetComponent<GameGrid>();

                    if (gameGridComp) presetGameGridInScene = gameGridComp;
                    else presetGameGridInScene = gameGridObj.AddComponent<GameGrid>();
                }
            }

            if (!presetGameGridInScene)
            {
                Debug.LogError("Game Grid doesnt exist! Game will not start!");

                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            if (GameAnswerConfig.gameAnswerConfigInstance == null && !FindAnyObjectByType<GameAnswerConfig>())
            {
                if (gameAnswerConfigPrefab)
                {
                    Instantiate(gameAnswerConfigPrefab.gameObject, Vector3.zero, Quaternion.identity);
                }
                else
                {
                    Debug.LogError("Game Answer Config and game answer word set data could not be found and its prefab is not provided for instantiation. Game won't start!");

                    gameObject.SetActive(false);

                    enabled = false;

                    return;
                }
            }
        }

        public override void OnStateEnter()
        {
            if (!enabled) return;

            if (!GameAnswerConfig.gameAnswerConfigInstance || !presetGameGridInScene) return;

            StartCoroutine(GameStartLoadProcess());
        }

        public override void OnStateExit()
        {
            if (!enabled) return;
        }

        private IEnumerator GameStartLoadProcess()
        {
            if (GameStartLoadUI.gameStartLoadUIInstance)
            {
                GameStartLoadUI.gameStartLoadUIInstance.OnGameStartsLoading();
            }

            presetGameGridInScene.SetGameElementLockedStatus(true, true);

            yield return GameAnswerConfig.gameAnswerConfigInstance.GenerateNewAnswerConfig(presetGameGridInScene);

            presetGameGridInScene.SetPlanksBasedOnWordSet(GameAnswerConfig.gameAnswerConfigInstance.answerWordSet);

            presetGameGridInScene.SetGameElementLockedStatus(false, true);

            if (GameStartLoadUI.gameStartLoadUIInstance)
            {
                GameStartLoadUI.gameStartLoadUIInstance.OnGameStopLoading();
            }
        }
    }
}
