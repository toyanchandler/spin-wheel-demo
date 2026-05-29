using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupHandleSet
    {
        public WheelOutcomePopupRootHandle Root { get; }
        public WheelOutcomePopupRectHandle ContentRoot { get; }
        public WheelOutcomePopupImageHandle Icon { get; }
        public WheelOutcomePopupTextHandle ResultText { get; }
        public WheelOutcomePopupChromeHandle Chrome { get; }
        public WheelOutcomePopupObjectHandle RewardPopupBackground { get; }
        public WheelOutcomePopupObjectHandle BombCardShadow { get; }
        public WheelOutcomePopupObjectHandle OutcomeRetryButton { get; }
        public WheelOutcomePopupImageHandle Flash { get; }
        public WheelOutcomePopupImageHandle Shine { get; }
        public WheelOutcomePopupImageHandle FlightIcon { get; }
        public WheelOutcomePopupComponentHandle<Camera> RewardBurstCamera { get; }
        public WheelOutcomePopupComponentHandle<RawImage> RewardBurstDisplay { get; }
        public WheelOutcomePopupBurstParticleHandle RewardBurstParticle { get; }

        private WheelOutcomePopupHandleSet(
            WheelOutcomePopupRootHandle root,
            WheelOutcomePopupRectHandle contentRoot,
            WheelOutcomePopupImageHandle icon,
            WheelOutcomePopupTextHandle resultText,
            WheelOutcomePopupChromeHandle chrome,
            WheelOutcomePopupObjectHandle rewardPopupBackground,
            WheelOutcomePopupObjectHandle bombCardShadow,
            WheelOutcomePopupObjectHandle outcomeRetryButton,
            WheelOutcomePopupImageHandle flash,
            WheelOutcomePopupImageHandle shine,
            WheelOutcomePopupImageHandle flightIcon,
            WheelOutcomePopupComponentHandle<Camera> rewardBurstCamera,
            WheelOutcomePopupComponentHandle<RawImage> rewardBurstDisplay,
            WheelOutcomePopupBurstParticleHandle rewardBurstParticle)
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
        }

        public static WheelOutcomePopupHandleSet FromWiring(Component owner, WheelOutcomePopupSceneWiring wiring)
        {
            ValidateWiring(owner, wiring);
            return new WheelOutcomePopupHandleSet(
                new WheelOutcomePopupRootHandle(wiring.RootCanvas, wiring.RootOverlay),
                new WheelOutcomePopupRectHandle(wiring.ContentRoot),
                new WheelOutcomePopupImageHandle(wiring.Icon),
                new WheelOutcomePopupTextHandle(wiring.ResultText),
                new WheelOutcomePopupChromeHandle(wiring.Chrome),
                new WheelOutcomePopupObjectHandle(wiring.RewardPopupBackground),
                new WheelOutcomePopupObjectHandle(wiring.BombCardShadow),
                new WheelOutcomePopupObjectHandle(wiring.OutcomeRetryButton),
                new WheelOutcomePopupImageHandle(wiring.Flash),
                new WheelOutcomePopupImageHandle(wiring.Shine),
                new WheelOutcomePopupImageHandle(wiring.FlightIcon),
                new WheelOutcomePopupComponentHandle<Camera>(wiring.RewardBurstCamera),
                new WheelOutcomePopupComponentHandle<RawImage>(wiring.RewardBurstDisplay),
                new WheelOutcomePopupBurstParticleHandle(wiring.RewardBurstParticle));
        }

        public void CaptureHome()
        {
            ContentRoot.CaptureHome();
            Icon.CaptureHome();
            FlightIcon.CaptureHome();
        }

        private static void ValidateWiring(Component owner, WheelOutcomePopupSceneWiring wiring)
        {
            const string panel = "outcome popup";
            WheelWiringValidation.Require(owner, panel, wiring.RootCanvas, nameof(wiring.RootCanvas));
            WheelWiringValidation.Require(owner, panel, wiring.ContentRoot, nameof(wiring.ContentRoot));
            WheelWiringValidation.Require(owner, panel, wiring.Icon, nameof(wiring.Icon));
            WheelWiringValidation.Require(owner, panel, wiring.ResultText, nameof(wiring.ResultText));
            WheelWiringValidation.Require(owner, panel, wiring.Chrome, nameof(wiring.Chrome));
            WheelWiringValidation.Require(owner, panel, wiring.RewardPopupBackground, nameof(wiring.RewardPopupBackground));
            WheelWiringValidation.Require(owner, panel, wiring.BombCardShadow, nameof(wiring.BombCardShadow));
            WheelWiringValidation.Require(owner, panel, wiring.OutcomeRetryButton, nameof(wiring.OutcomeRetryButton));
            WheelWiringValidation.Require(owner, panel, wiring.Flash, nameof(wiring.Flash));
            WheelWiringValidation.Require(owner, panel, wiring.Shine, nameof(wiring.Shine));
            WheelWiringValidation.Require(owner, panel, wiring.FlightIcon, nameof(wiring.FlightIcon));
            WheelWiringValidation.Require(owner, panel, wiring.RewardBurstCamera, nameof(wiring.RewardBurstCamera));
            WheelWiringValidation.Require(owner, panel, wiring.RewardBurstDisplay, nameof(wiring.RewardBurstDisplay));
            WheelWiringValidation.Require(owner, panel, wiring.RewardBurstParticle, nameof(wiring.RewardBurstParticle));
        }
    }
}
