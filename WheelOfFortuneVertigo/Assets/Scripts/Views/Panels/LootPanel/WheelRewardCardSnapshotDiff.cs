using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardCardSnapshotDiff
    {
        public static bool DiffersFromRendered(
            WheelHudRewardCardsSnapshot rewards,
            RewardInventoryEntry[] renderedCards,
            int renderedCount)
        {
            if (rewards.RewardCardCount != renderedCount)
            {
                return true;
            }

            int count = System.Math.Min(rewards.RewardCardCount, renderedCards.Length);
            for (int i = 0; i < count; i++)
            {
                if (EntryDiffers(rewards.RewardCards[i], renderedCards[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static int FindLastChangedIndex(
            WheelHudRewardCardsSnapshot rewards,
            RewardInventoryEntry[] renderedCards,
            int renderedCount)
        {
            int count = System.Math.Min(rewards.RewardCardCount, renderedCards.Length);
            int changedIndex = -1;
            for (int i = 0; i < count; i++)
            {
                if (EntryDiffers(rewards.RewardCards[i], renderedCards[i]))
                {
                    changedIndex = i;
                }
            }

            return rewards.RewardCardCount > renderedCount ? rewards.RewardCardCount - 1 : changedIndex;
        }

        private static bool EntryDiffers(RewardInventoryEntry incoming, RewardInventoryEntry rendered)
        {
            return incoming.RewardId != rendered.RewardId || incoming.Amount != rendered.Amount;
        }
    }
}
