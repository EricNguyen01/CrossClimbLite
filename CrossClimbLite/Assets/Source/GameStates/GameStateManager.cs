using System;
using System.Reflection;
using System.Collections;
using UnityEngine;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public class GameStateManager : MonoBehaviour
    {
        [Header("Game State Data")]

        [SerializeField]
        [NotNull, DisallowNull]
        private GameStateBase initState;

        [SerializeField]
        [Min(0.02f)]
        private float stateUpdateInterval = 0.2f;

        private List<GameStateBase> allGameStates = new List<GameStateBase>();

        public GameStateBase currentGameState { get; private set; }

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

        private void Start()
        {
            if (!enabled) return;

            TransitionToGameState(initState);
        }

        private void OnDisable()
        {
            StopStateUpdateCoroutine();
        }

        public void TransitionToGameState(GameStateBase newGameState)
        {
            if (!enabled) return;

            if (!newGameState)
            {
                Debug.LogWarning("Transitioned to a null game state...");

                if (currentGameState)
                {
                    Debug.LogWarning("Stopping Current State Instead");

                    StopStateUpdateCoroutine();

                    currentGameState.OnStateExit();
                }

                return;
            }

            if (!currentGameState)
            {
                currentGameState = newGameState;

                currentGameState.OnStateEnter();

                RunStateUpdateIfHasUpdateFunc();

                return;
            }

            StopStateUpdateCoroutine();

            currentGameState.OnStateExit();

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
            }
        }

        public void StopStateUpdateCoroutine()
        {
            if(!hasRunStateUpdateCoroutine) return;

            hasRunStateUpdateCoroutine = false;

            StopCoroutine(StateUpdateCoroutine());
        }

        private IEnumerator StateUpdateCoroutine()
        {
            if (!enabled) yield break;

            if (!currentGameState) yield break;

            hasRunStateUpdateCoroutine = true;

            while (hasRunStateUpdateCoroutine && enabled && currentGameState)
            {
                if (!currentGameState.OnStateUpdate())
                {
                    hasRunStateUpdateCoroutine = false;

                    yield break;
                }

                if (stateUpdateInterval <= 0.02f)
                {
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    yield return new WaitForSeconds(stateUpdateInterval);
                }
            }
        }

        public void TransitionToStateDelay(GameStateBase state, float delay)
        {
            StartCoroutine(TransitionToStateDelayCoroutine(state, delay));  
        }

        private IEnumerator TransitionToStateDelayCoroutine(GameStateBase state, float delay)
        {
            yield return new WaitForSeconds(delay);

            TransitionToGameState(state);
        }
    }
}
