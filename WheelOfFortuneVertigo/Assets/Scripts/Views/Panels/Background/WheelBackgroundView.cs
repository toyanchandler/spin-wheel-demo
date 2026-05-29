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
            _image.color = snapshot.BackgroundColor;
        }
    }
}
