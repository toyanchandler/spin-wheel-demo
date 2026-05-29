using System;
using NUnit.Framework;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelZoneTypeCatalogTests
    {
        private WheelTestObjectScope _scope;
        private WheelGameSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _scope = new WheelTestObjectScope();
            _settings = WheelTestFixtures.CreateSettings(_scope);
        }

        [TearDown]
        public void TearDown()
        {
            _scope.DestroyAll();
        }

        [Test]
        public void GetZoneGameplay_ReturnsExpectedProfiles_ForEachZoneType()
        {
            WheelZoneGameplayProfile standard = _settings.GetZoneGameplay(ZoneType.Standard);
            WheelZoneGameplayProfile safe = _settings.GetZoneGameplay(ZoneType.Safe);
            WheelZoneGameplayProfile super = _settings.GetZoneGameplay(ZoneType.Super);

            Assert.IsTrue(standard.IncludesBombSlot);
            Assert.IsFalse(safe.IncludesBombSlot);
            Assert.IsFalse(super.IncludesBombSlot);
        }

        [Test]
        public void GetRewardPool_ReturnsDistinctPools_ForEachZoneType()
        {
            Assert.AreNotSame(
                _settings.GetRewardPool(ZoneType.Standard),
                _settings.GetRewardPool(ZoneType.Safe));
            Assert.AreEqual(1, _settings.GetRewardPool(ZoneType.Standard).Count);
            Assert.AreEqual(1, _settings.GetRewardPool(ZoneType.Safe).Count);
            Assert.AreEqual(1, _settings.GetRewardPool(ZoneType.Super).Count);
        }

        [Test]
        public void GetZoneGameplay_Throws_ForInvalidEnumValue()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _settings.GetZoneGameplay((ZoneType)99));
        }
    }
}
