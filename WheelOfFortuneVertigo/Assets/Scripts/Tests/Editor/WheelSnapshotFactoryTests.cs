using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelSnapshotFactoryTests
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
            WheelRuntimeLocator.Clear();
            _scope.DestroyAll();
        }

        [Test]
        public void CreateZone_ReturnsIndependentPresentationArray_PerCall()
        {
            _state.PrepareCurrentZone();

            WheelZoneSnapshot first = WheelSnapshotFactory.CreateZone(_state);
            Assert.IsNotNull(first.SlicePresentations);
            Assert.AreEqual(first.SliceCount, first.SlicePresentations.Length);

            WheelSlicePresentation original = first.SlicePresentations[0];
            first.SlicePresentations[0] = default;

            WheelZoneSnapshot refreshed = WheelSnapshotFactory.CreateZone(_state);

            Assert.AreNotSame(first.SlicePresentations, refreshed.SlicePresentations);
            Assert.AreEqual(original.AmountText, refreshed.SlicePresentations[0].AmountText);
            Assert.AreEqual(original.ShowAmountLabel, refreshed.SlicePresentations[0].ShowAmountLabel);
            Assert.AreEqual(original.AmountColor, refreshed.SlicePresentations[0].AmountColor);
        }

        [Test]
        public void CreateZone_ResolvesSkinSprites_FromSettingsCatalog()
        {
            _state.PrepareCurrentZone();

            WheelZoneSnapshot snapshot = WheelSnapshotFactory.CreateZone(_state);

            Assert.IsNotNull(snapshot.WheelBaseSprite);
            Assert.IsNotNull(snapshot.IndicatorSprite);
            Assert.AreEqual(
                _settings.SkinCatalog.GetWheelBase(snapshot.SkinTier),
                snapshot.WheelBaseSprite);
            Assert.AreEqual(
                _settings.SkinCatalog.GetIndicator(snapshot.SkinTier),
                snapshot.IndicatorSprite);
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
            WheelSpinResult reward = WheelTestFixtures.CreateRewardResult(_scope, 0, "gold", "Gold", 2);
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
