using UnityEngine;

namespace CrossClimbLite
{
    public class GameUpdateState : GameStateBase
    {
        public bool hasKeywordsUnlocked { get; private set; } = false;

        public override bool OnStateEnter()
        {
            if (!base.OnStateEnter()) return false;

            if (!enabled) return false;

            hasKeywordsUnlocked = false;

            return true;
        }

        public override bool OnStateUpdate()
        {
            if (!base.OnStateUpdate()) return false;

            if (!enabled) return false;

            return true;
        }

        public override bool OnStateExit()
        {
            if (!base.OnStateExit()) return false;

            return true;
        }
    }
}
