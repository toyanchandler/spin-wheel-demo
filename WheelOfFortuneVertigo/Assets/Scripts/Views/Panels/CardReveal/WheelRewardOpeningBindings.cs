using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardOpeningBindings : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private Transform _cardPoolRoot;
        [SerializeField] private RectTransform _viewportRect;
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Camera _rewardBurstCamera;
        [SerializeField] private RawImage _rewardBurstDisplay;
        [SerializeField] private ParticleSystem _rewardBurstParticle;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelRewardCardView[] _rewardCards = Array.Empty<WheelRewardCardView>();

        public int CardCount { get { return _rewardCards == null ? 0 : _rewardCards.Length; } }

        public void Validate()
        {
            Require(_root, nameof(_root));
            Require(_titleText, nameof(_titleText));
            Require(_canvasGroup, nameof(_canvasGroup));
            Require(_contentRoot, nameof(_contentRoot));
            Require(_cardPoolRoot, nameof(_cardPoolRoot));
            Require(_viewportRect, nameof(_viewportRect));
            Require(_contentRect, nameof(_contentRect));
            Require(_scrollRect, nameof(_scrollRect));
            Require(_previousButton, nameof(_previousButton));
            Require(_nextButton, nameof(_nextButton));

            if (_rewardCards == null || _rewardCards.Length == 0)
            {
                throw new InvalidOperationException(name + " requires baked reward card views.");
            }
        }

        public void AddNavigationListeners(UnityAction previous, UnityAction next)
        {
            _previousButton.onClick.AddListener(previous);
            _nextButton.onClick.AddListener(next);
        }

        public void RemoveNavigationListeners(UnityAction previous, UnityAction next)
        {
            _previousButton.onClick.RemoveListener(previous);
            _nextButton.onClick.RemoveListener(next);
        }

        internal WheelRewardOpeningRootBinding CreateRootBinding()
        {
            return new WheelRewardOpeningRootBinding(_root, _titleText, _canvasGroup, _contentRoot);
        }

        internal WheelRewardOpeningDeckRenderer CreateDeckRenderer()
        {
            return new WheelRewardOpeningDeckRenderer(_rewardCards);
        }

        internal WheelRewardOpeningScroller CreateScroller(UnityEngine.Object tweenTarget)
        {
            return new WheelRewardOpeningScroller(
                _scrollRect,
                _viewportRect,
                _contentRect,
                _previousButton,
                _nextButton,
                _rewardCards,
                tweenTarget);
        }

        internal WheelRewardOpeningBurst CreateBurst(UnityEngine.Object tweenTarget)
        {
            return new WheelRewardOpeningBurst(
                new WheelRewardOpeningBurstBinding(_rewardBurstCamera, _rewardBurstDisplay, _rewardBurstParticle),
                tweenTarget);
        }

        private void Require(UnityEngine.Object value, string fieldName)
        {
            if (value == null)
            {
                throw new InvalidOperationException(name + " requires opening binding " + fieldName + ".");
            }
        }
    }
}
