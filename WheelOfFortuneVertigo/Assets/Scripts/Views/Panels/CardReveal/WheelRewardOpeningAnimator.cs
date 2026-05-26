using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardOpeningAnimator
    {
        public static void Show(WheelRewardOpeningRootBinding binding, Object tweenTarget, ref Tween tween)
        {
            tween?.Kill();
            binding.PrepareShow();
            tween = DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .Append(binding.CanvasGroup.DOFade(1f, WheelRewardOpeningMotion.ShowFadeDuration).SetEase(Ease.OutQuad))
                .Join(binding.ContentRoot.DOScale(Vector3.one, WheelRewardOpeningMotion.ShowScaleDuration).SetEase(Ease.OutBack));
        }

        public static void Hide(WheelRewardOpeningRootBinding binding, Object tweenTarget, ref Tween tween)
        {
            tween?.Kill();
            tween = binding.CanvasGroup.DOFade(0f, WheelRewardOpeningMotion.HideFadeDuration)
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .OnComplete(binding.CompleteHide);
        }
    }
}
