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

        public void Apply(
            RewardInventoryEntry entry,
            int displayIndex,
            bool featured,
            string defaultTitle)
        {
            Apply(entry, displayIndex, displayIndex + 1, featured, true, defaultTitle);
        }

        public void Apply(
            RewardInventoryEntry entry,
            int displayIndex,
            int visibleCardCount,
            bool featured,
            bool animated,
            string defaultTitle)
        {
            WheelRewardCardPresentation presentation = WheelRewardCardPresentationBuilder.Create(entry, defaultTitle);
            Apply(presentation, displayIndex, visibleCardCount, featured, animated);
        }

        public void Apply(
            WheelRewardCardPresentation presentation,
            int displayIndex,
            int visibleCardCount,
            bool featured,
            bool animated)
        {
            EnsureBinding();
            _binding.ApplyContent(presentation);
            gameObject.SetActive(true);

            if (animated)
            {
                _binding.PrepareReveal(presentation, featured);
                WheelRewardCardAnimator.PlayFlipReveal(this, _binding, displayIndex, visibleCardCount, featured, presentation.AccentColor);
                return;
            }

            _binding.ShowWithoutEntrance(featured, presentation);
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
            _binding?.ResetTransientVfx();
        }

        private void EnsureBinding()
        {
            if (_binding != null) return;

            WheelWiringValidation.Require(name, "reward opening card prefab", _frontImage, nameof(_frontImage));
            WheelWiringValidation.Require(name, "reward opening card prefab", _backImage, nameof(_backImage));
            WheelWiringValidation.Require(name, "reward opening card prefab", _haloImage, nameof(_haloImage));
            WheelWiringValidation.Require(name, "reward opening card prefab", _iconImage, nameof(_iconImage));
            WheelWiringValidation.Require(name, "reward opening card prefab", _titleText, nameof(_titleText));
            WheelWiringValidation.Require(name, "reward opening card prefab", _amountText, nameof(_amountText));
            WheelWiringValidation.Require(name, "reward opening card prefab", _canvasGroup, nameof(_canvasGroup));

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
            DOTween.Kill(transform);
            DOTween.Kill(this);
        }
    }
}
