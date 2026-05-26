using System;
using DG.Tweening;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelRewardOpeningView : MonoBehaviour
    {
        [WheelInject] private WheelEventBus _eventBus;
        [WheelInject] private WheelRewardOpeningBindings _bindings;

        private Tween _visibilityTween;
        private bool _isVisible;
        private WheelRewardOpeningRootBinding _rootBinding;
        private WheelRewardOpeningDeckRenderer _deckRenderer;
        private WheelRewardOpeningScroller _scroller;
        private WheelRewardOpeningBurst _burst;

        [WheelAfterInject]
        private void Connect()
        {
            RequireSceneWiring();
            BuildRuntimeParts();
            _bindings.AddNavigationListeners(ScrollPrevious, ScrollNext);
            _eventBus.HudStateChanged += OnHudStateChanged;
            _rootBinding.Deactivate();
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _bindings.RemoveNavigationListeners(ScrollPrevious, ScrollNext);
            StopTweens();
        }

        private void OnDisable()
        {
            StopTweens();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            if (snapshot.Phase != WheelGamePhase.CashedOut)
            {
                HideOpening();
                return;
            }

            ShowOpening();
            _rootBinding.SetTitle(snapshot.RewardOpeningTitle);
            RenderCards(snapshot);
        }

        private void ShowOpening()
        {
            if (_isVisible)
            {
                return;
            }

            _isVisible = true;
            WheelRewardOpeningAnimator.Show(_rootBinding, this, ref _visibilityTween);
        }

        private void HideOpening()
        {
            if (!_isVisible)
            {
                _rootBinding.Deactivate();
                return;
            }

            _isVisible = false;
            _scroller.Stop();
            _burst.Stop();
            WheelRewardOpeningAnimator.Hide(_rootBinding, this, ref _visibilityTween);
        }

        private void RenderCards(WheelHudSnapshot snapshot)
        {
            if (snapshot.RewardCardCount > _bindings.CardCount)
            {
                throw new InvalidOperationException(
                    name + " needs " + snapshot.RewardCardCount + " baked reward cards, but only " + _bindings.CardCount + " are wired.");
            }

            int visibleCount = Mathf.Min(snapshot.RewardCardCount, _bindings.CardCount);
            _scroller.ApplyBounds(visibleCount);
            _deckRenderer.Render(snapshot, visibleCount);
            _scroller.FocusCardIndex(visibleCount - 1);
            _scroller.UpdateControls();
            _scroller.PlayReverseReveal(visibleCount);
            _burst.Play(snapshot.RewardCardCount);
        }

        private void ScrollPrevious()
        {
            _scroller.ScrollBy(-1f);
        }

        private void ScrollNext()
        {
            _scroller.ScrollBy(1f);
        }

        private void BuildRuntimeParts()
        {
            _rootBinding = _bindings.CreateRootBinding();
            _deckRenderer = _bindings.CreateDeckRenderer();
            _scroller = _bindings.CreateScroller(this);
            _burst = _bindings.CreateBurst(this);
        }

        private void StopTweens()
        {
            _visibilityTween?.Kill();
            _visibilityTween = null;
            _scroller?.Stop();
            _burst?.Stop();
        }

        private void RequireSceneWiring()
        {
            if (_eventBus == null || _bindings == null)
            {
                throw new InvalidOperationException(
                    name + " reward opening dependencies are incomplete. Add WheelRewardOpeningBindings to the opening overlay.");
            }

            _bindings.Validate();
        }
    }
}
