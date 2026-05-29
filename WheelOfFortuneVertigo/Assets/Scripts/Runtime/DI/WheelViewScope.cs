using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelViewScope : MonoBehaviour
    {
        private MonoBehaviour[] _bindables = System.Array.Empty<MonoBehaviour>();
        private WheelViewContainer _views;
        private WheelEventBus _eventBus;
        private bool _isBound;

        private void Awake()
        {
            _views = WheelViewContainer.Build(transform);
            _bindables = WheelBindDiscovery.Collect(transform);
        }

        private void OnEnable()
        {
            WheelRuntimeLocator.RuntimeReady += OnRuntimeReady;
            WheelRuntimeLocator.RuntimeStopped += OnRuntimeStopped;
            TryBind();
        }

        private void OnDisable()
        {
            WheelRuntimeLocator.RuntimeReady -= OnRuntimeReady;
            WheelRuntimeLocator.RuntimeStopped -= OnRuntimeStopped;
            ReleaseBinding();
        }

        public void BindForEditorCapture(WheelEventBus eventBus)
        {
            if (_isBound) return;
            EnsureDiscovered();
            BindAll(eventBus);
            _isBound = true;
        }

        public void UnbindForEditorCapture()
        {
            ReleaseBinding();
        }

        private void OnRuntimeStopped()
        {
            _eventBus = null;
            ReleaseBinding();
        }

        private void OnRuntimeReady(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            TryBind();
        }

        private void TryBind()
        {
            if (_isBound || !WheelRuntimeLocator.IsReady || _eventBus == null) return;
            EnsureDiscovered();
            BindAll(_eventBus);
            _isBound = true;
        }

        private void EnsureDiscovered()
        {
            if (_views != null) return;
            _views = WheelViewContainer.Build(transform);
            _bindables = WheelBindDiscovery.Collect(transform);
        }

        private void BindAll(WheelEventBus eventBus)
        {
            for (int i = 0; i < _bindables.Length; i++) WheelInjector.Inject(_bindables[i], eventBus, _views);
        }

        private void ReleaseBinding()
        {
            if (!_isBound) return;
            for (int i = 0; i < _bindables.Length; i++) WheelInjector.Uninject(_bindables[i]);
            _isBound = false;
        }
    }
}
