using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelHudUiHost : WheelUiHostBase
    {
        [SerializeField, RequiredSceneReference]
        private WheelZoneProgressView _zoneProgress;
        [SerializeField, RequiredSceneReference]
        private WheelMilestoneBadgesView _milestoneBadges;
        [SerializeField, RequiredSceneReference]
        private WheelZonePanelView _zonePanel;
        [SerializeField, RequiredSceneReference]
        private WheelZoneTextView _zoneText;
        [SerializeField, RequiredSceneReference]
        private WheelZoneTypeTextView _zoneTypeText;
        [SerializeField, RequiredSceneReference]
        private WheelRewardPanelView _lootPanel;
        [SerializeField, RequiredSceneReference]
        private WheelOutcomePopupView _outcomePopup;
        [SerializeField, RequiredSceneReference]
        private WheelExitConfirmationView _exitConfirmation;
        [SerializeField, RequiredSceneReference]
        private WheelRewardOpeningView _rewardOpening;
        [SerializeField, RequiredSceneReference]
        private WheelStatusTextView _statusText;
        [SerializeField, RequiredSceneReference]
        private WheelLeaveButtonAction _leaveButton;
        [SerializeField, RequiredSceneReference]
        private WheelSpinButtonAction _spinButton;
        [SerializeField, RequiredSceneReference]
        private WheelRestartButtonAction _restartButton;
        [SerializeField, RequiredSceneReference]
        private WheelRestartButtonAction _rewardOpeningRestartButton;

        protected override void BindChildren(WheelEventBus eventBus)
        {
            RequireAssigned(_zoneProgress, nameof(_zoneProgress)).Bind(eventBus);
            RequireAssigned(_milestoneBadges, nameof(_milestoneBadges)).Bind(eventBus);
            RequireAssigned(_zonePanel, nameof(_zonePanel)).Bind(eventBus);
            RequireAssigned(_zoneText, nameof(_zoneText)).Bind(eventBus);
            RequireAssigned(_zoneTypeText, nameof(_zoneTypeText)).Bind(eventBus);
            RequireAssigned(_lootPanel, nameof(_lootPanel)).Bind(eventBus);
            RequireAssigned(_outcomePopup, nameof(_outcomePopup)).Bind(eventBus, _lootPanel);
            RequireAssigned(_exitConfirmation, nameof(_exitConfirmation)).Bind(eventBus);
            RequireAssigned(_rewardOpening, nameof(_rewardOpening)).Bind(eventBus);
            RequireAssigned(_statusText, nameof(_statusText)).Bind(eventBus);
            RequireAssigned(_leaveButton, nameof(_leaveButton)).Bind(eventBus);
            RequireAssigned(_spinButton, nameof(_spinButton)).Bind(eventBus);
            RequireAssigned(_restartButton, nameof(_restartButton)).Bind(eventBus);
            RequireAssigned(_rewardOpeningRestartButton, nameof(_rewardOpeningRestartButton)).Bind(eventBus);
        }

        protected override void UnbindChildren()
        {
            _rewardOpeningRestartButton.Unbind();
            _restartButton.Unbind();
            _spinButton.Unbind();
            _leaveButton.Unbind();
            _statusText.Unbind();
            _rewardOpening.Unbind();
            _exitConfirmation.Unbind();
            _outcomePopup.Unbind();
            _lootPanel.Unbind();
            _zoneTypeText.Unbind();
            _zoneText.Unbind();
            _zonePanel.Unbind();
            _milestoneBadges.Unbind();
            _zoneProgress.Unbind();
        }
    }
}
