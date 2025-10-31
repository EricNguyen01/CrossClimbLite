using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace CrossClimbLite
{
    public class GameStateManager : MonoBehaviour
    {
        [Header("Game State Data")]

        [SerializeField]
        [NotNull, DisallowNull]
        private GameStateBase initState;

        private List<GameStateBase> allGameStates = new List<GameStateBase>();

        private GameStateBase currentGameState;

        private bool hasRunStateUpdateCoroutine = false;

        private void OnEnable()
        {
            if (!initState)
            {
                Debug.LogError($"Game State Manager: {name} is missing its InitState ref. State Manager wont work and will be disabled!");

                gameObject.SetActive(false);

                enabled = false;

                return;
            }

            if(allGameStates == null) allGameStates = new List<GameStateBase>();

            if(allGameStates.Count == 0)
            {
                allGameStates = GetComponentsInChildren<GameStateBase>(true).ToList();
            }

            bool hasInitStateInList = false;

            for(int i = 0; i < allGameStates.Count; i++)
            {
                if (!allGameStates[i]) continue;

                allGameStates[i].InitializeState(this);

                if(allGameStates[i] == initState)
                {
                    hasInitStateInList = true;
                }
            }
            
            if(!hasInitStateInList)
            {
                allGameStates.Add(initState);

                initState.InitializeState(this);
            }
        }

        private void OnDisable()
        {
            StopStateUpdateCoroutine();
        }

        public void SetCurrentGameState(GameStateBase newGameState)
        {
            if (!enabled) return;

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
            if (!enabled) return;

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
            if (!enabled) yield break;

            if (!currentGameState) yield break;

            while (hasRunStateUpdateCoroutine || enabled || currentGameState)
            {
                currentGameState.OnStateUpdate();

                yield return new WaitForSeconds(0.8f);
            }
        }
    }
}
