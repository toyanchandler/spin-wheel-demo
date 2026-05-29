using NUnit.Framework;
using Vertigo.Wheel.Runtime;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelRewardCardSnapshotDiffTests
    {
        [Test]
        public void DiffersFromRendered_ReturnsTrue_WhenCountChanges()
        {
            var rewards = new WheelHudRewardCardsSnapshot(
                string.Empty,
                string.Empty,
                null,
                2,
                new[] { Entry("a", 1), Entry("b", 1) });

            Assert.IsTrue(WheelRewardCardSnapshotDiff.DiffersFromRendered(
                rewards,
                new[] { Entry("a", 1) },
                1));
        }

        [Test]
        public void FindLastChangedIndex_ReturnsLastMismatch()
        {
            var rewards = new WheelHudRewardCardsSnapshot(
                string.Empty,
                string.Empty,
                null,
                2,
                new[] { Entry("a", 1), Entry("b", 2) });

            int index = WheelRewardCardSnapshotDiff.FindLastChangedIndex(
                rewards,
                new[] { Entry("a", 1), Entry("b", 1) },
                2);

            Assert.AreEqual(1, index);
        }

        private static RewardInventoryEntry Entry(string id, int amount)
        {
            return new RewardInventoryEntry(id, "Label", amount, null, default);
        }
    }
}
