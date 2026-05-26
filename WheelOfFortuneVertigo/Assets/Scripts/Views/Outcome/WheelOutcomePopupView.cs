using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed partial class WheelOutcomePopupView : MonoBehaviour
    {
        [WheelInject] private WheelEventBus _eventBus;
        [WheelInject] private WheelRewardPanelView _rewardPanelView;
        [WheelInject] private WheelOutcomePopupRootBinding _rootBinding;
        [WheelInject] private WheelOutcomePopupContentRootBinding _contentRootBinding;
        [WheelInject] private WheelOutcomePopupIconBinding _iconBinding;
        [WheelInject] private WheelOutcomePopupResultTextBinding _resultTextBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupSummaryTextBinding _summaryTextBinding;
        [WheelInject] private WheelOutcomePopupChromeBinding _chromeBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupRewardBackgroundBinding _rewardBackgroundBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupBombCardShadowBinding _bombCardShadowBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupBombWarmTopGlowBinding _bombWarmTopGlowBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupBombHaloBinding _bombHaloBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupRetryButtonBinding _retryButtonBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupFlashBinding _flashBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupShineBinding _shineBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupFlightIconBinding _flightIconBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupRewardBurstCameraBinding _rewardBurstCameraBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupRewardBurstDisplayBinding _rewardBurstDisplayBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupRewardBurstParticleBinding _rewardBurstParticleBinding;

        private WheelOutcomePopupPresenter _presenter;
        private Image[] _flightIconPool = Array.Empty<Image>();
        private Vector2 _iconHomeAnchoredPosition;
        private Vector2 _contentHomeAnchorMin;
        private Vector2 _contentHomeAnchorMax;
        private Vector2 _contentHomePivot;
        private Vector2 _contentHomeAnchoredPosition;
        private Vector3 _contentHomeScale;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _flightIconPool = _flightIconBinding != null ? new[] { _flightIconBinding.Image } : Array.Empty<Image>();
            _iconHomeAnchoredPosition = _iconBinding.RectTransform.anchoredPosition;
            _contentHomeAnchorMin = _contentRootBinding.RectTransform.anchorMin;
            _contentHomeAnchorMax = _contentRootBinding.RectTransform.anchorMax;
            _contentHomePivot = _contentRootBinding.RectTransform.pivot;
            _contentHomeAnchoredPosition = _contentRootBinding.RectTransform.anchoredPosition;
            _contentHomeScale = _contentRootBinding.RectTransform.localScale;
            _presenter = new WheelOutcomePopupPresenter(CreateRefs(), this);
            _presenter.Reset();
            _eventBus.OutcomeResolved += OnOutcomeResolved;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.OutcomeResolved -= OnOutcomeResolved;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _presenter?.Reset();
            _presenter = null;
        }

        private void OnOutcomeResolved(WheelOutcomeSnapshot snapshot)
        {
            _presenter.HandleOutcome(snapshot);
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _presenter.HandleHud(snapshot.IsOutcomePopupAllowed);
        }

        private void ValidateWiring()
        {
            if (_rootBinding == null || _contentRootBinding == null || _iconBinding == null
                || _resultTextBinding == null || _chromeBinding == null
                || _eventBus == null || _rewardPanelView == null)
            {
                throw new InvalidOperationException(
                    name + " outcome popup scene bindings are incomplete. Add marker components to the popup hierarchy.");
            }
        }

        private WheelOutcomePopupRefs CreateRefs()
        {
            return new WheelOutcomePopupRefs
            {
                Root = _rootBinding.Root,
                IconImage = _iconBinding.Image,
                ResultText = _resultTextBinding.Text,
                SummaryText = _summaryTextBinding?.Text,
                CanvasGroup = _rootBinding.CanvasGroup,
                ContentRoot = _contentRootBinding.RectTransform,
                RewardChromeGroup = _chromeBinding.CanvasGroup,
                RewardPopupBackground = _rewardBackgroundBinding?.Target,
                BombCardShadow = _bombCardShadowBinding?.Target,
                BombWarmTopGlow = _bombWarmTopGlowBinding?.Target,
                BombHalo = _bombHaloBinding?.Target,
                OutcomeRetryButton = _retryButtonBinding?.Target,
                FlashImage = _flashBinding?.Image,
                ShineImage = _shineBinding?.Image,
                FlightIconPool = _flightIconPool,
                RewardBurstCamera = _rewardBurstCameraBinding?.Camera,
                RewardBurstDisplay = _rewardBurstDisplayBinding?.RawImage,
                RewardBurstParticle = _rewardBurstParticleBinding?.ParticleSystem,
                RewardBurstRenderer = _rewardBurstParticleBinding?.Renderer,
                RewardPanelView = _rewardPanelView,
                IconHomeAnchoredPosition = _iconHomeAnchoredPosition,
                ContentHomeAnchorMin = _contentHomeAnchorMin,
                ContentHomeAnchorMax = _contentHomeAnchorMax,
                ContentHomePivot = _contentHomePivot,
                ContentHomeAnchoredPosition = _contentHomeAnchoredPosition,
                ContentHomeScale = _contentHomeScale,
                GetCurrentSnapshot = () => _presenter?.CurrentSnapshot ?? default,
                MarkPresentationComplete = () => _presenter?.MarkPresentationComplete(),
            };
        }
    }
}
