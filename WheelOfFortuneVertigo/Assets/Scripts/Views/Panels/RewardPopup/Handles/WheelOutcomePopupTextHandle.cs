using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupTextHandle
    {
        private readonly TextMeshProUGUI _text;

        public WheelOutcomePopupTextHandle(TextMeshProUGUI text) => _text = text;

        public void SetAlpha(float alpha) => WheelUiGraphicUtility.SetTextAlpha(_text, alpha);
        public void Apply(string value, Color color)
        {
            _text.text = value ?? string.Empty;
            _text.color = color;
        }

        public Tween FadeTo(float alpha, float duration) => _text.DOFade(alpha, duration);
    }
}
