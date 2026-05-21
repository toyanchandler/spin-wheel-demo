using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(Button))]
    public abstract class WheelButtonAction : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private Button _button;
        private WheelEventBus _eventBus;

        protected WheelEventBus EventBus { get { return _eventBus; } }

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _button.onClick.AddListener(HandleClick);
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Dispose()
        {
            _button.onClick.RemoveListener(HandleClick);
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void HandleClick()
        {
            Execute();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _button.interactable = IsInteractable(snapshot);
        }

        protected abstract bool IsInteractable(WheelHudSnapshot snapshot);
        protected abstract void Execute();
    }
}
