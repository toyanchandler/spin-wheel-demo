using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupRectHandle
    {
        private readonly RectTransform _rect;
        private Vector2 _homeAnchorMin;
        private Vector2 _homeAnchorMax;
        private Vector2 _homePivot;
        private Vector2 _homeAnchoredPosition;
        private Vector3 _homeScale;

        public WheelOutcomePopupRectHandle(RectTransform rect) => _rect = rect;

        public RectTransform RectTransform => _rect;
        public Vector3 HomeScale => _homeScale;

        public void CaptureHome()
        {
            _homeAnchorMin = _rect.anchorMin;
            _homeAnchorMax = _rect.anchorMax;
            _homePivot = _rect.pivot;
            _homeAnchoredPosition = _rect.anchoredPosition;
            _homeScale = _rect.localScale;
        }

        public void RestoreHome()
        {
            _rect.anchorMin = _homeAnchorMin;
            _rect.anchorMax = _homeAnchorMax;
            _rect.pivot = _homePivot;
            _rect.anchoredPosition = _homeAnchoredPosition;
            _rect.localScale = _homeScale;
        }

        public void SetScale(Vector3 scale) => _rect.localScale = scale;
        public Tween TweenHomeScale(float duration, Ease ease) => _rect.DOScale(_homeScale, duration).SetEase(ease);
    }
}
