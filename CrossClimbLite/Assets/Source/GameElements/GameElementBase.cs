using System;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameElementBase : MonoBehaviour
    {
        protected GameElementUIBase gameElementUILinked;

        public void ConnectGameElementUI(GameElementUIBase gameElementUIToLinked)
        {
            if (!gameElementUIToLinked) return;

            gameElementUILinked = gameElementUIToLinked;
        }

        public abstract void SetGameElementSelectionStatus(bool isSelected, bool isFromUI);

        public abstract void SetGameElementLockedStatus(bool isLocked, bool isFromUI);
    }
}
