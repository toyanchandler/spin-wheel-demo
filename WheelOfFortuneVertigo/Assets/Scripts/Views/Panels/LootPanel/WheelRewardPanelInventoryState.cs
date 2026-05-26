using System;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardPanelInventoryState
    {
        private RewardInventoryEntry[] _renderedCards;
        private RewardInventoryEntry[] _pendingCards;
        private int _pendingChangedIndex = -1;
        private bool _hasPendingCards;

        public RewardInventoryEntry[] RenderedCards { get { return _renderedCards; } }
        public int RenderedCount { get; private set; }
        public int PendingCount { get; private set; }

        public WheelRewardPanelInventoryState(int capacity)
        {
            _renderedCards = new RewardInventoryEntry[capacity];
            _pendingCards = new RewardInventoryEntry[capacity];
        }

        public bool ShouldDeferRewardGain(WheelHudSnapshot snapshot)
        {
            return snapshot.Phase == WheelGamePhase.Won && SnapshotDiffersFromRendered(snapshot);
        }

        public void StorePending(WheelHudSnapshot snapshot)
        {
            _pendingChangedIndex = FindLastChangedIndex(snapshot);
            PendingCount = CopyCards(snapshot, _pendingCards);
            _hasPendingCards = true;
        }

        public void ApplySnapshotImmediately(WheelHudSnapshot snapshot)
        {
            _hasPendingCards = false;
            RenderedCount = CopyCards(snapshot, _renderedCards);
        }

        public bool CommitPending(out WheelRewardPanelCommit commit)
        {
            commit = default;
            if (!_hasPendingCards)
            {
                return false;
            }

            int oldCount = RenderedCount;
            CopyCards(_pendingCards, PendingCount, _renderedCards);
            RenderedCount = PendingCount;
            _hasPendingCards = false;
            commit = new WheelRewardPanelCommit(oldCount, _pendingChangedIndex);
            _pendingChangedIndex = -1;
            return true;
        }

        public int ResolveLandingIndex(string rewardId)
        {
            int index = FindPendingIndex(rewardId);
            if (index >= 0)
            {
                return index;
            }

            return Mathf.Max(0, Mathf.Min(PendingCount - 1, RenderedCount));
        }

        private bool SnapshotDiffersFromRendered(WheelHudSnapshot snapshot)
        {
            WheelHudRewardCardsSnapshot rewards = snapshot.Rewards;
            if (rewards.RewardCardCount != RenderedCount)
            {
                return true;
            }

            int count = Mathf.Min(rewards.RewardCardCount, _renderedCards.Length);
            for (int i = 0; i < count; i++)
            {
                RewardInventoryEntry incoming = rewards.RewardCards[i];
                RewardInventoryEntry rendered = _renderedCards[i];
                if (incoming.RewardId != rendered.RewardId || incoming.Amount != rendered.Amount)
                {
                    return true;
                }
            }

            return false;
        }

        private int FindLastChangedIndex(WheelHudSnapshot snapshot)
        {
            WheelHudRewardCardsSnapshot rewards = snapshot.Rewards;
            int count = Mathf.Min(rewards.RewardCardCount, _renderedCards.Length);
            int changedIndex = -1;
            for (int i = 0; i < count; i++)
            {
                RewardInventoryEntry incoming = rewards.RewardCards[i];
                RewardInventoryEntry rendered = _renderedCards[i];
                if (incoming.RewardId != rendered.RewardId || incoming.Amount != rendered.Amount)
                {
                    changedIndex = i;
                }
            }

            return rewards.RewardCardCount > RenderedCount ? rewards.RewardCardCount - 1 : changedIndex;
        }

        private int FindPendingIndex(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                return -1;
            }

            for (int i = 0; i < PendingCount; i++)
            {
                if (_pendingCards[i].RewardId == rewardId)
                {
                    return i;
                }
            }

            return -1;
        }

        private int CopyCards(WheelHudSnapshot snapshot, RewardInventoryEntry[] target)
        {
            WheelHudRewardCardsSnapshot rewards = snapshot.Rewards;
            return CopyCards(rewards.RewardCards, rewards.RewardCardCount, target);
        }

        private static int CopyCards(RewardInventoryEntry[] source, int sourceCount, RewardInventoryEntry[] target)
        {
            int count = Mathf.Min(sourceCount, target.Length);
            for (int i = 0; i < count; i++)
            {
                target[i] = source[i];
            }

            return count;
        }
    }

    internal readonly struct WheelRewardPanelCommit
    {
        public readonly int OldCount;
        public readonly int ChangedIndex;

        public WheelRewardPanelCommit(int oldCount, int changedIndex)
        {
            OldCount = oldCount;
            ChangedIndex = changedIndex;
        }
    }
}
