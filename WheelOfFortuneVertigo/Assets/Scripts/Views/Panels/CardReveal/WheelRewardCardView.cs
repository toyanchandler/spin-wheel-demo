using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardCardView : MonoBehaviour
    {
        public const float OpeningFeaturedRevealStep = WheelRewardCardMotion.OpeningFeaturedRevealStep;
        public const float OpeningFeaturedRevealMaxDelay = WheelRewardCardMotion.OpeningFeaturedRevealMaxDelay;

        [SerializeField] private Image _frontImage;
        [SerializeField] private Image _backImage;
        [SerializeField] private Image _haloImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Sprite _frontArtSprite;
        [SerializeField] private Sprite _backArtSprite;
        [SerializeField] private Sprite _haloSprite;

        private WheelRewardCardBinding _binding;

        public void ApplyFrames(Sprite cardFrameSprite)
        {
            EnsureBinding();
            _binding.ApplyFrames(cardFrameSprite);
        }

        public void Apply(RewardInventoryEntry entry, int displayIndex, bool featured, string defaultTitle)
        {
            Apply(entry, displayIndex, displayIndex + 1, featured, true, defaultTitle);
        }

        public void Apply(RewardInventoryEntry entry, int displayIndex, int visibleCardCount, bool featured, bool animated, string defaultTitle)
        {
            EnsureBinding();
            _binding.ApplyContent(entry, defaultTitle);
            gameObject.SetActive(true);

            if (animated)
            {
                _binding.PrepareReveal(entry, featured);
                WheelRewardCardAnimator.PlayFlipReveal(this, _binding, displayIndex, visibleCardCount, featured, entry.AccentColor);
                return;
            }

            _binding.ShowWithoutEntrance(featured, entry.AccentColor);
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
            WheelRewardCardAnimator.PlayLandingPulse(this, _binding, delay);
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

            if (_frontImage == null ||
                _backImage == null ||
                _haloImage == null ||
                _iconImage == null ||
                _titleText == null ||
                _amountText == null ||
                _canvasGroup == null)
            {
                throw new InvalidOperationException(name + " reward opening card prefab has missing serialized references.");
            }

            _binding = new WheelRewardCardBinding(
                transform,
                _frontImage,
                _backImage,
                _haloImage,
                _iconImage,
                _titleText,
                _amountText,
                _canvasGroup,
                _frontArtSprite,
                _backArtSprite,
                _haloSprite);
        }

        private void KillTweens()
        {
            transform.DOKill();
            DOTween.Kill(this);
        }
    }
}
