using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardOpeningDeckRenderer
    {
        private readonly WheelRewardCardView[] _cards;

        public WheelRewardOpeningDeckRenderer(WheelRewardCardView[] cards)
        {
            _cards = cards;
        }

        public void Render(WheelHudSnapshot snapshot, int visibleCount)
        {
            WheelHudRewardCardsSnapshot rewards = snapshot.Rewards;
            for (int i = 0; i < _cards.Length; i++)
            {
                if (i >= visibleCount)
                {
                    _cards[i].Hide();
                    continue;
                }

                int revealOrder = (visibleCount - 1) - i;
                _cards[i].ApplyFrames(rewards.RewardCardFrameSprite);
                _cards[i].Apply(rewards.RewardCards[i], revealOrder, visibleCount, true, true, rewards.DefaultRewardTitle);
            }
        }
    }
}
