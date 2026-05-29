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
            WheelRewardCardPresentation presentation = WheelRewardCardPresentationBuilder.Create(entry, defaultTitle);
            Apply(presentation, displayIndex, animated);
        }

        public void Apply(WheelRewardCardPresentation presentation, int displayIndex, bool animated)
        {
            EnsureBinding();
            _binding.ApplyContent(presentation);
            gameObject.SetActive(true);

            if (animated)
            {
                _binding.PrepareEntrance(presentation);
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
            _binding?.ResetTransientVfx();
        }

        private void EnsureBinding()
        {
            if (_binding != null) return;

            WheelWiringValidation.Require(name, "loot card prefab", _shadowImage, nameof(_shadowImage));
            WheelWiringValidation.Require(name, "loot card prefab", _cardImage, nameof(_cardImage));
            WheelWiringValidation.Require(name, "loot card prefab", _iconImage, nameof(_iconImage));
            WheelWiringValidation.Require(name, "loot card prefab", _titleText, nameof(_titleText));
            WheelWiringValidation.Require(name, "loot card prefab", _amountText, nameof(_amountText));
            WheelWiringValidation.Require(name, "loot card prefab", _canvasGroup, nameof(_canvasGroup));
            WheelWiringValidation.Require(name, "loot card prefab", _glowImage, nameof(_glowImage));
            WheelWiringValidation.Require(name, "loot card prefab", _shineImage, nameof(_shineImage));

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
            DOTween.Kill(transform);
            DOTween.Kill(this);
        }
    }
}
