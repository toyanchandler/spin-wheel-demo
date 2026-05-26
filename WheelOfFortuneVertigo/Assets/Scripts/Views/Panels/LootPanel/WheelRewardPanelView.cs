using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelRewardPanelView : MonoBehaviour
    {
        [SerializeField] private Transform _cardPoolRoot;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelLootCardView[] _cardViews = Array.Empty<WheelLootCardView>();

        [WheelInject] private WheelEventBus _eventBus;

        private WheelRewardPanelInventoryState _inventory;
        private WheelRewardPanelRenderer _renderer;
        private WheelRewardPanelLayout _layout;
        private WheelRewardPanelLandingResolver _landingResolver;
        private Tween _pendingFallbackTween;

        [WheelAfterInject]
        private void Connect()
        {
            RequireSceneWiring();
            BuildRuntimeParts();
            _layout.LayoutReservedCards(0, false);
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            StopTweens();
        }

        private void OnDisable()
        {
            StopTweens();
        }

        public void RefreshResponsiveLayout()
        {
            _layout.LayoutReservedCards(Mathf.Max(_inventory.RenderedCount, _inventory.PendingCount), false);
            _layout.ScrollToNewest(false);
        }

        public void HoldPendingRewardsForArrival()
        {
            KillFallbackCommit();
        }

        public void CommitPendingRewardsNow()
        {
            KillFallbackCommit();
            CommitPendingNow();
        }

        public Vector3 ResolveLandingWorldPosition(string rewardId, int burstIndex, int burstCount)
        {
            int index = _inventory.ResolveLandingIndex(rewardId);
            _layout.LayoutReservedCards(Mathf.Max(_inventory.RenderedCount, _inventory.PendingCount), false);
            _layout.ScrollToNewest(false);
            return _landingResolver.Resolve(index, burstIndex, burstCount, transform.position);
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            WheelHudRewardCardsSnapshot rewards = snapshot.Rewards;
            _renderer.SetPresentation(rewards.RewardCardFrameSprite, rewards.DefaultRewardTitle);
            if (_inventory.ShouldDeferRewardGain(snapshot))
            {
                _inventory.StorePending(snapshot);
                RenderStoredCards(_inventory.RenderedCards, _inventory.RenderedCount);
                PreparePendingLandingSpace();
                ScheduleFallbackCommit();
                return;
            }

            ApplySnapshotImmediately(snapshot);
        }

        private void ApplySnapshotImmediately(WheelHudSnapshot snapshot)
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
                .SetTarget(this)
                .SetUpdate(true);
        }

        private void CommitPendingNow()
        {
            if (!_inventory.CommitPending(out WheelRewardPanelCommit commit))
            {
                return;
            }

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

        private void PreparePendingLandingSpace()
        {
            int reservedCount = Mathf.Max(_inventory.RenderedCount, _inventory.PendingCount);
            _layout.LayoutReservedCards(reservedCount, true);
            _renderer.HideRange(_inventory.RenderedCount, reservedCount);
            _layout.ScrollToNewest(true);
        }

        private void BuildRuntimeParts()
        {
            ScrollRect scrollRect = GetComponent<ScrollRect>();
            RectTransform contentRect = _cardPoolRoot as RectTransform;
            RectTransform viewportRect = ResolveViewport(scrollRect, contentRect);
            if (scrollRect == null || contentRect == null || viewportRect == null)
            {
                throw new InvalidOperationException(
                    name + " reward panel scroll wiring is incomplete. Rebuild the baked hierarchy and collect children.");
            }

            _inventory = new WheelRewardPanelInventoryState(_cardViews.Length);
            _renderer = new WheelRewardPanelRenderer(_cardViews);
            _layout = new WheelRewardPanelLayout(_cardViews, scrollRect, contentRect, viewportRect, this);
            _landingResolver = new WheelRewardPanelLandingResolver(_cardViews);
        }

        private RectTransform ResolveViewport(ScrollRect scrollRect, RectTransform contentRect)
        {
            if (scrollRect != null && scrollRect.viewport != null)
            {
                return scrollRect.viewport;
            }

            return contentRect != null ? contentRect.parent as RectTransform : null;
        }

        private void KillFallbackCommit()
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
        }

        private void StopTweens()
        {
            KillFallbackCommit();
            _layout?.StopScroll();
        }

        private void RequireSceneWiring()
        {
            if (_cardPoolRoot == null || _cardViews == null || _cardViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no reward cards. Collect children in the inspector or rebuild the scene.");
            }
        }
    }
}
