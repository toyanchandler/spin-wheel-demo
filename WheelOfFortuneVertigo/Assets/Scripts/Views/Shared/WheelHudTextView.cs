using System;
using TMPro;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public abstract class WheelHudTextView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private TextMeshProUGUI _text;
        private WheelEventBus _eventBus;

        protected TextMeshProUGUI Text { get { return _text; } }

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
