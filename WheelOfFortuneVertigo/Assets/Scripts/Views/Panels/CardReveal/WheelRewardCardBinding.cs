using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardCardBinding
    {
        private readonly Transform _rootTransform;
        private readonly Image _frontImage;
        private readonly Image _backImage;
        private readonly Image _haloImage;
        private readonly Image _iconImage;
        private readonly TextMeshProUGUI _titleText;
        private readonly TextMeshProUGUI _amountText;
        private readonly CanvasGroup _canvasGroup;
        private readonly Sprite _frontArtSprite;
        private readonly Sprite _backArtSprite;
        private readonly Sprite _haloSprite;

        public Image FrontImage { get { return _frontImage; } }
        public Image BackImage { get { return _backImage; } }
        public Image HaloImage { get { return _haloImage; } }
        public Image IconImage { get { return _iconImage; } }
        public TextMeshProUGUI TitleText { get { return _titleText; } }
        public TextMeshProUGUI AmountText { get { return _amountText; } }
        public CanvasGroup CanvasGroup { get { return _canvasGroup; } }
        public Transform RootTransform { get { return _rootTransform; } }

        public WheelRewardCardBinding(
            Transform rootTransform,
            Image frontImage,
            Image backImage,
            Image haloImage,
            Image iconImage,
            TextMeshProUGUI titleText,
            TextMeshProUGUI amountText,
            CanvasGroup canvasGroup,
            Sprite frontArtSprite,
            Sprite backArtSprite,
            Sprite haloSprite)
        {
            _rootTransform = rootTransform;
            _frontImage = frontImage;
            _backImage = backImage;
            _haloImage = haloImage;
            _iconImage = iconImage;
            _titleText = titleText;
            _amountText = amountText;
            _canvasGroup = canvasGroup;
            _frontArtSprite = frontArtSprite;
            _backArtSprite = backArtSprite;
            _haloSprite = haloSprite;
        }

        public void ApplyFrames(Sprite cardFrameSprite)
        {
            Sprite frontSprite = _frontArtSprite != null ? _frontArtSprite : cardFrameSprite;
            if (frontSprite != null)
            {
                _frontImage.sprite = frontSprite;
            }

            if (_backArtSprite != null)
            {
                _backImage.sprite = _backArtSprite;
            }

            if (_haloSprite != null)
            {
                _haloImage.sprite = _haloSprite;
            }

        }

        public void ApplyContent(RewardInventoryEntry entry, string defaultTitle)
        {
            _iconImage.sprite = entry.Icon;
            _iconImage.enabled = entry.Icon != null;
            _iconImage.color = Color.white;

            ApplyTitle(entry.DisplayName, defaultTitle);

            WheelRewardAmountLabel.Apply(_amountText, entry.Amount, ResolveAmountColor(entry.AccentColor));
        }

        public void PrepareReveal(RewardInventoryEntry entry, bool featured)
        {
            _canvasGroup.alpha = 0f;
            _rootTransform.localScale = featured ? WheelRewardCardMotion.FeaturedStartScale : WheelRewardCardMotion.StandardStartScale;
            _rootTransform.localRotation = Quaternion.Euler(featured ? WheelRewardCardMotion.FeaturedStartRotation : WheelRewardCardMotion.StandardStartRotation);

            _frontImage.enabled = true;
            _backImage.enabled = true;
            _frontImage.color = WheelUiGraphicUtility.WithAlpha(Color.white, 0f);
            _backImage.color = Color.white;

            Color haloColor = Color.Lerp(Color.white, entry.AccentColor, WheelRewardCardMotion.HaloTintWeight);
            haloColor.a = 0f;
            _haloImage.color = haloColor;
            _haloImage.enabled = true;
            _haloImage.rectTransform.localRotation = Quaternion.identity;
            _haloImage.rectTransform.localScale = Vector3.one;

            SetFrontContentAlpha(0f);
            _iconImage.transform.localScale = WheelRewardCardMotion.IconStartScale;
            _titleText.transform.localScale = WheelRewardCardMotion.TitleStartScale;
            _amountText.transform.localScale = WheelRewardCardMotion.AmountStartScale;
        }

        public void PrepareHaloTimeline(Color accentColor)
        {
            Color haloColor = Color.Lerp(Color.white, accentColor, WheelRewardCardMotion.HaloTintWeight);
            haloColor.a = 0f;
            _haloImage.color = haloColor;
            _haloImage.rectTransform.localScale = WheelRewardCardMotion.HaloStartScale;
            _haloImage.rectTransform.localRotation = Quaternion.identity;
        }

        public void PrepareLandingPulse()
        {
            _rootTransform.localScale = Vector3.one;
            _rootTransform.localRotation = Quaternion.identity;
        }

        public void ShowFrontForFlip()
        {
            _rootTransform.localRotation = Quaternion.Euler(WheelRewardCardMotion.FlipFrontRotation);
            _backImage.color = WheelUiGraphicUtility.WithAlpha(Color.white, 0f);
            _frontImage.color = Color.white;
            SetFrontContentAlpha(0f);
        }

        public void ShowWithoutEntrance(bool featured, Color accentColor)
        {
            _rootTransform.localScale = Vector3.one;
            _rootTransform.localRotation = Quaternion.identity;
            _frontImage.color = Color.white;
            _backImage.color = WheelUiGraphicUtility.WithAlpha(Color.white, 0f);
            SetFrontContentAlpha(1f);

            Color haloColor = Color.Lerp(Color.white, accentColor, WheelRewardCardMotion.HaloTintWeight);
            haloColor.a = featured ? WheelRewardCardMotion.FeaturedHaloSettleAlpha : WheelRewardCardMotion.StandardHaloSettleAlpha;
            _haloImage.color = haloColor;
            _haloImage.rectTransform.localScale = featured ? WheelRewardCardMotion.HaloSettleScale : Vector3.one;
            _canvasGroup.alpha = 1f;
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            SetFrontContentAlpha(0f);
            ResetTransientVfx();
        }

        public void ResetTransientVfx()
        {
            WheelUiGraphicUtility.SetGraphicAlpha(_haloImage, 0f);
        }

        public void SetLostVisualState(bool isLost)
        {
            _canvasGroup.alpha = isLost ? WheelRewardCardMotion.LostCardAlpha : 1f;

            if (isLost)
            {
                _frontImage.color = WheelRewardCardMotion.LostFrontColor;
                _haloImage.color = WheelRewardCardMotion.LostHaloColor;
            }
        }

        public void SetFrontContentAlpha(float alpha)
        {
            WheelUiGraphicUtility.SetGraphicAlpha(_iconImage, alpha);
            WheelUiGraphicUtility.SetTextAlpha(_titleText, alpha);
            WheelUiGraphicUtility.SetTextAlpha(_amountText, alpha);
        }

        private void ApplyTitle(string displayName, string defaultTitle)
        {
            string title = string.IsNullOrEmpty(displayName) ? defaultTitle : displayName;
            _titleText.text = title.ToUpperInvariant();
            _titleText.enabled = true;
        }

        private static Color ResolveAmountColor(Color accentColor)
        {
            Color color = Color.Lerp(Color.white, accentColor, WheelRewardCardMotion.HaloTintWeight);
            color.a = 1f;
            return color;
        }
    }
}
