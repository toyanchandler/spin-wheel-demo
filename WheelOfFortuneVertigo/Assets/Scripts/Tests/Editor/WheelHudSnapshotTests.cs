using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelHudSnapshotTests
    {
        [Test]
        public void NestedParts_ExposeComposedHudData()
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
                    false,
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

            Assert.AreEqual("Super Zone", snapshot.ZoneLabels.ZoneTypeLabel);
            Assert.AreEqual(Color.red, snapshot.ZoneLabels.ZoneTypeColor);
            Assert.AreEqual(5, snapshot.Milestones.SafeZoneInterval);
            Assert.AreEqual("Super 10", snapshot.Milestones.SuperMilestoneBadgeText);
            Assert.AreEqual(Color.blue, snapshot.Milestones.SuperMilestoneBadgeColor);

            Assert.AreEqual("Spin to win", snapshot.StatusBar.StatusText);
            Assert.IsTrue(snapshot.StatusBar.IsStatusVisible);

            Assert.IsTrue(snapshot.Actions.CanSpin);
            Assert.IsFalse(snapshot.Actions.CanLeave);
            Assert.AreEqual("SPIN", snapshot.Actions.SpinButtonLabel);
            Assert.IsTrue(snapshot.Actions.IsOutcomePopupAllowed);

            Assert.AreEqual("Leave?", snapshot.ExitConfirmation.Title);
            Assert.AreEqual("Stay", snapshot.ExitConfirmation.ComeBackButtonLabel);

            Assert.AreEqual("Your Rewards", snapshot.Rewards.RewardOpeningTitle);
            Assert.AreEqual(1, snapshot.Rewards.RewardCardCount);
            Assert.AreEqual("Gold", snapshot.Rewards.RewardCards[0].DisplayName);
        }
    }
}
