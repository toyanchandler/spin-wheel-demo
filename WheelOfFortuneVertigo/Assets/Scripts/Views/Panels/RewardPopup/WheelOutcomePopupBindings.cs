using System;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupBindings : MonoBehaviour
    {
        [SerializeField] private WheelOutcomePopupRootBinding _root;
        [SerializeField] private WheelOutcomePopupContentRootBinding _contentRoot;
        [SerializeField] private WheelOutcomePopupIconBinding _icon;
        [SerializeField] private WheelOutcomePopupResultTextBinding _resultText;
        [SerializeField] private WheelOutcomePopupChromeBinding _chrome;
        [SerializeField] private WheelOutcomePopupRewardBackgroundBinding _rewardPopupBackground;
        [SerializeField] private WheelOutcomePopupBombCardShadowBinding _bombCardShadow;
        [SerializeField] private WheelOutcomePopupRetryButtonBinding _outcomeRetryButton;
        [SerializeField] private WheelOutcomePopupFlashBinding _flash;
        [SerializeField] private WheelOutcomePopupShineBinding _shine;
        [SerializeField] private WheelOutcomePopupFlightIconBinding _flightIcon;
        [SerializeField] private WheelOutcomePopupRewardBurstCameraBinding _rewardBurstCamera;
        [SerializeField] private WheelOutcomePopupRewardBurstDisplayBinding _rewardBurstDisplay;
        [SerializeField] private WheelOutcomePopupRewardBurstParticleBinding _rewardBurstParticle;

        public void Validate()
        {
            WheelBindingValidation.Require(this, _root, nameof(_root), "popup");
            WheelBindingValidation.Require(this, _contentRoot, nameof(_contentRoot), "popup");
            WheelBindingValidation.Require(this, _icon, nameof(_icon), "popup");
            WheelBindingValidation.Require(this, _resultText, nameof(_resultText), "popup");
            WheelBindingValidation.Require(this, _chrome, nameof(_chrome), "popup");
            WheelBindingValidation.Require(this, _rewardPopupBackground, nameof(_rewardPopupBackground), "popup");
            WheelBindingValidation.Require(this, _bombCardShadow, nameof(_bombCardShadow), "popup");
            WheelBindingValidation.Require(this, _outcomeRetryButton, nameof(_outcomeRetryButton), "popup");
            WheelBindingValidation.Require(this, _flash, nameof(_flash), "popup");
            WheelBindingValidation.Require(this, _shine, nameof(_shine), "popup");
            WheelBindingValidation.Require(this, _flightIcon, nameof(_flightIcon), "popup");
            WheelBindingValidation.Require(this, _rewardBurstCamera, nameof(_rewardBurstCamera), "popup");
            WheelBindingValidation.Require(this, _rewardBurstDisplay, nameof(_rewardBurstDisplay), "popup");
            WheelBindingValidation.Require(this, _rewardBurstParticle, nameof(_rewardBurstParticle), "popup");
        }

        public void CaptureHome()
        {
            _contentRoot.CaptureHome();
            _icon.CaptureHome();
            _flightIcon.CaptureHome();
        }

        internal WheelOutcomePopupRefs CreateRefs(
            WheelRewardPanelView rewardPanelView,
            Func<WheelOutcomeSnapshot> getCurrentSnapshot,
            Action markPresentationComplete)
        {
            return new WheelOutcomePopupRefs(
                _root,
                _contentRoot,
                _icon,
                _resultText,
                _chrome,
                _rewardPopupBackground,
                _bombCardShadow,
                _outcomeRetryButton,
                _flash,
                _shine,
                _flightIcon,
                _rewardBurstCamera,
                _rewardBurstDisplay,
                _rewardBurstParticle,
                rewardPanelView,
                getCurrentSnapshot,
                markPresentationComplete);
        }
    }
}
