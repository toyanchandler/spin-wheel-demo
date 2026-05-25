using System;
using DG.Tweening;
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
        private const float RevealScrollLeadIn = 0.10f;
        private const float RevealScrollSettle = 0.36f;

        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private Image _cardFrameSource;
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

        private WheelEventBus _eventBus;
        private Tween _visibilityTween;
        private Tween _scrollTween;
        private Sequence _burstSequence;
        private bool _isVisible;

        public void Bind(WheelEventBus eventBus)
        {
            RequireRewardCards();
            EnsureScrollableStrip();
            if (_previousButton != null)
            {
                _previousButton.onClick.AddListener(ScrollPrevious);
            }

            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(ScrollNext);
            }
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
            _root.SetActive(false);
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
            if (_previousButton != null)
            {
                _previousButton.onClick.RemoveListener(ScrollPrevious);
            }

            if (_nextButton != null)
            {
                _nextButton.onClick.RemoveListener(ScrollNext);
            }
            _visibilityTween?.Kill();
            _visibilityTween = null;
            _scrollTween?.Kill();
            _scrollTween = null;
            StopRevealParticleField();
        }

        private void OnDisable()
        {
            _visibilityTween?.Kill();
            _visibilityTween = null;
            _scrollTween?.Kill();
            _scrollTween = null;
            StopRevealParticleField();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            bool isVisible = snapshot.Phase == WheelGamePhase.CashedOut;
            if (!isVisible)
            {
                HideOpening();
                return;
            }

            ShowOpening();
            _titleText.text = "YOUR REWARDS:";
            RenderCards(snapshot);
        }

        private void ShowOpening()
        {
            if (_isVisible)
            {
                return;
            }

            _isVisible = true;
            _visibilityTween?.Kill();
            _root.SetActive(true);
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = 0f;
            if (_contentRoot != null)
            {
                _contentRoot.localScale = new Vector3(0.96f, 0.96f, 1f);
            }

            Sequence sequence = DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .Append(_canvasGroup.DOFade(1f, 0.22f).SetEase(Ease.OutQuad));
            if (_contentRoot != null)
            {
                sequence.Join(_contentRoot.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack));
            }

            _visibilityTween = sequence;
        }

        private void HideOpening()
        {
            if (!_isVisible)
            {
                _root.SetActive(false);
                return;
            }

            _isVisible = false;
            _visibilityTween?.Kill();
            _scrollTween?.Kill();
            StopRevealParticleField();
            if (_canvasGroup == null)
            {
                _root.SetActive(false);
                return;
            }

            _visibilityTween = _canvasGroup.DOFade(0f, 0.16f)
                .SetTarget(this)
                .SetUpdate(true)
                .OnComplete(() => _root.SetActive(false));
        }

        private void RenderCards(WheelHudSnapshot snapshot)
        {
            EnsureCardCapacity(snapshot.RewardCardCount);
            LayoutVisibleCards(snapshot.RewardCardCount);
            int visibleCount = Mathf.Min(snapshot.RewardCardCount, _rewardCards.Length);
            Sprite frame = _cardFrameSource.sprite;
            for (int i = 0; i < _rewardCards.Length; i++)
            {
                if (i >= visibleCount)
                {
                    _rewardCards[i].Hide();
                    continue;
                }

                int revealOrder = (visibleCount - 1) - i;
                _rewardCards[i].ApplyFrames(frame);
                _rewardCards[i].Apply(snapshot.RewardCards[i], revealOrder, true);
            }

            FocusCardIndex(visibleCount - 1);
            UpdateScrollControls();
            PlayReverseRevealScroll(visibleCount);
            PlayRevealParticleField(snapshot.RewardCardCount);
        }

        private void LayoutVisibleCards(int visibleCount)
        {
            int count = Mathf.Min(visibleCount, _rewardCards.Length);
            if (count <= 0)
            {
                return;
            }

            const float cardWidth = 370f;
            const float spacing = 48f;
            float rowWidth = (count * cardWidth) + ((count - 1) * spacing);
            float viewportWidth = _viewportRect == null ? 0f : _viewportRect.rect.width;
            float contentWidth = Mathf.Max(viewportWidth, rowWidth);
            float startX = ((contentWidth - rowWidth) * 0.5f) + (cardWidth * 0.5f);
            for (int i = 0; i < count; i++)
            {
                RectTransform rect = _rewardCards[i].GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(cardWidth, rect.sizeDelta.y);
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(startX + (i * (cardWidth + spacing)), 0f);
            }

            if (_contentRect != null)
            {
                _contentRect.sizeDelta = new Vector2(contentWidth, 0f);
            }
        }

        private void EnsureCardCapacity(int requiredCount)
        {
            if (requiredCount <= _rewardCards.Length || _rewardCards.Length == 0 || _cardPoolRoot == null)
            {
                return;
            }

            var expandedCards = new WheelRewardCardView[requiredCount];
            Array.Copy(_rewardCards, expandedCards, _rewardCards.Length);
            WheelRewardCardView template = _rewardCards[0];
            for (int i = _rewardCards.Length; i < requiredCount; i++)
            {
                WheelRewardCardView view = Instantiate(template, _cardPoolRoot);
                view.name = "ui_reward_opening_card_runtime_" + i;
                view.Hide();
                expandedCards[i] = view;
            }

            _rewardCards = expandedCards;
        }

        private void EnsureScrollableStrip()
        {
            if (_viewportRect == null)
            {
                _viewportRect = _cardPoolRoot as RectTransform;
            }

            if (_contentRect == null)
            {
                _contentRect = _cardPoolRoot as RectTransform;
            }

            if (_contentRect != null)
            {
                _cardPoolRoot = _contentRect;
            }

            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            if (_scrollRect == null || _viewportRect == null || _contentRect == null)
            {
                return;
            }

            _scrollRect.viewport = _viewportRect;
            _scrollRect.content = _contentRect;
            _scrollRect.horizontal = true;
            _scrollRect.vertical = false;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.inertia = true;
            _scrollRect.scrollSensitivity = 44f;
        }

        private void FocusCardIndex(int index)
        {
            if (_scrollRect == null || _contentRect == null || _viewportRect == null || _rewardCards.Length == 0)
            {
                return;
            }

            int clampedIndex = Mathf.Clamp(index, 0, _rewardCards.Length - 1);
            RectTransform cardRect = _rewardCards[clampedIndex].GetComponent<RectTransform>();
            float viewportWidth = _viewportRect.rect.width;
            float contentWidth = _contentRect.rect.width;
            float maxScrollX = Mathf.Max(0f, contentWidth - viewportWidth);
            float cardLeft = cardRect.anchoredPosition.x - (cardRect.rect.width * cardRect.pivot.x);
            float targetX = Mathf.Clamp(cardLeft - 32f, 0f, maxScrollX);
            _scrollRect.StopMovement();
            _contentRect.anchoredPosition = new Vector2(-targetX, _contentRect.anchoredPosition.y);
            _scrollRect.horizontalNormalizedPosition = maxScrollX <= 0f ? 0f : targetX / maxScrollX;
            UpdateScrollControls();
        }

        private void PlayReverseRevealScroll(int visibleCount)
        {
            _scrollTween?.Kill();
            _scrollTween = null;
            if (visibleCount <= 1 || _scrollRect == null || _contentRect == null || _viewportRect == null)
            {
                return;
            }

            float maxScrollX = Mathf.Max(0f, _contentRect.rect.width - _viewportRect.rect.width);
            if (maxScrollX <= 1f)
            {
                return;
            }

            float revealDuration = Mathf.Min(
                WheelRewardCardView.OpeningFeaturedRevealMaxDelay,
                (visibleCount - 1) * WheelRewardCardView.OpeningFeaturedRevealStep);
            float scrollDuration = Mathf.Max(0.24f, revealDuration + RevealScrollSettle);
            _scrollTween = DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .AppendInterval(RevealScrollLeadIn)
                .Append(DOTween.To(
                        () => _scrollRect.horizontalNormalizedPosition,
                        value => _scrollRect.horizontalNormalizedPosition = value,
                        0f,
                        scrollDuration)
                    .SetEase(Ease.InOutCubic)
                    .OnUpdate(UpdateScrollControls))
                .OnComplete(UpdateScrollControls);
        }

        private void ScrollPrevious()
        {
            ScrollBy(-1f);
        }

        private void ScrollNext()
        {
            ScrollBy(1f);
        }

        private void ScrollBy(float direction)
        {
            if (_scrollRect == null || _contentRect == null || _viewportRect == null)
            {
                return;
            }

            float maxScrollX = Mathf.Max(0f, _contentRect.rect.width - _viewportRect.rect.width);
            if (maxScrollX <= 1f)
            {
                UpdateScrollControls();
                return;
            }

            float pageStep = Mathf.Clamp01((_viewportRect.rect.width * 0.72f) / maxScrollX);
            float target = Mathf.Clamp01(_scrollRect.horizontalNormalizedPosition + (direction * pageStep));
            _scrollTween?.Kill();
            _scrollTween = DOTween.To(
                    () => _scrollRect.horizontalNormalizedPosition,
                    value => _scrollRect.horizontalNormalizedPosition = value,
                    target,
                    0.24f)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .OnUpdate(UpdateScrollControls)
                .OnComplete(UpdateScrollControls);
        }

        private void UpdateScrollControls()
        {
            if (_previousButton == null || _nextButton == null || _scrollRect == null || _contentRect == null || _viewportRect == null)
            {
                return;
            }

            bool hasOverflow = _contentRect.rect.width > _viewportRect.rect.width + 1f;
            float position = _scrollRect.horizontalNormalizedPosition;
            _previousButton.gameObject.SetActive(hasOverflow);
            _nextButton.gameObject.SetActive(hasOverflow);
            _previousButton.interactable = hasOverflow && position > 0.01f;
            _nextButton.interactable = hasOverflow && position < 0.99f;
        }

        private void PlayRevealParticleField(int rewardCardCount)
        {
            if (_rewardBurstDisplay == null || _rewardBurstParticle == null)
            {
                return;
            }

            StopRevealParticleField();
            int visibleCount = Mathf.Clamp(rewardCardCount, 1, 5);
            RectTransform displayRect = _rewardBurstDisplay.rectTransform;
            displayRect.anchoredPosition = new Vector2(0f, 18f);
            displayRect.sizeDelta = new Vector2(1540f, 760f);
            displayRect.localRotation = Quaternion.identity;
            displayRect.localScale = Vector3.one;
            _rewardBurstDisplay.enabled = true;
            _rewardBurstDisplay.color = new Color(1f, 1f, 1f, 0f);

            if (_rewardBurstCamera != null)
            {
                _rewardBurstCamera.enabled = true;
            }

            Transform particleTransform = _rewardBurstParticle.transform;
            particleTransform.localPosition = Vector3.zero;
            particleTransform.localRotation = Quaternion.identity;
            particleTransform.localScale = Vector3.one;
            _rewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            int openingBurst = Mathf.Clamp(visibleCount * 6, 22, 42);
            _burstSequence = DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .AppendInterval(0.26f)
                .AppendCallback(() => EmitRevealParticles(openingBurst))
                .Join(_rewardBurstDisplay.DOFade(0.42f, 0.22f))
                .AppendInterval(0.36f)
                .AppendCallback(() => EmitRevealParticles(Mathf.Clamp(visibleCount * 4, 16, 30)))
                .AppendInterval(0.54f)
                .AppendCallback(() => EmitRevealParticles(Mathf.Clamp(visibleCount * 3, 12, 24)))
                .AppendInterval(1.40f)
                .Append(_rewardBurstDisplay.DOFade(0f, 0.36f))
                .OnComplete(StopRevealParticleField);
        }

        private void EmitRevealParticles(int count)
        {
            if (_rewardBurstParticle == null)
            {
                return;
            }

            if (_rewardBurstCamera != null)
            {
                _rewardBurstCamera.enabled = true;
            }

            if (_rewardBurstDisplay != null)
            {
                _rewardBurstDisplay.enabled = true;
            }

            _rewardBurstParticle.Emit(count);
        }

        private void StopRevealParticleField()
        {
            if (_burstSequence != null)
            {
                Sequence sequence = _burstSequence;
                _burstSequence = null;
                sequence.Kill();
            }

            if (_rewardBurstParticle != null)
            {
                _rewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (_rewardBurstDisplay != null)
            {
                _rewardBurstDisplay.DOKill();
                _rewardBurstDisplay.enabled = false;
            }

            if (_rewardBurstCamera != null)
            {
                _rewardBurstCamera.enabled = false;
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
    }
}
