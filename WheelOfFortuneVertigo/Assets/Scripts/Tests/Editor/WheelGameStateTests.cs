using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelGameStateTests
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
        public void Restart_ResetsZoneInventoryAndPhase()
        {
            _state.Resolve(WheelTestFixtures.CreateRewardResult(0, "gold", "Gold", 2));

            _state.Restart();

            Assert.AreEqual(1, _state.Zone);
            Assert.AreEqual(WheelGamePhase.Ready, _state.Phase);
            Assert.AreEqual(0, _state.Inventory.Count);
            Assert.IsFalse(_state.HasLastResult);
        }

        [Test]
        public void Resolve_WinResult_AdvancesZoneAndAddsInventory()
        {
            WheelSpinResult reward = WheelTestFixtures.CreateRewardResult(0, "gold", "Gold", 2);

            _state.Resolve(reward);

            Assert.AreEqual(WheelGamePhase.Won, _state.Phase);
            Assert.AreEqual(2, _state.Zone);
            Assert.AreEqual(1, _state.Inventory.Count);
            Assert.IsTrue(_state.CanLeave);
        }

        [Test]
        public void Resolve_BombResult_ClearsInventoryAndBlocksLeave()
        {
            _state.Resolve(WheelTestFixtures.CreateRewardResult(0, "gold", "Gold", 2));
            _state.Resolve(WheelTestFixtures.CreateBombResult(1));

            Assert.AreEqual(WheelGamePhase.Bombed, _state.Phase);
            Assert.AreEqual(0, _state.Inventory.Count);
            Assert.IsFalse(_state.CanLeave);
            Assert.IsTrue(_state.CanRestart);
        }

        [Test]
        public void CreateSpinResult_UsesCurrentSliceBuffer()
        {
            _state.PrepareCurrentZone();
            int bombIndex = _state.FindFirstSliceIndex(true);

            WheelSpinResult result = _state.CreateSpinResult(bombIndex);

            Assert.AreEqual(bombIndex, result.SliceIndex);
            Assert.IsTrue(result.IsBomb);
        }

        [Test]
        public void CopySliceDefinitions_DoesNotExposeLiveBufferReference()
        {
            _state.PrepareCurrentZone();

            var destination = new WheelSliceDefinition[_state.SliceCount];
            for (int i = 0; i < destination.Length; i++)
            {
                destination[i] = new WheelSliceDefinition();
            }

            _state.CopySliceDefinitions(destination);
            string originalLabel = destination[0].DisplayLabel;
            destination[0].ApplySlot(true, destination[0].Reward, null, 99, Color.red, "Mutated", false);

            WheelSpinResult result = _state.CreateSpinResult(0);

            Assert.AreNotEqual("Mutated", result.Label);
            Assert.AreEqual(originalLabel, result.Label);
        }

        [Test]
        public void CashOut_MovesPhaseToCashedOut()
        {
            _state.Resolve(WheelTestFixtures.CreateRewardResult(0, "gold", "Gold", 1));

            _state.CashOut();

            Assert.AreEqual(WheelGamePhase.CashedOut, _state.Phase);
            Assert.IsTrue(_state.CanRestart);
        }
    }
}
