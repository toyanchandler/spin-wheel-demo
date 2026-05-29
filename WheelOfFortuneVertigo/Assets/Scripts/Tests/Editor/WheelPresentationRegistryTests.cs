using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelPresentationRegistryTests
    {
        private WheelEventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _eventBus = new WheelEventBus();
        }

        [Test]
        public void LootChannel_ForwardsToRegisteredHandler()
        {
            var handler = new StubLootHandler { Landing = new Vector3(1f, 2f, 3f) };
            _eventBus.Presentation.Loot.Register(handler);

            handler.WasHeld = false;
            _eventBus.Presentation.Loot.HoldForArrival();
            Vector3 landing = _eventBus.Presentation.Loot.ResolveLandingWorldPosition("gold", 0, 1);
            _eventBus.Presentation.Loot.CommitPendingNow();

            Assert.IsTrue(handler.WasHeld);
            Assert.AreEqual(handler.Landing, landing);
            Assert.IsTrue(handler.WasCommitted);
        }

        [Test]
        public void Clear_RemovesLootHandler()
        {
            _eventBus.Presentation.Loot.Register(new StubLootHandler());
            _eventBus.Presentation.Clear();

            Assert.DoesNotThrow(() => _eventBus.Presentation.Loot.HoldForArrival());
        }

        private sealed class StubLootHandler : IWheelLootFlightHandler
        {
            public Vector3 Landing;
            public bool WasHeld;
            public bool WasCommitted;

            public void HoldForArrival() => WasHeld = true;

            public Vector3 ResolveLandingWorldPosition(string rewardId, int burstIndex, int burstCount) => Landing;

            public void CommitPendingNow() => WasCommitted = true;
        }
    }
}
