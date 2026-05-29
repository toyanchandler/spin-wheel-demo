using NUnit.Framework;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelSpinResolvePipelineTests
    {
        private WheelTestObjectScope _scope;
        private WheelGameSettings _settings;
        private WheelGameState _state;

        [SetUp]
        public void SetUp()
        {
            _scope = new WheelTestObjectScope();
            _state = WheelTestFixtures.CreateState(_scope, out _settings);
        }

        [TearDown]
        public void TearDown()
        {
            _scope.DestroyAll();
        }

        [Test]
        public void Apply_RewardResult_MatchesWinCatalogProfile()
        {
            WheelSpinResult reward = WheelTestFixtures.CreateRewardResult(_scope, 0, "gold", "Gold", 2);

            WheelSpinResolvePipeline.Apply(_state, reward);

            Assert.AreEqual(WheelGamePhase.Won, _state.Phase);
            Assert.AreEqual(2, _state.Zone);
            Assert.AreEqual(1, _state.Inventory.Count);
        }

        [Test]
        public void Apply_BombResult_MatchesBombCatalogProfile()
        {
            _state.Resolve(WheelTestFixtures.CreateRewardResult(_scope, 0, "gold", "Gold", 1));

            WheelSpinResolvePipeline.Apply(_state, WheelTestFixtures.CreateBombResult(_scope, 1));

            Assert.AreEqual(WheelGamePhase.Bombed, _state.Phase);
            Assert.AreEqual(0, _state.Inventory.Count);
        }
    }
}
