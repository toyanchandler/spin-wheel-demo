using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Projects <see cref="WheelGameState"/> onto the bus as read-only snapshots.
    ///
    /// Publish matrix (see ARCHITECTURE_AND_LOGIC.md §9):
    /// - PublishZone  → wheel slices / skin
    /// - PublishHud   → buttons, status, loot panel, milestones
    /// - PublishOutcome → result popup
    /// - PublishAll   → zone + hud (restart, win refresh)
    ///
    /// Only <see cref="WheelGameFlowController"/> and session boot should drive publish order.
    /// </summary>
    public sealed class WheelStatePublisher : MonoBehaviour
    {
        private IWheelSnapshotPublisher _snapshots;
        private WheelGameState _state;

        public void Bind(WheelEventBus eventBus, WheelGameState state)
        {
            _snapshots = eventBus;
            _state = state;
        }

        public void Unbind()
        {
            _snapshots = null;
            _state = null;
        }

        public void PublishAll()
        {
            if (!TryGetBound(out IWheelSnapshotPublisher snapshots, out WheelGameState state))
            {
                return;
            }

            PublishZone(snapshots, state);
            PublishHud(snapshots, state);
        }

        public void PublishZone()
        {
            if (!TryGetBound(out IWheelSnapshotPublisher snapshots, out WheelGameState state))
            {
                return;
            }

            PublishZone(snapshots, state);
        }

        public void PublishHud()
        {
            if (!TryGetBound(out IWheelSnapshotPublisher snapshots, out WheelGameState state))
            {
                return;
            }

            PublishHud(snapshots, state);
        }

        public void PublishOutcome(WheelSpinResult result, bool hasResult)
        {
            if (!TryGetBound(out IWheelSnapshotPublisher snapshots, out WheelGameState state))
            {
                return;
            }

            snapshots.RaiseOutcome(WheelSnapshotFactory.CreateOutcome(
                state.Phase,
                result,
                hasResult,
                state.Inventory,
                state.Settings));
        }

        private bool TryGetBound(out IWheelSnapshotPublisher snapshots, out WheelGameState state)
        {
            snapshots = _snapshots;
            state = _state;
            return snapshots != null && state != null;
        }

        private static void PublishZone(IWheelSnapshotPublisher snapshots, WheelGameState state)
        {
            snapshots.RaiseZoneChanged(WheelSnapshotFactory.CreateZone(state));
        }

        private static void PublishHud(IWheelSnapshotPublisher snapshots, WheelGameState state)
        {
            snapshots.RaiseHudState(WheelSnapshotFactory.CreateHud(state));
        }
    }
}
