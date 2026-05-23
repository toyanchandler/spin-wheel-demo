using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public abstract class WheelUiHostBase : MonoBehaviour
    {
        private bool _isBound;

        protected virtual void OnEnable()
        {
            WheelRuntimeLocator.RuntimeReady += OnRuntimeReady;
            WheelRuntimeLocator.RuntimeStopped += OnRuntimeStopped;
            TryBind();
        }

        protected virtual void OnDisable()
        {
            WheelRuntimeLocator.RuntimeReady -= OnRuntimeReady;
            WheelRuntimeLocator.RuntimeStopped -= OnRuntimeStopped;
            ReleaseBinding();
        }

        public void BindForEditorCapture(WheelEventBus eventBus)
        {
            if (_isBound)
            {
                return;
            }

            BindChildren(eventBus);
            _isBound = true;
        }

        public void UnbindForEditorCapture()
        {
            ReleaseBinding();
        }

        private void OnRuntimeStopped()
        {
            ReleaseBinding();
        }

        private void OnRuntimeReady(WheelEventBus eventBus)
        {
            TryBind();
        }

        private void TryBind()
        {
            if (_isBound || !WheelRuntimeLocator.IsReady)
            {
                return;
            }

            BindChildren(WheelRuntimeLocator.EventBus);
            _isBound = true;
        }

        private void ReleaseBinding()
        {
            if (!_isBound)
            {
                return;
            }

            UnbindChildren();
            _isBound = false;
        }

        protected abstract void BindChildren(WheelEventBus eventBus);
        protected abstract void UnbindChildren();

        protected static T RequireAssigned<T>(T component, string fieldName) where T : Component
        {
            Require(component != null, fieldName + " is not assigned on " + typeof(T).Name + " host dependency.");
            return component;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.InvalidOperationException(message);
            }
        }
    }
}
