using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    /// <summary>
    /// Loot strip lifecycle. HUD logic: <see cref="WheelRewardPanelHudCoordinator"/>.
    /// Registers <see cref="IWheelLootFlightHandler"/> on the bus for outcome-popup flight.
    /// </summary>
    [WheelBind]
    public sealed class WheelRewardPanelView : MonoBehaviour, IWheelLootFlightHandler
    {
        [SerializeField] private Transform _cardPoolRoot;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelLootCardView[] _cardViews = Array.Empty<WheelLootCardView>();

        [WheelInject] private WheelEventBus _eventBus;

        private WheelRewardPanelHudCoordinator _hudCoordinator;
        private WheelRewardPanelLandingResolver _landingResolver;

        [WheelAfterInject]
        private void Connect()
        {
            RequireSceneWiring();
            BuildRuntimeParts();
            _eventBus.Presentation.Loot.Register(this);
            _hudCoordinator.RefreshLayout();
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.Presentation.Loot.Clear();
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _hudCoordinator.Stop();
        }

        private void OnDisable()
        {
            _hudCoordinator?.Stop();
        }

        public void RefreshResponsiveLayout()
        {
            _hudCoordinator?.RefreshLayout();
        }

        public void HoldForArrival()
        {
            _hudCoordinator?.HoldForArrival();
        }

        public void CommitPendingNow()
        {
            _hudCoordinator?.CommitPendingNow();
        }

        public Vector3 ResolveLandingWorldPosition(string rewardId, int burstIndex, int burstCount)
        {
            int index = _hudCoordinator.ResolveLandingIndex(rewardId);
            _hudCoordinator.RefreshLayout();
            return _landingResolver.Resolve(index, burstIndex, burstCount, transform.position);
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _hudCoordinator.HandleHudSnapshot(snapshot);
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

            var inventory = new WheelRewardPanelInventoryState(_cardViews.Length);
            var renderer = new WheelRewardPanelRenderer(_cardViews);
            var layout = new WheelRewardPanelLayout(_cardViews, scrollRect, contentRect, viewportRect, this);
            _hudCoordinator = new WheelRewardPanelHudCoordinator(inventory, renderer, layout, this);
            _landingResolver = new WheelRewardPanelLandingResolver(_cardViews);
        }

        private static RectTransform ResolveViewport(ScrollRect scrollRect, RectTransform contentRect)
        {
            if (scrollRect != null && scrollRect.viewport != null)
            {
                return scrollRect.viewport;
            }

            return contentRect != null ? contentRect.parent as RectTransform : null;
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
