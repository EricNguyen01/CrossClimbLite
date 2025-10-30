using UnityEngine;

namespace CrossClimbLite
{
    public abstract class GameStateBase : MonoBehaviour
    {
        public abstract void OnStateEnter();

        public abstract void OnStateUpdate();

        public abstract void OnStateExit();
    }
}
