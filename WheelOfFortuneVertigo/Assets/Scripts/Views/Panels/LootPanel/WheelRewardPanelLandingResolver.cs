using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardPanelLandingResolver
    {
        private readonly WheelLootCardView[] _cardViews;

        public WheelRewardPanelLandingResolver(WheelLootCardView[] cardViews)
        {
            _cardViews = cardViews;
        }

        public Vector3 Resolve(int index, int burstIndex, int burstCount, Vector3 fallbackPosition)
        {
            if (index < 0 || index >= _cardViews.Length) return fallbackPosition;
            RectTransform rect = _cardViews[index].GetComponent<RectTransform>();
            if (rect == null) return fallbackPosition;
            return rect.TransformPoint(rect.rect.center) + WheelRewardPanelMotion.LandingSpreadOffset(burstIndex, burstCount);
        }
    }
}
