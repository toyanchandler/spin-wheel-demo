using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupChromeHandle
    {
        private readonly CanvasGroup _canvasGroup;

        public WheelOutcomePopupChromeHandle(CanvasGroup canvasGroup) => _canvasGroup = canvasGroup;

        public void Hide() => _canvasGroup.alpha = 0f;
        public Tween FadeIn() => _canvasGroup.DOFade(1f, WheelOutcomePopupAnimationConfig.ChromeFadeInDuration).SetUpdate(true);
    }
}
