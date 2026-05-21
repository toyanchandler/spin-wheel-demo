using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(Image))]
    public sealed class WheelBackgroundView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private Image _image;
        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Dispose()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _image.color = snapshot.BackgroundColor;
        }
    }
}
