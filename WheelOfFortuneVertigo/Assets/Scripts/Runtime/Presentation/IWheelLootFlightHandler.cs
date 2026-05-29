using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Loot strip contract for outcome-popup reward flight (hold landing slot, resolve world position, commit HUD).
    /// Implemented by <c>WheelRewardPanelView</c>. Animation sequencing lives in
    /// <c>WheelOutcomePopupRewardFlight</c>, which calls this API through <see cref="WheelLootPresentationChannel"/>.
    /// </summary>
    public interface IWheelLootFlightHandler
    {
        void HoldForArrival();
        Vector3 ResolveLandingWorldPosition(string rewardId, int burstIndex, int burstCount);
        void CommitPendingNow();
    }
}
