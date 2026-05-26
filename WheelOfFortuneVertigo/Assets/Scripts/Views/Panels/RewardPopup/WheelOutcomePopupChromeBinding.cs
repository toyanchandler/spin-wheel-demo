using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupChromeBinding : WheelOutcomePopupSceneComponentBinding<CanvasGroup>
    {
        public CanvasGroup CanvasGroup { get { return RequiredComponent; } }

        public void Hide()
        {
            CanvasGroup.alpha = 0f;
        }

        public Tween FadeIn()
        {
            return CanvasGroup.DOFade(1f, WheelOutcomePopupAnimationConfig.ChromeFadeInDuration).SetUpdate(true);
        }
    }
}
