using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Collections;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardPanelView : MonoBehaviour
    {
        [SerializeField] private Image _panelFrameImage;
        [SerializeField] private Transform _cardPoolRoot;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelRewardCardView[] _cardViews = Array.Empty<WheelRewardCardView>();
        [SerializeField] private RectTransform _viewportRect;
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private ScrollRect _scrollRect;

        private WheelEventBus _eventBus;
        private RewardInventoryEntry[] _renderedCards = Array.Empty<RewardInventoryEntry>();
        private RewardInventoryEntry[] _pendingCards = Array.Empty<RewardInventoryEntry>();
        private int _renderedCount;
        private int _pendingCount;
        private int _pendingChangedIndex = -1;
        private bool _hasPendingCards;
        private Tween _pendingFallbackTween;
        private Sprite _cardFrameSprite;

        public void Bind(WheelEventBus eventBus)
        {
            RequireCardViews();
            EnsureBuffers();
            EnsureScrollableStrip();
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
        }

        private void OnDisable()
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
            if (_panelFrameImage != null)
            {
                _panelFrameImage.transform.DOKill();
            }
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            EnsureCardCapacity(snapshot.RewardCardCount);
            EnsureBuffers();
            _panelFrameImage.sprite = snapshot.RewardPanelFrameSprite;
            _panelFrameImage.color = Color.white;
            _panelFrameImage.raycastTarget = true;
            _cardFrameSprite = snapshot.RewardCardFrameSprite;
            if (ShouldDeferRewardGain(snapshot))
            {
                StorePending(snapshot);
                RenderStoredCards(_renderedCards, _renderedCount);
                ScheduleFallbackCommit();
                return;
            }

            ApplySnapshotImmediately(snapshot);
        }

        public void CommitPendingRewards(float delay)
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = DOVirtual.DelayedCall(delay, CommitPendingNow, false)
                .SetTarget(this)
                .SetUpdate(true);
        }

        public Vector3 ResolveLandingWorldPosition(string rewardId, int burstIndex, int burstCount)
        {
            int index = FindPendingIndex(rewardId);
            if (index < 0)
            {
                index = Mathf.Max(0, Mathf.Min(_pendingCount - 1, _renderedCount));
            }

            if (index < 0 || index >= _cardViews.Length)
            {
                return transform.position;
            }

            FocusCardIndex(index, false);
            RectTransform rect = _cardViews[index].GetComponent<RectTransform>();
            Vector3 target = rect.TransformPoint(rect.rect.center);
            float spread = Mathf.Min(26f, 8f + burstCount * 2f);
            float offsetX = burstCount <= 1 ? 0f : Mathf.Lerp(-spread, spread, burstIndex / Mathf.Max(1f, burstCount - 1f));
            float offsetY = burstCount <= 1 ? 0f : UnityEngine.Random.Range(-8f, 8f);
            return target + new Vector3(offsetX, offsetY, 0f);
        }

        private bool ShouldDeferRewardGain(WheelHudSnapshot snapshot)
        {
            return snapshot.Phase == WheelGamePhase.Won && SnapshotDiffersFromRendered(snapshot);
        }

        private bool SnapshotDiffersFromRendered(WheelHudSnapshot snapshot)
        {
            if (snapshot.RewardCardCount != _renderedCount)
            {
                return true;
            }

            int count = Mathf.Min(snapshot.RewardCardCount, _renderedCards.Length);
            for (int i = 0; i < count; i++)
            {
                RewardInventoryEntry incoming = snapshot.RewardCards[i];
                RewardInventoryEntry rendered = _renderedCards[i];
                if (incoming.RewardId != rendered.RewardId || incoming.Amount != rendered.Amount)
                {
                    return true;
                }
            }

            return false;
        }

        private void StorePending(WheelHudSnapshot snapshot)
        {
            _pendingChangedIndex = FindFirstChangedIndex(snapshot);
            _pendingCount = CopyCards(snapshot, _pendingCards);
            _hasPendingCards = true;
            ArrangeCards(Mathf.Max(_renderedCount, _pendingCount));
        }

        private void ScheduleFallbackCommit()
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = DOVirtual.DelayedCall(2.35f, CommitPendingNow, false)
                .SetTarget(this)
                .SetUpdate(true);
        }

        private void CommitPendingNow()
        {
            if (!_hasPendingCards)
            {
                return;
            }

            int oldCount = _renderedCount;
            CopyCards(_pendingCards, _pendingCount, _renderedCards);
            _renderedCount = _pendingCount;
            _hasPendingCards = false;
            RenderStoredCards(_renderedCards, _renderedCount);
            PlayPanelPulse();
            FocusCardIndex(_pendingChangedIndex >= 0 ? _pendingChangedIndex : _renderedCount - 1, true);
            PulseChangedCards(oldCount, _pendingChangedIndex);
            _pendingChangedIndex = -1;
        }

        private void ApplySnapshotImmediately(WheelHudSnapshot snapshot)
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
            _hasPendingCards = false;
            _renderedCount = CopyCards(snapshot, _renderedCards);
            RenderStoredCards(_renderedCards, _renderedCount);
        }

        private void RenderStoredCards(RewardInventoryEntry[] cards, int count)
        {
            ArrangeCards(count);
            for (int i = 0; i < _cardViews.Length; i++)
            {
                int visible = System.Convert.ToInt32(i < count);
                CardSlotActions[visible](_cardViews[i], cards, _cardFrameSprite, i);
            }

            FocusCardIndex(count - 1, false);
        }

        private void PulseChangedCards(int oldCount, int changedIndex)
        {
            int count = Mathf.Min(_renderedCount, _cardViews.Length);
            for (int i = 0; i < count; i++)
            {
                bool isNewSlot = i >= oldCount;
                bool amountChanged = !isNewSlot && i == changedIndex;
                if (isNewSlot || amountChanged)
                {
                    _cardViews[i].PlayLandingPulse(0.02f * i);
                }
            }
        }

        private void PlayPanelPulse()
        {
            Transform panelTransform = _panelFrameImage.transform;
            panelTransform.DOKill();
            panelTransform.localScale = Vector3.one;
            DOTween.Sequence()
                .SetTarget(panelTransform)
                .SetUpdate(true)
                .Append(panelTransform.DOScale(new Vector3(1.025f, 1.025f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .Append(panelTransform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack));
        }

        private int FindFirstChangedIndex(WheelHudSnapshot snapshot)
        {
            int count = Mathf.Min(snapshot.RewardCardCount, _renderedCards.Length);
            for (int i = 0; i < count; i++)
            {
                RewardInventoryEntry incoming = snapshot.RewardCards[i];
                RewardInventoryEntry rendered = _renderedCards[i];
                if (incoming.RewardId != rendered.RewardId || incoming.Amount != rendered.Amount)
                {
                    return i;
                }
            }

            return snapshot.RewardCardCount > _renderedCount ? _renderedCount : -1;
        }

        private int FindPendingIndex(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                return -1;
            }

            for (int i = 0; i < _pendingCount; i++)
            {
                if (_pendingCards[i].RewardId == rewardId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void EnsureBuffers()
        {
            if (_renderedCards.Length != _cardViews.Length)
            {
                Array.Resize(ref _renderedCards, _cardViews.Length);
                Array.Resize(ref _pendingCards, _cardViews.Length);
            }
        }

        private void EnsureCardCapacity(int requiredCount)
        {
            if (requiredCount <= _cardViews.Length || _cardViews.Length == 0 || _cardPoolRoot == null)
            {
                return;
            }

            var expandedViews = new WheelRewardCardView[requiredCount];
            Array.Copy(_cardViews, expandedViews, _cardViews.Length);
            WheelRewardCardView template = _cardViews[0];
            for (int i = _cardViews.Length; i < requiredCount; i++)
            {
                WheelRewardCardView view = Instantiate(template, _cardPoolRoot);
                view.name = "ui_loot_card_runtime_" + i;
                view.Hide();
                expandedViews[i] = view;
            }

            _cardViews = expandedViews;
        }

        private int CopyCards(WheelHudSnapshot snapshot, RewardInventoryEntry[] target)
        {
            return CopyCards(snapshot.RewardCards, snapshot.RewardCardCount, target);
        }

        private static int CopyCards(RewardInventoryEntry[] source, int sourceCount, RewardInventoryEntry[] target)
        {
            int count = Mathf.Min(sourceCount, target.Length);
            for (int i = 0; i < count; i++)
            {
                target[i] = source[i];
            }

            return count;
        }

        private void EnsureScrollableStrip()
        {
            if (_viewportRect == null)
            {
                _viewportRect = _cardPoolRoot as RectTransform;
            }

            if (_viewportRect == null)
            {
                return;
            }

            EnsureViewportMask();

            if (_contentRect == null)
            {
                _contentRect = FindExistingScrollContent();
            }

            if (_contentRect == null)
            {
                _contentRect = _cardPoolRoot as RectTransform;
            }

            _cardPoolRoot = _contentRect;

            if (_scrollRect == null)
            {
                _scrollRect = GetComponent<ScrollRect>();
            }

            if (_scrollRect == null)
            {
                return;
            }

            _scrollRect.viewport = _viewportRect;
            _scrollRect.content = _contentRect;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.inertia = true;
            _scrollRect.scrollSensitivity = 28f;
        }

        private void EnsureViewportMask()
        {
            if (_viewportRect.GetComponent<RectMask2D>() == null)
            {
                _viewportRect.gameObject.AddComponent<RectMask2D>();
            }
        }

        private RectTransform FindExistingScrollContent()
        {
            Transform content = _viewportRect.Find("ui_loot_cards_scroll_content");
            return content == null ? null : content as RectTransform;
        }

        private void ArrangeCards(int visibleCount)
        {
            if (_contentRect == null)
            {
                return;
            }

            float spacing = 12f;
            float contentHeight = 0f;
            int arrangedCount = Mathf.Max(visibleCount, _cardViews.Length);
            for (int i = 0; i < arrangedCount && i < _cardViews.Length; i++)
            {
                RectTransform rect = _cardViews[i].GetComponent<RectTransform>();
                Vector2 size = rect.sizeDelta;
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -((size.y * 0.5f) + (i * (size.y + spacing))));
                contentHeight = Mathf.Max(contentHeight, ((i + 1) * size.y) + (i * spacing));
            }

            float viewportHeight = _viewportRect == null ? 0f : _viewportRect.rect.height;
            _contentRect.sizeDelta = new Vector2(0f, Mathf.Max(viewportHeight, contentHeight));
        }

        private void FocusCardIndex(int index, bool animated)
        {
            if (_scrollRect == null || _contentRect == null || _viewportRect == null || _cardViews.Length == 0)
            {
                return;
            }

            int clampedIndex = Mathf.Clamp(index, 0, _cardViews.Length - 1);
            RectTransform cardRect = _cardViews[clampedIndex].GetComponent<RectTransform>();
            float viewportHeight = _viewportRect.rect.height;
            float contentHeight = _contentRect.rect.height;
            float maxScrollY = Mathf.Max(0f, contentHeight - viewportHeight);
            float cardBottomFromContentTop = -cardRect.anchoredPosition.y + (cardRect.rect.height * (1f - cardRect.pivot.y));
            float targetY = Mathf.Clamp(cardBottomFromContentTop - viewportHeight, 0f, maxScrollY);
            ApplyScrollY(targetY, animated);
        }

        private void ApplyScrollY(float targetY, bool animated)
        {
            _contentRect.DOKill();
            _scrollRect.StopMovement();
            if (!animated)
            {
                SetContentScrollY(targetY);
                return;
            }

            _contentRect.DOAnchorPosY(targetY, 0.18f)
                .SetEase(Ease.OutCubic)
                .SetTarget(_contentRect)
                .SetUpdate(true);
        }

        private void SetContentScrollY(float targetY)
        {
            Vector2 position = _contentRect.anchoredPosition;
            position.y = targetY;
            _contentRect.anchoredPosition = position;
            _scrollRect.verticalNormalizedPosition = targetY <= 0f ? 1f : 0f;
        }

        private void RequireCardViews()
        {
            if (_cardViews == null || _cardViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no reward cards. Collect children in the inspector or rebuild the scene.");
            }
        }

        private static readonly System.Action<WheelRewardCardView, RewardInventoryEntry[], Sprite, int>[] CardSlotActions =
        {
            (view, cards, frameSprite, index) => view.Hide(),
            (view, cards, frameSprite, index) =>
            {
                view.ApplyFrames(frameSprite);
                view.Apply(cards[index]);
            }
        };
    }
}
