using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupRootBinding : WheelOutcomePopupSceneComponentBinding<CanvasGroup>
    {
        private Image _overlay;

        public GameObject Root { get { return gameObject; } }
        public CanvasGroup CanvasGroup { get { return RequiredComponent; } }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public bool IsVisible { get { return gameObject.activeSelf; } }

        public void SetAlpha(float alpha)
        {
            CanvasGroup.alpha = alpha;
        }

        public Tween FadeTo(float alpha, float duration)
        {
            return CanvasGroup.DOFade(alpha, duration);
        }

        public void SetOverlayAlpha(float alpha)
        {
            if (_overlay == null)
            {
                TryGetComponent(out _overlay);
            }

            if (_overlay == null)
            {
                return;
            }

            Color color = _overlay.color;
            color.a = alpha;
            _overlay.color = color;
        }
    }
}
