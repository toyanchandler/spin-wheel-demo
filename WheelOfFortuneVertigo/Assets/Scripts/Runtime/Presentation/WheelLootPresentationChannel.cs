using UnityEngine;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelLootPresentationChannel
    {
        private IWheelLootFlightHandler _handler;

        public void Register(IWheelLootFlightHandler handler)
        {
            _handler = handler;
        }

        public void Clear()
        {
            _handler = null;
        }

        public void HoldForArrival()
        {
            _handler?.HoldForArrival();
        }

        public Vector3 ResolveLandingWorldPosition(string rewardId, int burstIndex, int burstCount)
        {
            return _handler == null
                ? Vector3.zero
                : _handler.ResolveLandingWorldPosition(rewardId, burstIndex, burstCount);
        }

        public void CommitPendingNow()
        {
            _handler?.CommitPendingNow();
        }
    }
}
