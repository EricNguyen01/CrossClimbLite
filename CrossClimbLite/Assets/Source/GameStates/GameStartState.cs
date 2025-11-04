using System.Collections;
using UnityEngine;

namespace CrossClimbLite
{
    public class GameStartState : GameStateBase
    {
        [SerializeField]
        private GameGrid gameGridPrefab;

        [SerializeField]
        private GameGrid gameGridRuntime;

        [SerializeField]
        private GameAnswerConfig gameAnswerConfigPrefab;

        private void OnEnable()
        {
            if (!gameGridRuntime)
            {
                gameGridRuntime = FindAnyObjectByType<GameGrid>();

                if (!gameGridRuntime && gameGridPrefab)
                {
                    GameObject gameGridObj = Instantiate(gameGridPrefab.gameObject, Vector3.zero, Quaternion.identity);

                    GameGrid gameGridComp = gameGridObj.GetComponent<GameGrid>();

                    if (gameGridComp) gameGridRuntime = gameGridComp;
                    else gameGridRuntime = gameGridObj.AddComponent<GameGrid>();
                }
            }

            if (!gameGridRuntime)
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

            if (!GameAnswerConfig.gameAnswerConfigInstance || !gameGridRuntime) return;

            StartCoroutine(GameStartProcess());
        }

        public override void OnStateExit()
        {
            if (!enabled) return;
        }

        private IEnumerator GameStartProcess()
        {
            yield return GameAnswerConfig.gameAnswerConfigInstance.GenerateNewAnswerConfig(gameGridRuntime);

            gameGridRuntime.SetPlanksBasedOnWordSet(GameAnswerConfig.gameAnswerConfigInstance.answerWordSet);
        }
    }
}
