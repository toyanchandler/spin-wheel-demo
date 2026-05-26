using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelHudSnapshotTests
    {
        [Test]
        public void FlatProperties_ForwardToNestedParts()
        {
            var rewardCards = new[] { new RewardInventoryEntry("gold", "Gold", 2, null, Color.yellow) };
            var snapshot = new WheelHudSnapshot(
                zone: 4,
                phase: WheelGamePhase.Ready,
                backgroundColor: Color.black,
                zoneLabels: new WheelHudZoneLabelsSnapshot("Super Zone", Color.white, Color.red),
                milestones: new WheelHudMilestoneSnapshot(
                    5,
                    10,
                    5,
                    10,
                    "Safe 5",
                    "Super 10",
                    Color.green,
                    Color.blue),
                statusBar: new WheelHudStatusSnapshot("Spin to win", true, Color.gray),
                actions: new WheelHudActionsSnapshot(
                    true,
                    false,
                    true,
                    true,
                    "SPIN",
                    "LEAVE",
                    "RETRY"),
                exitConfirmation: new WheelHudExitConfirmationSnapshot(
                    "Leave?",
                    "Are you sure?",
                    "Collect",
                    "Stay"),
                rewards: new WheelHudRewardCardsSnapshot(
                    "Your Rewards",
                    "Reward",
                    null,
                    1,
                    rewardCards));

            Assert.AreEqual(4, snapshot.Zone);
            Assert.AreEqual(WheelGamePhase.Ready, snapshot.Phase);
            Assert.AreEqual(Color.black, snapshot.BackgroundColor);

            Assert.AreEqual("Super Zone", snapshot.ZoneTypeLabel);
            Assert.AreEqual(Color.red, snapshot.ZoneTypeColor);
            Assert.AreEqual(5, snapshot.SafeZoneInterval);
            Assert.AreEqual("Super 10", snapshot.SuperMilestoneBadgeText);
            Assert.AreEqual(Color.blue, snapshot.SuperMilestoneBadgeColor);

            Assert.AreEqual("Spin to win", snapshot.StatusText);
            Assert.IsTrue(snapshot.IsStatusVisible);

            Assert.IsTrue(snapshot.CanSpin);
            Assert.IsFalse(snapshot.CanLeave);
            Assert.AreEqual("SPIN", snapshot.SpinButtonLabel);
            Assert.IsTrue(snapshot.IsOutcomePopupAllowed);

            Assert.AreEqual("Leave?", snapshot.ExitConfirmationTitle);
            Assert.AreEqual("Stay", snapshot.ExitComeBackButtonLabel);

            Assert.AreEqual("Your Rewards", snapshot.RewardOpeningTitle);
            Assert.AreEqual(1, snapshot.RewardCardCount);
            Assert.AreEqual("Gold", snapshot.RewardCards[0].DisplayName);
        }
    }
}
