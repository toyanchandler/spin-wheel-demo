using DG.Tweening;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardOpeningBurst
    {
        private readonly WheelRewardOpeningBurstBinding _binding;
        private readonly Object _tweenTarget;
        private Sequence _sequence;

        public WheelRewardOpeningBurst(WheelRewardOpeningBurstBinding binding, Object tweenTarget)
        {
            _binding = binding;
            _tweenTarget = tweenTarget;
        }

        public void Play(int rewardCardCount)
        {
            if (!_binding.IsAvailable) return;
            Stop();
            int visibleCount = Mathf.Clamp(
                rewardCardCount,
                WheelRewardOpeningBurstMotion.MinimumVisibleRewardCount,
                WheelRewardOpeningBurstMotion.MaximumVisibleRewardCount);
            WheelRewardOpeningBurstAnimator.Play(_binding, _tweenTarget, visibleCount, ref _sequence);
        }

        public void Stop()
        {
            if (_sequence != null)
            {
                Sequence sequence = _sequence;
                _sequence = null;
                sequence.Kill();
            }

            _binding.Stop();
        }
    }
}
