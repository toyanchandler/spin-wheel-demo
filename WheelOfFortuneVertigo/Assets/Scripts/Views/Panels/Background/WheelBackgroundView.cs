using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(Image))]
    [WheelBind]
    public sealed class WheelBackgroundView : MonoBehaviour
    {
        [SerializeField] private Image _image;

        [WheelInject] private WheelEventBus _eventBus;

        [WheelAfterInject]
        private void Connect()
        {
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
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
