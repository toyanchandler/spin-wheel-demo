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
            Require(_root, nameof(_root));
            Require(_contentRoot, nameof(_contentRoot));
            Require(_icon, nameof(_icon));
            Require(_resultText, nameof(_resultText));
            Require(_chrome, nameof(_chrome));
            Require(_rewardPopupBackground, nameof(_rewardPopupBackground));
            Require(_bombCardShadow, nameof(_bombCardShadow));
            Require(_outcomeRetryButton, nameof(_outcomeRetryButton));
            Require(_flash, nameof(_flash));
            Require(_shine, nameof(_shine));
            Require(_flightIcon, nameof(_flightIcon));
            Require(_rewardBurstCamera, nameof(_rewardBurstCamera));
            Require(_rewardBurstDisplay, nameof(_rewardBurstDisplay));
            Require(_rewardBurstParticle, nameof(_rewardBurstParticle));
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

        private void Require(UnityEngine.Object value, string fieldName)
        {
            if (value == null)
            {
                throw new InvalidOperationException(name + " requires popup binding " + fieldName + ".");
            }
        }
    }
}
