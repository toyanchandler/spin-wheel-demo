using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelSliceImpactAnimator
    {
        public static void PlayBombHitPulse(Object owner, RectTransform root)
        {
            Reset(owner, root);
            if (root == null) return;
            DOTween.Sequence()
                .SetTarget(owner)
                .SetUpdate(true)
                .Append(root.DOScale(WheelSliceMotion.UniformScale(WheelSliceMotion.BombFirstScale), WheelSliceMotion.BombFirstDuration).SetEase(Ease.OutQuad))
                .Append(root.DOScale(WheelSliceMotion.UniformScale(WheelSliceMotion.BombDipScale), WheelSliceMotion.BombDipDuration).SetEase(Ease.InOutSine))
                .Append(root.DOScale(WheelSliceMotion.UniformScale(WheelSliceMotion.BombSecondScale), WheelSliceMotion.BombSecondDuration).SetEase(Ease.OutQuad))
                .Append(root.DOScale(Vector3.one, WheelSliceMotion.BombSettleDuration).SetEase(Ease.OutBack));
        }

        public static void PlayRewardHitPulse(Object owner, RectTransform root)
        {
            Reset(owner, root);
            if (root == null) return;
            DOTween.Sequence()
                .SetTarget(owner)
                .SetUpdate(true)
                .Append(root.DOScale(WheelSliceMotion.UniformScale(WheelSliceMotion.RewardFirstScale), WheelSliceMotion.RewardFirstDuration).SetEase(Ease.OutQuad))
                .Append(root.DOScale(WheelSliceMotion.UniformScale(WheelSliceMotion.RewardDipScale), WheelSliceMotion.RewardDipDuration).SetEase(Ease.InOutSine))
                .Append(root.DOScale(WheelSliceMotion.UniformScale(WheelSliceMotion.RewardSecondScale), WheelSliceMotion.RewardSecondDuration).SetEase(Ease.OutQuad))
                .Append(root.DOScale(Vector3.one, WheelSliceMotion.RewardSettleDuration).SetEase(Ease.OutBack));
        }

        public static void Clear(Object owner, RectTransform root)
        {
            Reset(owner, root);
        }

        private static void Reset(Object owner, RectTransform root)
        {
            DOTween.Kill(owner);
            if (root == null) return;
            DOTween.Kill(root);
            root.localScale = Vector3.one;
        }
    }
}
