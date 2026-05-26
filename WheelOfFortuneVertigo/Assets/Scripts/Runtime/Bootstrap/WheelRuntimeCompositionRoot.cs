using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    [DefaultExecutionOrder(-100)]
    public sealed class WheelRuntimeCompositionRoot : MonoBehaviour
    {
        private readonly WheelEventBus _eventBus = new WheelEventBus();
        private WheelGameState _state;
        private WheelStatePublisher _publisher;
        private WheelGameFlowController _flow;
        private bool _isRunning;
        private bool _hasConfiguredTween;

        private void EnsureGameplayComponents()
        {
            if (_state == null)
            {
                _state = GetComponent<WheelGameState>();
            }

            if (_publisher == null)
            {
                _publisher = GetComponent<WheelStatePublisher>();
            }

            if (_flow == null)
            {
                _flow = GetComponent<WheelGameFlowController>();
            }

            WheelGameConfigSource configSource = GetComponent<WheelGameConfigSource>();
            if (configSource == null || configSource.Settings == null)
            {
                throw new InvalidOperationException("game_wheel_runtime requires WheelGameConfigSource with WheelGameSettings assigned.");
            }

            WheelRuntimeLocator.RegisterSettings(configSource.Settings);
            WheelRuntimeLocator.RegisterGameplay(_eventBus, _state, _publisher);
        }

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
            StopActions[Convert.ToInt32(_isRunning)](this);
        }

        public void RequestSpin()
        {
            _eventBus.RequestSpin();
        }

        public void RequestLeaveConfirmation()
        {
            _eventBus.RequestLeaveConfirmation();
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
            EnsureGameplayComponents();
            ConfigureTweenOnce();
            _state.InitializeRuntime();
            _publisher.Bind(_eventBus);
            _flow.Bind(_eventBus);
            WheelRuntimeLocator.Spinner.Bind(_eventBus);
            WheelRuntimeLocator.NotifyRuntimeReady();
            _state.Restart();
            _publisher.PublishAll();
        }

        private void EndRuntime()
        {
            if (WheelRuntimeLocator.Spinner != null)
            {
                WheelRuntimeLocator.Spinner.Unbind();
            }

            if (_flow != null)
            {
                _flow.Unbind();
            }

            if (_publisher != null)
            {
                _publisher.Unbind();
            }

            WheelRuntimeLocator.Clear();
            _eventBus.Clear();
            _isRunning = false;
        }

        private void ConfigureTweenOnce()
        {
            ConfigureTweenActions[Convert.ToInt32(_hasConfiguredTween)](this);
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

        private static readonly Action<WheelRuntimeCompositionRoot>[] ConfigureTweenActions =
        {
            root =>
            {
                DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
                DOTween.SetTweensCapacity(64, 16);
                root._hasConfiguredTween = true;
            },
            root => { }
        };
    }
}
