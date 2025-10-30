using System;
using System.Reflection;
using System.Collections;
using UnityEngine;

namespace CrossClimbLite
{
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager gameStateManagerInstance;

        private GameStateBase currentGameState;

        private bool hasRunStateUpdateCoroutine = false;

        private void Awake()
        {
            if (gameStateManagerInstance)
            {
                Destroy(gameObject);

                return;
            }
            
            gameStateManagerInstance = this;

            DontDestroyOnLoad(gameObject);
        }

        private void OnDisable()
        {
            StopStateUpdateCoroutine();
        }

        public void SetCurrentGameState(GameStateBase newGameState)
        {
            if (!newGameState)
            {
                if(currentGameState) currentGameState.OnStateExit();

                return;
            }

            if (!currentGameState)
            {
                currentGameState = newGameState;

                currentGameState.OnStateEnter();

                RunStateUpdateIfHasUpdateFunc();

                return;
            }

            currentGameState.OnStateExit();

            StopStateUpdateCoroutine();

            currentGameState = newGameState;

            currentGameState.OnStateEnter();

            RunStateUpdateIfHasUpdateFunc();
        }

        private void RunStateUpdateIfHasUpdateFunc()
        {
            Type stateDerivedType = currentGameState.GetType();

            MethodInfo updateMethodInDerived = stateDerivedType.GetMethod("OnStateUpdate");

            if(updateMethodInDerived != null)
            {
                StartCoroutine(StateUpdateCoroutine());

                hasRunStateUpdateCoroutine = true;
            }
        }

        private void StopStateUpdateCoroutine()
        {
            if(!hasRunStateUpdateCoroutine) return;

            StopCoroutine(StateUpdateCoroutine());

            hasRunStateUpdateCoroutine = false;
        }

        private IEnumerator StateUpdateCoroutine()
        {
            if(!currentGameState) yield break;

            while (hasRunStateUpdateCoroutine)
            {
                currentGameState.OnStateUpdate();

                yield return new WaitForSeconds(0.8f);
            }
        }
    }
}
