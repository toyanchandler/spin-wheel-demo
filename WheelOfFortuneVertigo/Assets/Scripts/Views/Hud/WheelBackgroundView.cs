using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(Image))]
    public sealed class WheelBackgroundView : MonoBehaviour
    {
        [SerializeField] private Image _image;
        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            Color tint = snapshot.BackgroundColor;
            tint.r = Mathf.Min(1f, tint.r + 0.02f);
            tint.g = Mathf.Min(1f, tint.g + 0.025f);
            tint.b = Mathf.Min(1f, tint.b + 0.035f);
            tint.a = 0.32f;
            _image.color = tint;
        }
    }
}
