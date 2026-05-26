using System;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupRefs
    {
        public WheelOutcomePopupRootBinding Root { get; }
        public WheelOutcomePopupContentRootBinding ContentRoot { get; }
        public WheelOutcomePopupIconBinding Icon { get; }
        public WheelOutcomePopupResultTextBinding ResultText { get; }
        public WheelOutcomePopupChromeBinding Chrome { get; }
        public WheelOutcomePopupRewardBackgroundBinding RewardPopupBackground { get; }
        public WheelOutcomePopupBombCardShadowBinding BombCardShadow { get; }
        public WheelOutcomePopupRetryButtonBinding OutcomeRetryButton { get; }
        public WheelOutcomePopupFlashBinding Flash { get; }
        public WheelOutcomePopupShineBinding Shine { get; }
        public WheelOutcomePopupFlightIconBinding FlightIcon { get; }
        public WheelOutcomePopupRewardBurstCameraBinding RewardBurstCamera { get; }
        public WheelOutcomePopupRewardBurstDisplayBinding RewardBurstDisplay { get; }
        public WheelOutcomePopupRewardBurstParticleBinding RewardBurstParticle { get; }
        public WheelRewardPanelView RewardPanelView { get; }
        public Func<WheelOutcomeSnapshot> GetCurrentSnapshot { get; }
        public Action MarkPresentationComplete { get; }

        public WheelOutcomePopupRefs(
            WheelOutcomePopupRootBinding root,
            WheelOutcomePopupContentRootBinding contentRoot,
            WheelOutcomePopupIconBinding icon,
            WheelOutcomePopupResultTextBinding resultText,
            WheelOutcomePopupChromeBinding chrome,
            WheelOutcomePopupRewardBackgroundBinding rewardPopupBackground,
            WheelOutcomePopupBombCardShadowBinding bombCardShadow,
            WheelOutcomePopupRetryButtonBinding outcomeRetryButton,
            WheelOutcomePopupFlashBinding flash,
            WheelOutcomePopupShineBinding shine,
            WheelOutcomePopupFlightIconBinding flightIcon,
            WheelOutcomePopupRewardBurstCameraBinding rewardBurstCamera,
            WheelOutcomePopupRewardBurstDisplayBinding rewardBurstDisplay,
            WheelOutcomePopupRewardBurstParticleBinding rewardBurstParticle,
            WheelRewardPanelView rewardPanelView,
            Func<WheelOutcomeSnapshot> getCurrentSnapshot,
            Action markPresentationComplete)
        {
            Root = root;
            ContentRoot = contentRoot;
            Icon = icon;
            ResultText = resultText;
            Chrome = chrome;
            RewardPopupBackground = rewardPopupBackground;
            BombCardShadow = bombCardShadow;
            OutcomeRetryButton = outcomeRetryButton;
            Flash = flash;
            Shine = shine;
            FlightIcon = flightIcon;
            RewardBurstCamera = rewardBurstCamera;
            RewardBurstDisplay = rewardBurstDisplay;
            RewardBurstParticle = rewardBurstParticle;
            RewardPanelView = rewardPanelView;
            GetCurrentSnapshot = getCurrentSnapshot;
            MarkPresentationComplete = markPresentationComplete;
        }
    }
}
