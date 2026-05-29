using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>Gameplay → view snapshot publishing. Only flow and <see cref="WheelStatePublisher"/> should use this.</summary>
    public interface IWheelSnapshotPublisher
    {
        void RaiseZoneChanged(WheelZoneSnapshot snapshot);
        void RaiseHudState(WheelHudSnapshot snapshot);
        void RaiseSpinLanded(WheelSpinResult result);
        void RaiseOutcome(WheelOutcomeSnapshot snapshot);
    }
}
