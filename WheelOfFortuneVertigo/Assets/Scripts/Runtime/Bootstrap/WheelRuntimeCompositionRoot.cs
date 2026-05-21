using System;
using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelRuntimeCompositionRoot : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] _runtimeComponents;
        [SerializeField] private WheelGameState _state;
        [SerializeField] private WheelStatePublisher _publisher;

        private readonly WheelEventBus _eventBus = new WheelEventBus();
        private bool _isRunning;

        private void Start()
        {
            StartRuntime();
        }

        private void OnDisable()
        {
            StopRuntime();
        }

        private void OnApplicationQuit()
        {
            StopRuntime();
        }

        public void StartRuntime()
        {
            StartActions[Convert.ToInt32(_isRunning)](this);
        }

        public void StopRuntime()
        {
            StopActions[Convert.ToInt32(!_isRunning)](this);
        }

        public void RequestSpin()
        {
            _eventBus.RequestSpin();
        }

        public void RequestLeave()
        {
            _eventBus.RequestLeave();
        }

        public void RequestRestart()
        {
            _eventBus.RequestRestart();
        }

        private void BeginRuntime()
        {
            _isRunning = true;
            TweenSetup.ConfigureOnce();
            _state.InitializeRuntime();
            for (int i = 0; i < _runtimeComponents.Length; i++)
            {
                RuntimeComponentAt(i).Initialize(_eventBus);
            }

            _state.Restart();
            _publisher.PublishAll();
        }

        private void EndRuntime()
        {
            for (int i = _runtimeComponents.Length - 1; i >= 0; i--)
            {
                RuntimeComponentAt(i).Dispose();
            }

            _eventBus.Clear();
            _isRunning = false;
        }

        private IWheelRuntimeComponent RuntimeComponentAt(int index)
        {
            MonoBehaviour component = _runtimeComponents[index];
            IWheelRuntimeComponent runtimeComponent = component as IWheelRuntimeComponent;
            if (runtimeComponent != null)
            {
                return runtimeComponent;
            }

            string componentName = component == null ? "null" : component.GetType().Name;
            throw new InvalidOperationException("Runtime component slot " + index + " must implement IWheelRuntimeComponent. Found: " + componentName);
        }

        private static readonly Action<WheelRuntimeCompositionRoot>[] StartActions =
        {
            root => root.BeginRuntime(),
            root => { }
        };

        private static readonly Action<WheelRuntimeCompositionRoot>[] StopActions =
        {
            root => { },
            root => root.EndRuntime()
        };

        private static class TweenSetup
        {
            private static bool _configured;

            public static void ConfigureOnce()
            {
                ConfigureActions[Convert.ToInt32(_configured)]();
            }

            private static readonly Action[] ConfigureActions =
            {
                () =>
                {
                    DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
                    DOTween.SetTweensCapacity(64, 16);
                    _configured = true;
                },
                () => { }
            };
        }
    }
}
