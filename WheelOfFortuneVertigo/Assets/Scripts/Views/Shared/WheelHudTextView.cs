using TMPro;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class WheelHudTextView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        [WheelInject] private WheelEventBus _eventBus;

        protected TextMeshProUGUI Text => _text;

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
            Apply(snapshot);
        }

        protected abstract void Apply(WheelHudSnapshot snapshot);

        protected void ApplyVisibleText(TextMeshProUGUI text, bool isVisible, string content, Color color)
        {
            if (!isVisible)
            {
                text.enabled = false;
                return;
            }

            text.enabled = true;
            text.text = content;
            text.color = color;
        }
    }
}
