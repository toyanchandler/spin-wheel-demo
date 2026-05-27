using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    [DefaultExecutionOrder(-100)]
    public sealed class WheelRuntimeCompositionRoot : MonoBehaviour
    {
        private static WheelRuntimeCompositionRoot _active;

        private readonly WheelEventBus _eventBus = new WheelEventBus();
        private WheelGameSettings _settings;
        private WheelGameState _state;
        private WheelStatePublisher _publisher;
        private WheelGameFlowController _flow;
        private WheelSpinner _spinner;
        private bool _isRunning;
        private bool _hasConfiguredTween;

        public static WheelRuntimeCompositionRoot Active { get { return _active; } }
        public bool IsRunning { get { return _isRunning; } }
        public WheelEventBus SessionEventBus { get { return _eventBus; } }
        public WheelGameSettings GameSettings { get { return _settings; } }
        public WheelGameState GameState { get { return _state; } }
        public WheelStatePublisher StatePublisher { get { return _publisher; } }
        public WheelSpinner Spinner { get { return _spinner; } }

        private void Awake()
        {
            _active = this;
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
            }
        }

        public void RegisterSpinner(WheelSpinner spinner)
        {
            if (spinner == null)
            {
                return;
            }

            _spinner = spinner;
        }

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

            _settings = configSource.Settings;
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
            if (_isRunning)
            {
                return;
            }

            BeginRuntime();
        }

        public void StopRuntime()
        {
            if (!_isRunning)
            {
                return;
            }

            EndRuntime();
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
            _state.InitializeRuntime(_settings);
            _publisher.Bind(_eventBus, _state);
            _spinner?.Bind(_settings);
            _flow.Bind(_eventBus, _state, _publisher, _spinner);
            WheelRuntimeLocator.NotifyRuntimeReady(_eventBus);
            _state.Restart();
            _publisher.PublishAll();
        }

        private void EndRuntime()
        {
            if (_spinner != null)
            {
                _spinner.Unbind();
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
            _settings = null;
            _isRunning = false;
        }

        private void ConfigureTweenOnce()
        {
            if (_hasConfiguredTween)
            {
                return;
            }

            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DOTween.SetTweensCapacity(64, 16);
            _hasConfiguredTween = true;
        }
    }
}
