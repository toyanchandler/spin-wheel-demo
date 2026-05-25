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
    public sealed class WheelRewardPanelView : MonoBehaviour
    {
        [SerializeField] private Image _panelFrameImage;
        [SerializeField] private Transform _cardPoolRoot;

        [CollectChildren(nameof(_cardPoolRoot))]
        [SerializeField] private WheelRewardCardView[] _cardViews = Array.Empty<WheelRewardCardView>();
        [SerializeField] private RectTransform _viewportRect;
        [SerializeField] private RectTransform _contentRect;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GameObject _starterLootRoot;
        [SerializeField] private CanvasGroup _starterLootGroup;
        [SerializeField] private CanvasGroup _cardPoolGroup;
        [SerializeField] private Image _riskValueCardImage;
        [SerializeField] private Image _riskValueGlowImage;
        [SerializeField] private TextMeshProUGUI _riskValueLabel;
        [SerializeField] private TextMeshProUGUI _riskValueAmount;

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
            EnsureFailStateReferences();
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
            HideLegacyRiskValueCard();
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
            _panelFrameImage.raycastTarget = true;
            _cardFrameSprite = snapshot.RewardCardFrameSprite;
            if (ShouldDeferRewardGain(snapshot))
            {
                StorePending(snapshot);
                RenderStoredCards(_renderedCards, _renderedCount);
                PreparePendingLandingSpace();
                SyncStarterLootVisibility();
                ApplyLostState(false);
                ScheduleFallbackCommit();
                return;
            }

            ApplySnapshotImmediately(snapshot);
        }

        public void HoldPendingRewardsForArrival()
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
        }

        public void CommitPendingRewardsNow()
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
            CommitPendingNow();
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
            RenderStoredCards(_renderedCards, _renderedCount, false, -1);
            PlayPanelPulse();
            FocusCardIndex(_pendingChangedIndex >= 0 ? _pendingChangedIndex : _renderedCount - 1, true);
            PulseChangedCards(oldCount, _pendingChangedIndex);
            SyncStarterLootVisibility();
            _pendingChangedIndex = -1;
        }

        private void ApplySnapshotImmediately(WheelHudSnapshot snapshot)
        {
            _pendingFallbackTween?.Kill();
            _pendingFallbackTween = null;
            _hasPendingCards = false;
            _renderedCount = CopyCards(snapshot, _renderedCards);
            RenderStoredCards(_renderedCards, _renderedCount, false, -1);
            SyncStarterLootVisibility();
            ApplyLostState(snapshot.Phase == WheelGamePhase.Bombed);
        }

        private void RenderStoredCards(RewardInventoryEntry[] cards, int count)
        {
            RenderStoredCards(cards, count, false, -1);
        }

        private void RenderStoredCards(RewardInventoryEntry[] cards, int count, bool animateChangedCard, int changedIndex)
        {
            ArrangeCards(count);
            for (int i = 0; i < _cardViews.Length; i++)
            {
                int visible = System.Convert.ToInt32(i < count);
                bool animated = animateChangedCard && i == changedIndex;
                CardSlotActions[visible](_cardViews[i], cards, _cardFrameSprite, i, animated);
                _cardViews[i].SetLostVisualState(false);
            }

            FocusCardIndex(count - 1, false);
        }

        private void SyncStarterLootVisibility()
        {
            if (_starterLootRoot != null)
            {
                _starterLootRoot.SetActive(false);
            }
        }

        private void PreparePendingLandingSpace()
        {
            int reservedCount = Mathf.Max(_renderedCount, _pendingCount);
            ArrangeCards(reservedCount);
            int focusIndex = _pendingChangedIndex >= 0 ? _pendingChangedIndex : reservedCount - 1;
            FocusCardIndex(focusIndex, true);
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

        private void ApplyLostState(bool isLost)
        {
            EnsureFailStateReferences();
            SetGroupAlpha(_starterLootGroup, isLost ? 0.62f : 1f);
            SetGroupAlpha(_cardPoolGroup, isLost ? 0.66f : 1f);

            int count = Mathf.Min(_renderedCount, _cardViews.Length);
            for (int i = 0; i < count; i++)
            {
                _cardViews[i].SetLostVisualState(isLost);
            }

            if (_riskValueLabel != null)
            {
                _riskValueLabel.text = isLost ? "LOOT LOST" : "CLAIM LOOT";
                _riskValueLabel.color = isLost
                    ? new Color(1f, 0.52f, 0.34f, 0.96f)
                    : new Color(0.68f, 0.88f, 0.94f, 0.92f);
            }

            if (_riskValueAmount != null)
            {
                _riskValueAmount.text = isLost ? "0" : "+245";
                _riskValueAmount.color = isLost
                    ? new Color(1f, 0.28f, 0.10f, 1f)
                    : new Color(0.62f, 1f, 0.84f, 1f);
            }

            if (_riskValueCardImage != null)
            {
                _riskValueCardImage.color = isLost
                    ? new Color(0.18f, 0.055f, 0.04f, 0.94f)
                    : new Color(0.035f, 0.16f, 0.20f, 0.92f);
            }

            if (_riskValueGlowImage != null)
            {
                _riskValueGlowImage.color = isLost
                    ? new Color(1f, 0.22f, 0.05f, 0.20f)
                    : new Color(0.22f, 0.92f, 1f, 0.12f);
            }

            if (isLost && _panelFrameImage != null)
            {
                _panelFrameImage.transform.DOKill();
                _panelFrameImage.transform.localScale = Vector3.one;
                DOTween.Sequence()
                    .SetTarget(_panelFrameImage.transform)
                    .SetUpdate(true)
                    .Append(_panelFrameImage.transform.DOScale(new Vector3(0.985f, 0.985f, 1f), 0.10f).SetEase(Ease.OutQuad))
                    .Append(_panelFrameImage.transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack));
            }
        }

        private void EnsureFailStateReferences()
        {
            if (_starterLootRoot != null && _starterLootGroup == null)
            {
                _starterLootGroup = EnsureCanvasGroup(_starterLootRoot);
            }

            if (_cardPoolRoot != null && _cardPoolGroup == null)
            {
                _cardPoolGroup = EnsureCanvasGroup(_cardPoolRoot.gameObject);
            }

            if (_riskValueCardImage == null)
            {
                _riskValueCardImage = FindDescendantComponent<Image>(transform, "ui_loot_risk_value_card");
            }

            if (_riskValueGlowImage == null)
            {
                _riskValueGlowImage = FindDescendantComponent<Image>(transform, "ui_loot_risk_value_glow");
            }

            if (_riskValueLabel == null)
            {
                _riskValueLabel = FindDescendantComponent<TextMeshProUGUI>(transform, "ui_loot_risk_value_label");
            }

            if (_riskValueAmount == null)
            {
                _riskValueAmount = FindDescendantComponent<TextMeshProUGUI>(transform, "ui_loot_risk_value_amount");
            }

            HideLegacyRiskValueCard();
        }

        private void HideLegacyRiskValueCard()
        {
            if (_riskValueCardImage != null)
            {
                _riskValueCardImage.gameObject.SetActive(false);
            }
        }

        private static CanvasGroup EnsureCanvasGroup(GameObject target)
        {
            CanvasGroup group = target.GetComponent<CanvasGroup>();
            return group != null ? group : target.AddComponent<CanvasGroup>();
        }

        private static void SetGroupAlpha(CanvasGroup group, float alpha)
        {
            if (group != null)
            {
                group.alpha = alpha;
            }
        }

        private static T FindDescendantComponent<T>(Transform root, string childName) where T : Component
        {
            Transform child = FindDescendant(root, childName);
            return child == null ? null : child.GetComponent<T>();
        }

        private static Transform FindDescendant(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            foreach (Transform child in root)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform nested = FindDescendant(child, childName);
                if (nested != null)
                {
                    return nested;
                }
            }

            return null;
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
                view.name = string.Concat("ui_loot_card_runtime_", i);
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
            int arrangedCount = Mathf.Clamp(visibleCount, 0, _cardViews.Length);
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

            if (index < 0)
            {
                ApplyScrollY(0f, animated);
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
            float viewportHeight = _viewportRect == null ? 0f : _viewportRect.rect.height;
            float contentHeight = _contentRect == null ? 0f : _contentRect.rect.height;
            float maxScrollY = Mathf.Max(0f, contentHeight - viewportHeight);
            _scrollRect.verticalNormalizedPosition = maxScrollY <= 0f ? 1f : 1f - Mathf.Clamp01(targetY / maxScrollY);
        }

        private void RequireCardViews()
        {
            if (_cardViews == null || _cardViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no reward cards. Collect children in the inspector or rebuild the scene.");
            }
        }

        private static readonly System.Action<WheelRewardCardView, RewardInventoryEntry[], Sprite, int, bool>[] CardSlotActions =
        {
            (view, cards, frameSprite, index, animated) => view.Hide(),
            (view, cards, frameSprite, index, animated) =>
            {
                view.ApplyFrames(frameSprite);
                view.Apply(cards[index], index, false, animated);
            }
        };
    }
}
