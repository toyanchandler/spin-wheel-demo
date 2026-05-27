using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelSnapshotFactoryTests
    {
        private WheelGameSettings _settings;
        private WheelGameState _state;
        private GameObject _stateObject;

        [SetUp]
        public void SetUp()
        {
            _state = WheelTestFixtures.CreateState(out _settings);
            _stateObject = _state.gameObject;
        }

        [TearDown]
        public void TearDown()
        {
            WheelRuntimeLocator.Clear();
            Object.DestroyImmediate(_stateObject);
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void CreateZone_ReturnsDefensiveCopy_OfRuntimeSliceBuffer()
        {
            _state.PrepareCurrentZone();

            WheelZoneSnapshot snapshot = WheelSnapshotFactory.CreateZone(_state);
            bool originalBombFlag = snapshot.Slices[0].IsBomb;

            snapshot.Slices[0].ApplySlot(
                !originalBombFlag,
                snapshot.Slices[0].Reward,
                null,
                99,
                Color.magenta,
                "Mutated",
                true);

            WheelZoneSnapshot refreshedSnapshot = WheelSnapshotFactory.CreateZone(_state);

            Assert.AreEqual(originalBombFlag, refreshedSnapshot.Slices[0].IsBomb);
            Assert.AreNotEqual("Mutated", refreshedSnapshot.Slices[0].DisplayLabel);
            Assert.AreNotEqual(99, refreshedSnapshot.Slices[0].DisplayAmount);
        }

        [Test]
        public void CreateHud_ReflectsGameplayGuards_FromState()
        {
            WheelHudSnapshot readySnapshot = WheelSnapshotFactory.CreateHud(_state);

            Assert.IsTrue(readySnapshot.Actions.CanSpin);
            Assert.IsFalse(readySnapshot.Actions.CanLeave);
            Assert.AreEqual(1, readySnapshot.Zone);
            Assert.AreEqual(WheelGamePhase.Ready, readySnapshot.Phase);

            _state.BeginSpin();
            WheelHudSnapshot spinningSnapshot = WheelSnapshotFactory.CreateHud(_state);

            Assert.IsFalse(spinningSnapshot.Actions.CanSpin);
            Assert.AreEqual(WheelGamePhase.Spinning, spinningSnapshot.Phase);
        }

        [Test]
        public void CreateOutcome_UsesInventorySummary_ForCashedOutPhase()
        {
            WheelSpinResult reward = WheelTestFixtures.CreateRewardResult(0, "gold", "Gold", 2);
            _state.Resolve(reward);

            WheelOutcomeSnapshot snapshot = WheelSnapshotFactory.CreateOutcome(
                WheelGamePhase.CashedOut,
                reward,
                hasResult: false,
                _state.Inventory,
                _settings);

            Assert.AreEqual("Gold x2", snapshot.ResultText);
        }
    }
}
