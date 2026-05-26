using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelLootCardView : MonoBehaviour
    {
        [SerializeField] private Image _shadowImage;
        [SerializeField] private Image _cardImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _glowImage;
        [SerializeField] private Image _shineImage;
        [SerializeField] private Sprite _cardArtSprite;

        private WheelLootCardBinding _binding;

        public void ApplyFrame(Sprite cardFrameSprite)
        {
            EnsureBinding();
            _binding.ApplyFrame(cardFrameSprite);
        }

        public void Apply(RewardInventoryEntry entry, int displayIndex, bool animated, string defaultTitle)
        {
            EnsureBinding();
            _binding.ApplyContent(entry, defaultTitle);
            gameObject.SetActive(true);

            if (animated)
            {
                _binding.PrepareEntrance(entry);
                WheelLootCardAnimator.PlayEntrance(this, _binding, displayIndex);
                return;
            }

            _binding.ShowWithoutEntrance();
        }

        public void Hide()
        {
            KillTweens();
            EnsureBinding();
            _binding.Hide();
            gameObject.SetActive(false);
        }

        public void SetLostVisualState(bool isLost)
        {
            EnsureBinding();
            _binding.SetLostVisualState(isLost);
        }

        public void PlayLandingPulse(float delay)
        {
            EnsureBinding();
            _binding.PrepareLandingPulse();
            WheelLootCardAnimator.PlayLandingPulse(this, _binding, delay);
        }

        private void OnDisable()
        {
            KillTweens();
            if (_binding != null)
            {
                _binding.ResetTransientVfx();
            }
        }

        private void EnsureBinding()
        {
            if (_binding != null)
            {
                return;
            }

            if (_shadowImage == null ||
                _cardImage == null ||
                _iconImage == null ||
                _titleText == null ||
                _amountText == null ||
                _canvasGroup == null ||
                _glowImage == null ||
                _shineImage == null)
            {
                throw new InvalidOperationException(name + " loot card prefab has missing serialized references.");
            }

            _binding = new WheelLootCardBinding(
                transform,
                _shadowImage,
                _cardImage,
                _iconImage,
                _titleText,
                _amountText,
                _canvasGroup,
                _glowImage,
                _shineImage,
                _cardArtSprite);
        }

        private void KillTweens()
        {
            transform.DOKill();
            DOTween.Kill(this);
        }
    }
}
