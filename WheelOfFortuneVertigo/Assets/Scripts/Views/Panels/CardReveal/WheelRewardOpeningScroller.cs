using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardOpeningScroller
    {
        private readonly ScrollRect _scrollRect;
        private readonly RectTransform _viewportRect;
        private readonly RectTransform _contentRect;
        private readonly Button _previousButton;
        private readonly Button _nextButton;
        private readonly WheelRewardCardView[] _cards;
        private readonly Object _tweenTarget;
        private Tween _scrollTween;
        private int _visibleCardCount;

        public WheelRewardOpeningScroller(
            ScrollRect scrollRect,
            RectTransform viewportRect,
            RectTransform contentRect,
            Button previousButton,
            Button nextButton,
            WheelRewardCardView[] cards,
            Object tweenTarget)
        {
            _scrollRect = scrollRect;
            _viewportRect = viewportRect;
            _contentRect = contentRect;
            _previousButton = previousButton;
            _nextButton = nextButton;
            _cards = cards;
            _tweenTarget = tweenTarget;
        }

        public void Stop()
        {
            _scrollTween?.Kill();
            _scrollTween = null;
        }

        public void ApplyBounds(int visibleCount)
        {
            _visibleCardCount = visibleCount;

            float viewportWidth = _viewportRect.rect.width;
            float visibleWidth = ResolveVisibleContentWidth(visibleCount);
            float contentWidth = Mathf.Max(viewportWidth, visibleWidth);
            _contentRect.sizeDelta = new Vector2(contentWidth - viewportWidth, _contentRect.sizeDelta.y);
            SetNormalizedScroll(_scrollRect.horizontalNormalizedPosition);
        }

        public void FocusCardIndex(int index)
        {
            if (_cards.Length == 0) return;
            int clampedIndex = Mathf.Clamp(index, 0, Mathf.Max(0, _visibleCardCount - 1));
            RectTransform cardRect = _cards[clampedIndex].GetComponent<RectTransform>();
            if (cardRect == null) return;
            float maxScrollX = ResolveMaxScrollX();
            float cardLeft = cardRect.anchoredPosition.x - (cardRect.rect.width * cardRect.pivot.x);
            float targetX = Mathf.Clamp(cardLeft - WheelRewardOpeningMotion.ScrollEdgePadding, 0f, maxScrollX);
            _scrollRect.StopMovement();
            _contentRect.anchoredPosition = new Vector2(-targetX, _contentRect.anchoredPosition.y);
            _scrollRect.horizontalNormalizedPosition = maxScrollX <= 0f ? 0f : targetX / maxScrollX;
        }

        public void PlayReverseReveal(int visibleCount)
        {
            Stop();
            if (visibleCount <= 1) return;
            float maxScrollX = ResolveMaxScrollX();
            if (maxScrollX <= 1f) return;
            float revealDuration = Mathf.Min(
                WheelRewardCardView.OpeningFeaturedRevealMaxDelay,
                (visibleCount - 1) * WheelRewardCardView.OpeningFeaturedRevealStep);
            float scrollDuration = Mathf.Max(WheelRewardOpeningMotion.MinimumScrollDuration, revealDuration + WheelRewardOpeningMotion.RevealScrollSettle);
            _scrollTween = DOTween.Sequence()
                .SetTarget(_tweenTarget)
                .SetUpdate(true)
                .AppendInterval(WheelRewardOpeningMotion.RevealScrollLeadIn)
                .Append(DOTween.To(
                        () => _scrollRect.horizontalNormalizedPosition,
                        SetNormalizedScroll,
                        0f,
                        scrollDuration)
                    .SetEase(Ease.InOutCubic)
                    .OnUpdate(UpdateControls))
                .OnComplete(UpdateControls);
        }

        public void ScrollBy(float direction)
        {
            float maxScrollX = ResolveMaxScrollX();
            if (maxScrollX <= 1f)
            {
                UpdateControls();
                return;
            }

            float cardStep = ResolveCardScrollStep();
            float currentX = Mathf.Clamp01(_scrollRect.horizontalNormalizedPosition) * maxScrollX;
            float targetX = Mathf.Clamp(currentX + (direction * cardStep), 0f, maxScrollX);
            float target = Mathf.Clamp01(targetX / maxScrollX);

            Stop();
            _scrollTween = DOTween.To(
                    () => _scrollRect.horizontalNormalizedPosition,
                    SetNormalizedScroll,
                    target,
                    WheelRewardOpeningMotion.ScrollButtonDuration)
                .SetEase(Ease.OutCubic)
                .SetTarget(_tweenTarget)
                .SetUpdate(true)
                .OnUpdate(UpdateControls)
                .OnComplete(UpdateControls);
        }

        public void UpdateControls()
        {
            float maxScrollX = ResolveMaxScrollX();
            bool hasOverflow = maxScrollX > 1f;
            float position = Mathf.Clamp01(_scrollRect.horizontalNormalizedPosition);
            SetButtonState(_previousButton, hasOverflow, position > WheelRewardOpeningMotion.ScrollEndTolerance);
            SetButtonState(_nextButton, hasOverflow, position < 1f - WheelRewardOpeningMotion.ScrollEndTolerance);
        }

        private float ResolveVisibleContentWidth() => ResolveVisibleContentWidth(_visibleCardCount);

        private float ResolveVisibleContentWidth(int visibleCardCount)
        {
            if (visibleCardCount <= 0 || _cards.Length == 0) return 0f;

            int lastIndex = Mathf.Min(visibleCardCount, _cards.Length) - 1;
            RectTransform lastRect = _cards[lastIndex].GetComponent<RectTransform>();
            if (lastRect == null) return _contentRect.rect.width;

            float cardLeft = lastRect.anchoredPosition.x - (lastRect.rect.width * lastRect.pivot.x);
            return cardLeft + lastRect.rect.width + WheelRewardOpeningMotion.ScrollEdgePadding;
        }

        private float ResolveMaxScrollX()
        {
            float visibleWidth = ResolveVisibleContentWidth();
            return Mathf.Max(0f, visibleWidth - _viewportRect.rect.width);
        }

        private float ResolveCardScrollStep()
        {
            if (_cards.Length == 0 || _cards[0] == null) return WheelRewardOpeningMotion.DefaultCardStep * WheelRewardOpeningMotion.ScrollButtonStepCards;

            RectTransform firstRect = _cards[0].GetComponent<RectTransform>();
            float firstWidth = firstRect == null ? WheelRewardOpeningMotion.DefaultCardWidth : firstRect.rect.width;
            float spacing = ResolveCardSpacing(firstRect);
            return Mathf.Max(1f, (firstWidth + spacing) * WheelRewardOpeningMotion.ScrollButtonStepCards);
        }

        private float ResolveCardSpacing(RectTransform firstRect)
        {
            if (_cards.Length < 2 || _cards[1] == null || firstRect == null) return WheelRewardOpeningMotion.DefaultCardSpacing;

            RectTransform secondRect = _cards[1].GetComponent<RectTransform>();
            if (secondRect == null) return WheelRewardOpeningMotion.DefaultCardSpacing;

            float firstRight = firstRect.anchoredPosition.x + (firstRect.rect.width * (1f - firstRect.pivot.x));
            float secondLeft = secondRect.anchoredPosition.x - (secondRect.rect.width * secondRect.pivot.x);
            return Mathf.Max(0f, secondLeft - firstRight);
        }

        private void SetNormalizedScroll(float value)
        {
            _scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(value);
        }

        private static void SetButtonState(Button button, bool visible, bool interactable)
        {
            if (button == null) return;

            button.gameObject.SetActive(visible);
            button.interactable = visible && interactable;
        }
    }
}
