using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelZoneTypeTableTests
    {
        [Test]
        public void Resolve_ReturnsFallback_WhenNoRuleMatches()
        {
            var rules = new[] { WheelZoneIntervalRule.Create(5, ZoneType.Safe) };

            Assert.AreEqual(ZoneType.Standard, WheelZoneTypeTable.Resolve(3, rules, ZoneType.Standard));
        }

        [Test]
        public void Resolve_ReturnsFirstMatchingRule_InArrayOrder()
        {
            var rules = new[]
            {
                WheelZoneIntervalRule.Create(5, ZoneType.Safe),
                WheelZoneIntervalRule.Create(10, ZoneType.Super)
            };

            Assert.AreEqual(ZoneType.Safe, WheelZoneTypeTable.Resolve(10, rules, ZoneType.Standard));
            Assert.AreEqual(ZoneType.Safe, WheelZoneTypeTable.Resolve(20, rules, ZoneType.Standard));
            Assert.AreEqual(ZoneType.Super, WheelZoneTypeTable.Resolve(10, new[] { WheelZoneIntervalRule.Create(10, ZoneType.Super) }, ZoneType.Standard));
        }
    }

    public sealed class WheelSliceQueriesTests
    {
        [Test]
        public void FindFirst_ReturnsIndex_WhenSliceMatches()
        {
            var slices = new[]
            {
                CreateSlice(false),
                CreateSlice(true),
                CreateSlice(false)
            };

            Assert.AreEqual(1, WheelSliceQueries.FindFirst(slices, slices.Length, true));
            Assert.AreEqual(0, WheelSliceQueries.FindFirst(slices, slices.Length, false));
        }

        [Test]
        public void FindFirst_ReturnsNegativeOne_WhenSliceDoesNotExist()
        {
            var slices = new[] { CreateSlice(false), CreateSlice(false) };

            Assert.AreEqual(-1, WheelSliceQueries.FindFirst(slices, slices.Length, true));
            Assert.IsFalse(WheelSliceQueries.ContainsBomb(slices, slices.Length));
        }

        [Test]
        public void ContainsBomb_ReturnsTrue_WhenBombSliceExists()
        {
            var slices = new[] { CreateSlice(false), CreateSlice(true) };

            Assert.IsTrue(WheelSliceQueries.ContainsBomb(slices, slices.Length));
        }

        private static WheelSliceDefinition CreateSlice(bool isBomb)
        {
            var reward = RewardDefinition.Create("reward", "Reward", null, 1, RewardTier.Common, Color.white);
            reward.CacheRuntimeText("Won {0}");

            var slice = new WheelSliceDefinition();
            slice.ApplySlot(isBomb, reward, null, 1, Color.white, "Reward", false);
            return slice;
        }
    }

    public sealed class WheelSliceSlotCatalogTests
    {
        [Test]
        public void RewardSlotsForZone_SubtractsBombSlot_WhenZoneIncludesBomb()
        {
            var zoneGameplay = WheelZoneGameplayProfile.Create(includesBombSlot: true, allowLeave: false);

            Assert.AreEqual(7, WheelSliceSlotCatalog.RewardSlotsForZone(8, zoneGameplay));
            Assert.AreEqual(8, WheelSliceSlotCatalog.RewardSlotsForZone(8, WheelZoneGameplayProfile.Create(includesBombSlot: false, allowLeave: true)));
        }

        [Test]
        public void BombIndexForZone_ReturnsNegativeOne_WhenBombDisabled()
        {
            var zoneGameplay = WheelZoneGameplayProfile.Create(includesBombSlot: false, allowLeave: true);

            Assert.AreEqual(-1, WheelSliceSlotCatalog.BombIndexForZone(8, zoneGameplay));
        }

        [Test]
        public void CreateRewardProfile_ShowsAmountLabel_WhenAmountGreaterThanOne()
        {
            var reward = RewardDefinition.Create("coins", "Coins", null, 3, RewardTier.Common, Color.yellow);

            WheelSliceSlotProfile profile = WheelSliceSlotCatalog.CreateRewardProfile(reward);

            Assert.IsTrue(profile.ShowAmountLabel);
        }

        [Test]
        public void CreateRewardProfile_HidesAmountLabel_WhenAmountIsOne()
        {
            var reward = RewardDefinition.Create("coins", "Coins", null, 1, RewardTier.Common, Color.yellow);

            WheelSliceSlotProfile profile = WheelSliceSlotCatalog.CreateRewardProfile(reward);

            Assert.IsFalse(profile.ShowAmountLabel);
        }
    }
}
