using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardOpeningView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _cardFrameSource;
        [SerializeField] private Transform _cardPoolRoot;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelRewardCardView[] _rewardCards = Array.Empty<WheelRewardCardView>();

        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            RequireRewardCards();
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
            _root.SetActive(false);
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            bool isVisible = snapshot.Phase == WheelGamePhase.CashedOut;
            _root.SetActive(isVisible);

            if (!isVisible)
            {
                return;
            }

            _titleText.text = "YOUR REWARDS:";
            RenderCards(snapshot);
        }

        private void RenderCards(WheelHudSnapshot snapshot)
        {
            for (int i = 0; i < _rewardCards.Length; i++)
            {
                int visible = System.Convert.ToInt32(i < snapshot.RewardCardCount);
                CardSlotActions[visible](_rewardCards[i], snapshot, _cardFrameSource.sprite, i);
            }
        }

        private void RequireRewardCards()
        {
            if (_rewardCards == null || _rewardCards.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no reward cards. Collect children in the inspector or rebuild the scene.");
            }
        }

        private static readonly System.Action<WheelRewardCardView, WheelHudSnapshot, Sprite, int>[] CardSlotActions =
        {
            (view, snapshot, frame, index) => view.Hide(),
            (view, snapshot, frame, index) =>
            {
                view.ApplyFrames(frame);
                view.Apply(snapshot.RewardCards[index]);
            }
        };
    }
}
