using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelExitConfirmationAnimator
    {
        public static Sequence PlayShow(WheelExitConfirmationBindings bindings)
        {
            bindings.PrepareShow();
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(bindings.CanvasGroup.DOFade(1f, 0.16f))
                .Join(bindings.ContentRoot.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack));
        }

        public static Sequence PlayHide(WheelExitConfirmationBindings bindings, TweenCallback onComplete)
        {
            bindings.PrepareHide();
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(bindings.CanvasGroup.DOFade(0f, 0.12f))
                .Join(bindings.ContentRoot.DOScale(new Vector3(0.96f, 0.96f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .OnComplete(onComplete);
        }
    }
}
