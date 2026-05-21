using System;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardPanelView : MonoBehaviour
    {
        [SerializeField] private Image _panelFrameImage;
        [SerializeField] private Transform _cardPoolRoot;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelRewardCardView[] _cardViews = Array.Empty<WheelRewardCardView>();

        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            RequireCardViews();
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
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

        private void RequireCardViews()
        {
            if (_cardViews == null || _cardViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no reward cards. Collect children in the inspector or rebuild the scene.");
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
