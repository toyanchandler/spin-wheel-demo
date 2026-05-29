using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardPanelLayout
    {
        private readonly WheelLootCardView[] _cardViews;
        private readonly ScrollRect _scrollRect;
        private readonly RectTransform _contentRect;
        private readonly RectTransform _viewportRect;
        private readonly Object _tweenTarget;
        private readonly Vector3[] _cardCorners = new Vector3[4];
        private readonly float _authoredContentHeight;
        private int _reservedCount;
        private Tween _scrollTween;

        public WheelRewardPanelLayout(
            WheelLootCardView[] cardViews,
            ScrollRect scrollRect,
            RectTransform contentRect,
            RectTransform viewportRect,
            Object tweenTarget)
        {
            _cardViews = cardViews;
            _scrollRect = scrollRect;
            _contentRect = contentRect;
            _viewportRect = viewportRect;
            _tweenTarget = tweenTarget;
            _authoredContentHeight = Mathf.Max(1f, contentRect.rect.height);
        }

        public void StopScroll()
        {
            _scrollTween?.Kill();
            _scrollTween = null;
        }

        public void LayoutReservedCards(int reservedCount, bool keepGapVisible)
        {
            _reservedCount = Mathf.Clamp(reservedCount, 0, _cardViews.Length);
            float contentHeight = Mathf.Max(ResolveViewportHeight(), _authoredContentHeight);
            _contentRect.sizeDelta = new Vector2(_contentRect.sizeDelta.x, contentHeight);
            if (keepGapVisible) ScrollToNewest(true);
        }

        public void ScrollToNewest(bool animated)
        {
            if (!HasScrollTargets()) return;

            _scrollRect.StopMovement();
            StopScroll();

            float target = ComputeScrollTarget();
            ApplyScrollTarget(target, animated);
        }

        private bool HasScrollTargets()
        {
            return _scrollRect != null && _contentRect != null && _viewportRect != null;
        }

        private float ComputeScrollTarget()
        {
            float overflow = _contentRect.rect.height - _viewportRect.rect.height;
            return ResolveScrollTarget(overflow);
        }

        private void ApplyScrollTarget(float target, bool animated)
        {
            if (!animated || Mathf.Abs(_scrollRect.verticalNormalizedPosition - target) < WheelRewardPanelMotion.ScrollPositionTolerance)
            {
                _scrollRect.verticalNormalizedPosition = target;
                return;
            }

            _scrollTween = DOVirtual.Float(
                    _scrollRect.verticalNormalizedPosition,
                    target,
                    WheelRewardPanelMotion.ScrollSettleDuration,
                    value => _scrollRect.verticalNormalizedPosition = value)
                .SetEase(Ease.OutCubic)
                .SetTarget(_tweenTarget)
                .SetUpdate(true);
        }

        private float ResolveScrollTarget(float overflow)
        {
            if (overflow <= WheelRewardPanelMotion.ScrollOverflowTolerance || _reservedCount <= 0)
            {
                return WheelRewardPanelMotion.ScrollTopNormalizedPosition;
            }

            RectTransform newestCard = ResolveNewestCardRect();
            if (newestCard == null) return WheelRewardPanelMotion.ScrollTopNormalizedPosition;

            float cardBottomDistanceFromTop = MeasureCardBottomDistanceFromTop(newestCard);
            float targetOffset = Mathf.Clamp(cardBottomDistanceFromTop - ResolveViewportHeight(), 0f, overflow);
            return Mathf.Clamp01(1f - (targetOffset / overflow));
        }

        private RectTransform ResolveNewestCardRect()
        {
            WheelLootCardView newestCardView = _cardViews[_reservedCount - 1];
            return newestCardView == null ? null : newestCardView.transform as RectTransform;
        }

        private float MeasureCardBottomDistanceFromTop(RectTransform cardRect)
        {
            cardRect.GetWorldCorners(_cardCorners);
            float bottom = float.PositiveInfinity;
            for (int i = 0; i < _cardCorners.Length; i++)
            {
                Vector3 localCorner = _contentRect.InverseTransformPoint(_cardCorners[i]);
                bottom = Mathf.Min(bottom, localCorner.y);
            }

            return _contentRect.rect.yMax - bottom;
        }

        private float ResolveViewportHeight() => _viewportRect == null ? 0f : _viewportRect.rect.height;
    }
}
