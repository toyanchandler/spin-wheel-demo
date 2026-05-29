using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupRootHandle
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly Image _overlay;

        public WheelOutcomePopupRootHandle(CanvasGroup canvasGroup, Image overlay)
        {
            _canvasGroup = canvasGroup;
            _overlay = overlay;
        }

        public GameObject Root => _canvasGroup.gameObject;
        public bool IsVisible => Root.activeSelf;

        public void Show() => Root.SetActive(true);
        public void Hide() => Root.SetActive(false);
        public void SetAlpha(float alpha) => _canvasGroup.alpha = alpha;
        public Tween FadeTo(float alpha, float duration) => _canvasGroup.DOFade(alpha, duration);

        public void SetOverlayAlpha(float alpha)
        {
            if (_overlay == null) return;
            Color color = _overlay.color;
            color.a = alpha;
            _overlay.color = color;
        }
    }
}
