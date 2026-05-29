using System;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupRefs
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
        public WheelOutcomePopupComponentHandle<UnityEngine.Camera> RewardBurstCamera { get; }
        public WheelOutcomePopupComponentHandle<UnityEngine.UI.RawImage> RewardBurstDisplay { get; }
        public WheelOutcomePopupBurstParticleHandle RewardBurstParticle { get; }
        public WheelLootPresentationChannel LootPresentation { get; }
        public Func<WheelOutcomeSnapshot> GetCurrentSnapshot { get; }
        public Action MarkPresentationComplete { get; }

        public static WheelOutcomePopupRefs From(
            WheelOutcomePopupHandleSet handles,
            WheelLootPresentationChannel lootPresentation,
            Func<WheelOutcomeSnapshot> getCurrentSnapshot,
            Action markPresentationComplete)
        {
            if (handles == null) throw new ArgumentNullException(nameof(handles));

            return new WheelOutcomePopupRefs(handles, lootPresentation, getCurrentSnapshot, markPresentationComplete);
        }

        private WheelOutcomePopupRefs(
            WheelOutcomePopupHandleSet handles,
            WheelLootPresentationChannel lootPresentation,
            Func<WheelOutcomeSnapshot> getCurrentSnapshot,
            Action markPresentationComplete)
        {
            Root = handles.Root;
            ContentRoot = handles.ContentRoot;
            Icon = handles.Icon;
            ResultText = handles.ResultText;
            Chrome = handles.Chrome;
            RewardPopupBackground = handles.RewardPopupBackground;
            BombCardShadow = handles.BombCardShadow;
            OutcomeRetryButton = handles.OutcomeRetryButton;
            Flash = handles.Flash;
            Shine = handles.Shine;
            FlightIcon = handles.FlightIcon;
            RewardBurstCamera = handles.RewardBurstCamera;
            RewardBurstDisplay = handles.RewardBurstDisplay;
            RewardBurstParticle = handles.RewardBurstParticle;
            LootPresentation = lootPresentation;
            GetCurrentSnapshot = getCurrentSnapshot;
            MarkPresentationComplete = markPresentationComplete;
        }
    }
}
