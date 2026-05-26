using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class RewardInventoryTests
    {
        [Test]
        public void BuildSummary_ReturnsEmptySummary_WhenInventoryIsEmpty()
        {
            var inventory = new RewardInventory();

            Assert.AreEqual("No rewards", inventory.BuildSummary("No rewards"));
        }

        [Test]
        public void Add_TracksSingleReward_AndBuildsSummary()
        {
            var inventory = new RewardInventory();
            inventory.Add(CreateResult("gold", "Gold", 2), "Reward");

            Assert.AreEqual(1, inventory.Count);
            Assert.AreEqual("Gold x2", inventory.BuildSummary("No rewards"));
        }

        [Test]
        public void Add_MergesDuplicateRewards_AndOrdersNewestLast()
        {
            var inventory = new RewardInventory();
            inventory.Add(CreateResult("gold", "Gold", 1), "Reward");
            inventory.Add(CreateResult("gems", "Gems", 3), "Reward");
            inventory.Add(CreateResult("gold", "Gold", 2), "Reward");

            Assert.AreEqual(2, inventory.Count);
            Assert.AreEqual("Gems x3  Gold x3", inventory.BuildSummary("No rewards"));
        }

        [Test]
        public void Clear_RemovesTrackedRewards()
        {
            var inventory = new RewardInventory();
            inventory.Add(CreateResult("gold", "Gold", 1), "Reward");

            inventory.Clear();

            Assert.AreEqual(0, inventory.Count);
            Assert.AreEqual("No rewards", inventory.BuildSummary("No rewards"));
        }

        [Test]
        public void CopyEntries_ReturnsEntriesInNewestOrder()
        {
            var inventory = new RewardInventory();
            inventory.Add(CreateResult("gold", "Gold", 1), "Reward");
            inventory.Add(CreateResult("gems", "Gems", 2), "Reward");
            inventory.Add(CreateResult("gold", "Gold", 3), "Reward");

            var buffer = new RewardInventoryEntry[4];
            int count = inventory.CopyEntries(buffer);

            Assert.AreEqual(2, count);
            Assert.AreEqual("gems", buffer[0].RewardId);
            Assert.AreEqual("gold", buffer[1].RewardId);
            Assert.AreEqual(4, buffer[1].Amount);
        }

        private static WheelSpinResult CreateResult(string rewardId, string displayName, int amount)
        {
            var reward = RewardDefinition.Create(rewardId, displayName, null, amount, RewardTier.Common, Color.white);
            reward.CacheRuntimeText("Won {0}");

            var slice = new WheelSliceDefinition();
            slice.ApplySlot(false, reward, null, amount, Color.white, displayName, amount > 1);
            return new WheelSpinResult(0, slice);
        }
    }
}
