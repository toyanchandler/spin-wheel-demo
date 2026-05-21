using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardOpeningView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private Image _cardFrameSource;
        [SerializeField] private WheelRewardCardView[] _cardViews;

        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
            _root.SetActive(false);
        }

        public void Dispose()
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
            for (int i = 0; i < _cardViews.Length; i++)
            {
                int visible = System.Convert.ToInt32(i < snapshot.RewardCardCount);
                CardSlotActions[visible](_cardViews[i], snapshot, _cardFrameSource.sprite, i);
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
