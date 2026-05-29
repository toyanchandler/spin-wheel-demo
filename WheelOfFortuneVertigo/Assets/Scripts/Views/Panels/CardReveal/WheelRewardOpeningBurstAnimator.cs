using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardOpeningBurstAnimator
    {
        public static void Play(WheelRewardOpeningBurstBinding binding, Object tweenTarget, int visibleCount, ref Sequence sequence)
        {
            sequence?.Kill();
            binding.Prepare();

            sequence = DOTween.Sequence()
                .SetTarget(tweenTarget)
                .SetUpdate(true)
                .AppendInterval(WheelRewardOpeningBurstMotion.FirstBurstDelay)
                .AppendCallback(() => binding.Emit(ResolveOpeningBurstCount(visibleCount)))
                .AppendInterval(WheelRewardOpeningBurstMotion.SecondBurstDelay)
                .AppendCallback(() => binding.Emit(ResolveSecondBurstCount(visibleCount)))
                .AppendInterval(WheelRewardOpeningBurstMotion.FinalBurstDelay)
                .AppendCallback(() => binding.Emit(ResolveFinalBurstCount(visibleCount)))
                .AppendInterval(WheelRewardOpeningBurstMotion.FadeOutDelay)
                .OnComplete(binding.Stop);
        }

        private static int ResolveOpeningBurstCount(int visibleCount)
        {
            return Mathf.Clamp(
                visibleCount * WheelRewardOpeningBurstMotion.OpeningBurstPerCard,
                WheelRewardOpeningBurstMotion.OpeningBurstMin,
                WheelRewardOpeningBurstMotion.OpeningBurstMax);
        }

        private static int ResolveSecondBurstCount(int visibleCount)
        {
            return Mathf.Clamp(
                visibleCount * WheelRewardOpeningBurstMotion.SecondBurstPerCard,
                WheelRewardOpeningBurstMotion.SecondBurstMin,
                WheelRewardOpeningBurstMotion.SecondBurstMax);
        }

        private static int ResolveFinalBurstCount(int visibleCount)
        {
            return Mathf.Clamp(
                visibleCount * WheelRewardOpeningBurstMotion.FinalBurstPerCard,
                WheelRewardOpeningBurstMotion.FinalBurstMin,
                WheelRewardOpeningBurstMotion.FinalBurstMax);
        }
    }
}
