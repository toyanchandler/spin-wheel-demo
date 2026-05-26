using System;
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

        protected TextMeshProUGUI Text { get { return _text; } }

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
            VisibleTextActions[System.Convert.ToInt32(isVisible)](text, content, color);
        }

        private static readonly Action<TextMeshProUGUI, string, Color>[] VisibleTextActions =
        {
            (target, content, color) => target.enabled = false,
            (target, content, color) =>
            {
                target.enabled = true;
                target.text = content;
                target.color = color;
            }
        };
    }
}
