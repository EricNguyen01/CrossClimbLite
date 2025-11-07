using UnityEngine;

namespace CrossClimbLite
{
    public class GameEndState : GameStateBase
    {
        public override bool OnStateEnter()
        {
            if (!base.OnStateEnter()) return false;

            if (presetGameGridInScene)
            {
                presetGameGridInScene.SetGameElementLockedStatus(true, true);
            }

            if (GameEndUI.gameEndUIInstance)
            {
                GameEndUI.gameEndUIInstance.DisplayUIPanel();
            }

            return true;
        }

        public override bool OnStateExit()
        {
            if(!base.OnStateExit()) return false;

            if (GameEndUI.gameEndUIInstance)
            {
                GameEndUI.gameEndUIInstance.HideUIPanel();
            }

            return true;
        }
    }
}
