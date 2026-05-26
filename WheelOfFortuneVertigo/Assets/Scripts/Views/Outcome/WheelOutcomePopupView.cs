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
        [WheelInject(Optional = true)] private WheelZoneProgressView _zoneProgress;
        [WheelInject] private WheelOutcomePopupRootBinding _rootBinding;
        [WheelInject] private WheelOutcomePopupContentRootBinding _contentRootBinding;
        [WheelInject] private WheelOutcomePopupIconBinding _iconBinding;
        [WheelInject] private WheelOutcomePopupTitleTextBinding _titleTextBinding;
        [WheelInject] private WheelOutcomePopupResultTextBinding _resultTextBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupSummaryTextBinding _summaryTextBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupCenterAnchorBinding _centerAnchorBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupFitAreaBinding _fitAreaBinding;
        [WheelInject] private WheelOutcomePopupChromeBinding _chromeBinding;
        [WheelInject] private WheelOutcomePopupBombBackgroundBinding _bombBackgroundBinding;
        [WheelInject] private WheelOutcomePopupRewardBackgroundBinding _rewardBackgroundBinding;
        [WheelInject] private WheelOutcomePopupBombCardShadowBinding _bombCardShadowBinding;
        [WheelInject] private WheelOutcomePopupBombOuterStrokeBinding _bombOuterStrokeBinding;
        [WheelInject] private WheelOutcomePopupBombWarmTopGlowBinding _bombWarmTopGlowBinding;
        [WheelInject(Optional = true)] private WheelOutcomePopupBombHaloBinding _bombHaloBinding;
        [WheelInject] private WheelOutcomePopupRetryButtonBinding _retryButtonBinding;
        [WheelInject] private WheelOutcomePopupExitButtonBinding _exitButtonBinding;
        [WheelInject] private WheelOutcomePopupFlashBinding _flashBinding;
        [WheelInject] private WheelOutcomePopupShineBinding _shineBinding;
        [WheelInject] private WheelOutcomePopupFlightIconBinding _flightIconBinding;
        [WheelInject] private WheelOutcomePopupRewardBurstCameraBinding _rewardBurstCameraBinding;
        [WheelInject] private WheelOutcomePopupRewardBurstDisplayBinding _rewardBurstDisplayBinding;
        [WheelInject] private WheelOutcomePopupRewardBurstParticleBinding _rewardBurstParticleBinding;

        private WheelOutcomePopupPresenter _presenter;
        private Image[] _flightIconPool = Array.Empty<Image>();
        private Vector2 _iconHomeAnchoredPosition;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _flightIconPool = new[] { _flightIconBinding.Image };
            _iconHomeAnchoredPosition = _iconBinding.RectTransform.anchoredPosition;
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
            if (_presenter != null)
            {
                _presenter.Reset();
                _presenter = null;
            }
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
                || _titleTextBinding == null || _resultTextBinding == null || _chromeBinding == null
                || _bombBackgroundBinding == null || _rewardBackgroundBinding == null
                || _bombCardShadowBinding == null || _bombOuterStrokeBinding == null
                || _bombWarmTopGlowBinding == null || _retryButtonBinding == null
                || _exitButtonBinding == null || _flashBinding == null || _shineBinding == null
                || _flightIconBinding == null || _rewardBurstCameraBinding == null
                || _rewardBurstDisplayBinding == null || _rewardBurstParticleBinding == null)
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
                TitleText = _titleTextBinding.Text,
                ResultText = _resultTextBinding.Text,
                SummaryText = _summaryTextBinding != null ? _summaryTextBinding.Text : null,
                CanvasGroup = _rootBinding.CanvasGroup,
                ContentRoot = _contentRootBinding.RectTransform,
                RewardChromeGroup = _chromeBinding.CanvasGroup,
                BombPopupBackground = _bombBackgroundBinding.Target,
                RewardPopupBackground = _rewardBackgroundBinding.Target,
                BombCardShadow = _bombCardShadowBinding.Target,
                BombOuterStroke = _bombOuterStrokeBinding.Target,
                BombWarmTopGlow = _bombWarmTopGlowBinding.Target,
                BombHalo = _bombHaloBinding != null ? _bombHaloBinding.Target : null,
                OutcomeRetryButton = _retryButtonBinding.Target,
                OutcomeExitButton = _exitButtonBinding.Target,
                FlashImage = _flashBinding.Image,
                ShineImage = _shineBinding.Image,
                FlightIconPool = _flightIconPool,
                RewardBurstCamera = _rewardBurstCameraBinding.Camera,
                RewardBurstDisplay = _rewardBurstDisplayBinding.RawImage,
                RewardBurstParticle = _rewardBurstParticleBinding.ParticleSystem,
                RewardBurstRenderer = _rewardBurstParticleBinding.Renderer,
                RewardPanelView = _rewardPanelView,
                IconHomeAnchoredPosition = _iconHomeAnchoredPosition,
                PopupCenterAnchor = _centerAnchorBinding != null ? _centerAnchorBinding.RectTransform : null,
                PopupFitArea = _fitAreaBinding != null ? _fitAreaBinding.RectTransform : null,
                ZoneProgress = _zoneProgress,
                GetCurrentSnapshot = () => _presenter != null ? _presenter.CurrentSnapshot : default,
                MarkPresentationComplete = () =>
                {
                    if (_presenter != null)
                    {
                        _presenter.MarkPresentationComplete();
                    }
                },
            };
        }
    }
}
