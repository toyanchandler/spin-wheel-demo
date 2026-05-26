using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardPanelRenderer
    {
        private readonly WheelLootCardView[] _cardViews;
        private Sprite _cardFrameSprite;
        private string _defaultRewardTitle = string.Empty;

        public WheelRewardPanelRenderer(WheelLootCardView[] cardViews)
        {
            _cardViews = cardViews;
        }

        public void SetPresentation(Sprite cardFrameSprite, string defaultRewardTitle)
        {
            _cardFrameSprite = cardFrameSprite;
            _defaultRewardTitle = defaultRewardTitle;
        }

        public void Render(RewardInventoryEntry[] cards, int count, bool animateChangedCard, int changedIndex)
        {
            for (int i = 0; i < _cardViews.Length; i++)
            {
                if (i >= count)
                {
                    _cardViews[i].Hide();
                    continue;
                }

                bool animated = animateChangedCard && i == changedIndex;
                _cardViews[i].ApplyFrame(_cardFrameSprite);
                _cardViews[i].Apply(cards[i], i, animated, _defaultRewardTitle);
            }
        }

        public void HideRange(int startIndex, int endIndex)
        {
            int start = Mathf.Clamp(startIndex, 0, _cardViews.Length);
            int end = Mathf.Clamp(endIndex, start, _cardViews.Length);
            for (int i = start; i < end; i++)
            {
                _cardViews[i].Hide();
            }
        }

        public void PulseChangedCards(int oldCount, int changedIndex)
        {
            int count = Mathf.Min(_cardViews.Length, oldCount <= changedIndex ? changedIndex + 1 : _cardViews.Length);
            for (int i = 0; i < count; i++)
            {
                bool isNewSlot = i >= oldCount;
                bool amountChanged = !isNewSlot && i == changedIndex;
                if (isNewSlot || amountChanged)
                {
                    _cardViews[i].PlayLandingPulse(WheelRewardPanelMotion.LandingPulseDelay(i));
                }
            }
        }
    }
}
