using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    /// <summary>
    /// Loot panel reaction to <see cref="WheelHudSnapshot"/> — defer vs immediate commit logic.
    /// See ARCHITECTURE_AND_LOGIC.md §14.
    /// </summary>
    internal sealed class WheelRewardPanelHudCoordinator
    {
        private readonly WheelRewardPanelInventoryState _inventory;
        private readonly WheelRewardPanelRenderer _renderer;
        private readonly WheelRewardPanelLayout _layout;
        private readonly object _tweenTarget;
        private Tween _pendingFallbackTween;

        public WheelRewardPanelHudCoordinator(
            WheelRewardPanelInventoryState inventory,
            WheelRewardPanelRenderer renderer,
            WheelRewardPanelLayout layout,
            object tweenTarget)
        {
            _inventory = inventory;
            _renderer = renderer;
            _layout = layout;
            _tweenTarget = tweenTarget;
        }

        public void Stop()
        {
            KillFallbackCommit();
            _layout.StopScroll();
        }

        /// <summary>Cancels timed fallback commit while reward flight is in progress.</summary>
        public void HoldForArrival()
        {
            KillFallbackCommit();
        }

        public int ResolveLandingIndex(string rewardId)
        {
            return _inventory.ResolveLandingIndex(rewardId);
        }

        public void HandleHudSnapshot(WheelHudSnapshot snapshot)
        {
            WheelHudRewardCardsSnapshot rewards = snapshot.Rewards;
            _renderer.SetPresentation(rewards.RewardCardFrameSprite, rewards.DefaultRewardTitle);

            if (_inventory.ShouldDeferRewardGain(snapshot))
            {
                ApplyDeferredGain(snapshot);
                return;
            }

            ApplyImmediate(snapshot);
        }

        public void PrepareLandingLayout()
        {
            int reservedCount = Mathf.Max(_inventory.RenderedCount, _inventory.PendingCount);
            _layout.LayoutReservedCards(reservedCount, true);
            _renderer.HideRange(_inventory.RenderedCount, reservedCount);
            _layout.ScrollToNewest(true);
        }

        public void RefreshLayout()
        {
            int reservedCount = Mathf.Max(_inventory.RenderedCount, _inventory.PendingCount);
            _layout.LayoutReservedCards(reservedCount, false);
            _layout.ScrollToNewest(false);
        }

        private void ApplyDeferredGain(WheelHudSnapshot snapshot)
        {
            _inventory.StorePending(snapshot);
            RenderStoredCards(_inventory.RenderedCards, _inventory.RenderedCount);
            PrepareLandingLayout();
            ScheduleFallbackCommit();
        }

        private void ApplyImmediate(WheelHudSnapshot snapshot)
        {
            KillFallbackCommit();
            _inventory.ApplySnapshotImmediately(snapshot);
            RenderStoredCards(_inventory.RenderedCards, _inventory.RenderedCount);
        }

        private void ScheduleFallbackCommit()
        {
            KillFallbackCommit();
            _pendingFallbackTween = DOVirtual.DelayedCall(
                    WheelRewardPanelMotion.PendingFallbackCommitDelay,
                    CommitPendingNow,
                    false)
                .SetTarget(_tweenTarget)
                .SetUpdate(true);
        }

        public void CommitPendingNow()
        {
            KillFallbackCommit();
            WheelRewardPanelCommitResult commitResult = _inventory.CommitPending();
            if (!commitResult.HasCommit)
            {
                return;
            }

            WheelRewardPanelCommit commit = commitResult.Commit;
            _renderer.Render(_inventory.RenderedCards, _inventory.RenderedCount, false, -1);
            _renderer.PulseChangedCards(commit.OldCount, commit.ChangedIndex);
            _layout.LayoutReservedCards(_inventory.RenderedCount, false);
            _layout.ScrollToNewest(false);
        }

        private void RenderStoredCards(RewardInventoryEntry[] cards, int count)
        {
            _layout.LayoutReservedCards(count, false);
            _renderer.Render(cards, count, false, -1);
            _layout.ScrollToNewest(false);
        }

        private void KillFallbackCommit()
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
        }
    }
}
