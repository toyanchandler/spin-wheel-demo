using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardPanelView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private Image _panelFrameImage;
        [SerializeField] private WheelRewardCardView[] _cardViews;

        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Dispose()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _panelFrameImage.sprite = snapshot.RewardPanelFrameSprite;
            _panelFrameImage.color = Color.white;
            RenderCards(snapshot);
        }

        private void RenderCards(WheelHudSnapshot snapshot)
        {
            for (int i = 0; i < _cardViews.Length; i++)
            {
                int visible = System.Convert.ToInt32(i < snapshot.RewardCardCount);
                CardSlotActions[visible](_cardViews[i], snapshot, i);
            }
        }

        private static readonly System.Action<WheelRewardCardView, WheelHudSnapshot, int>[] CardSlotActions =
        {
            (view, snapshot, index) => view.Hide(),
            (view, snapshot, index) =>
            {
                view.ApplyFrames(snapshot.RewardCardFrameSprite);
                view.Apply(snapshot.RewardCards[index]);
            }
        };
    }
}
