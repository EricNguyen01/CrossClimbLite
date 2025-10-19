using System;
using UnityEngine;

namespace CrossClimbLite
{
    [DisallowMultipleComponent]
    public abstract class GameElementBase : MonoBehaviour
    {
        public event Action OnElementUpdated;

        public event Action<bool> OnElementSelected;

        public event Action<bool> OnElementLocked;

        protected void InvokeOnElementUpdatedEvent()
        {
            OnElementUpdated?.Invoke();
        }

        protected void InvokeOnElementSelectedEvent(bool isSelected)
        {
            OnElementSelected?.Invoke(isSelected);
        }

        protected void InvokeOnElementLockedEvent(bool isLocked)
        {
            OnElementLocked?.Invoke(isLocked);
        }
    }
}
