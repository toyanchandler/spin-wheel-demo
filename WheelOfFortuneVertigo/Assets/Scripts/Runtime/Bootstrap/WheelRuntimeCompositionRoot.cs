using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Boots the gameplay session on <c>game_wheel_runtime</c>.
    /// See <c>ARCHITECTURE_AND_LOGIC.md</c> section 3 for lifecycle order.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class WheelRuntimeCompositionRoot : MonoBehaviour
    {
        private readonly WheelEventBus _eventBus = new WheelEventBus();
        private IRandomSource _randomSource = UnityRandomSource.Shared;
        private WheelGameSettings _settings;
        private WheelGameState _state;
        private WheelStatePublisher _publisher;
        private WheelGameFlowController _flow;
        private WheelSpinner _spinner;
        private bool _isRunning;
        private bool _hasConfiguredTween;

        public bool IsRunning => _isRunning;
        public WheelEventBus SessionEventBus => _eventBus;
        public WheelGameSettings GameSettings => _settings;
        public WheelGameState GameState => _state;
        public WheelStatePublisher StatePublisher => _publisher;
        public WheelSpinner Spinner => _spinner;

        public void RegisterSpinner(WheelSpinner spinner)
        {
            if (spinner != null)
            {
                _spinner = spinner;
            }
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
            LoadGameplayComponentsFromScene();
            InitializeGameStateFromSettings();
            BindGameplaySystems();
            RegisterSessionAndNotifyViews();
            StartNewRun();
        }

        private void LoadGameplayComponentsFromScene()
        {
            _state = GetComponent<WheelGameState>();
            _publisher = GetComponent<WheelStatePublisher>();
            _flow = GetComponent<WheelGameFlowController>();

            WheelGameConfigSource configSource = GetComponent<WheelGameConfigSource>();
            if (configSource == null || configSource.Settings == null)
            {
                throw new InvalidOperationException(
                    "game_wheel_runtime requires WheelGameConfigSource with WheelGameSettings assigned.");
            }

            _settings = configSource.Settings;
        }

        private void InitializeGameStateFromSettings()
        {
            ConfigureTweenOnce();
            _state.InitializeRuntime(_settings, _randomSource);
        }

        private void BindGameplaySystems()
        {
            _publisher.Bind(_eventBus, _state);

            ResolveSpinnerFromScene();

            if (_spinner == null)
            {
                throw new InvalidOperationException(
                    "game_wheel_runtime requires a WheelSpinner in the scene.");
            }

            _spinner.Bind(_settings, _eventBus.Presentation.Spin, _randomSource);
            _eventBus.Presentation.Spin.RegisterDriver(_spinner);
            _flow.Bind(_eventBus, _state, _publisher, _spinner);
        }

        private void RegisterSessionAndNotifyViews()
        {
            WheelRuntimeLocator.RegisterSession(_eventBus, _settings, _state, _publisher, _flow, _spinner);
            WheelRuntimeLocator.NotifyRuntimeReady();
        }

        private void StartNewRun()
        {
            _state.Restart();
            _publisher.PublishAll();
        }

        private void EndRuntime()
        {
            _spinner?.Unbind();
            _flow?.Unbind();
            _publisher?.Unbind();
            WheelRuntimeLocator.Clear();
            _eventBus.Clear();
            _settings = null;
            _isRunning = false;
        }

        private void ResolveSpinnerFromScene()
        {
            if (_spinner != null)
            {
                return;
            }

            _spinner = FindFirstObjectByType<WheelSpinner>();
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
