using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelLootCardBinding
    {
        private readonly Transform _rootTransform;
        private readonly Image _shadowImage;
        private readonly Image _cardImage;
        private readonly Image _iconImage;
        private readonly TextMeshProUGUI _titleText;
        private readonly TextMeshProUGUI _amountText;
        private readonly CanvasGroup _canvasGroup;
        private readonly Image _glowImage;
        private readonly Image _shineImage;
        private readonly Sprite _cardArtSprite;
        private Vector2 _shineHomePosition;
        private bool _hasShineHomePosition;

        public Transform RootTransform => _rootTransform;
        public Image ShadowImage => _shadowImage;
        public Image GlowImage => _glowImage;
        public Image ShineImage => _shineImage;
        public Image IconImage => _iconImage;
        public CanvasGroup CanvasGroup => _canvasGroup;
        public Vector2 ShineHomePosition => _shineHomePosition;

        public WheelLootCardBinding(
            Transform rootTransform,
            Image shadowImage,
            Image cardImage,
            Image iconImage,
            TextMeshProUGUI titleText,
            TextMeshProUGUI amountText,
            CanvasGroup canvasGroup,
            Image glowImage,
            Image shineImage,
            Sprite cardArtSprite)
        {
            _rootTransform = rootTransform;
            _shadowImage = shadowImage;
            _cardImage = cardImage;
            _iconImage = iconImage;
            _titleText = titleText;
            _amountText = amountText;
            _canvasGroup = canvasGroup;
            _glowImage = glowImage;
            _shineImage = shineImage;
            _cardArtSprite = cardArtSprite;
        }

        public void ApplyFrame(Sprite cardFrameSprite)
        {
            Sprite sprite = _cardArtSprite != null ? _cardArtSprite : cardFrameSprite;
            if (sprite != null) _cardImage.sprite = sprite;
            _cardImage.color = Color.white;
        }

        public void ApplyContent(WheelRewardCardPresentation presentation)
        {
            _iconImage.sprite = presentation.Icon;
            _iconImage.enabled = presentation.Icon != null;
            _iconImage.color = Color.white;
            _titleText.text = presentation.TitleText;
            _titleText.enabled = true;

            if (!presentation.ShowAmountLabel)
            {
                _amountText.text = string.Empty;
                _amountText.enabled = false;
                return;
            }

            _amountText.SetText(presentation.AmountText);
            _amountText.color = presentation.AmountColor;
            _amountText.enabled = true;
        }

        public void PrepareEntrance(WheelRewardCardPresentation presentation)
        {
            _canvasGroup.alpha = 0f;
            SetShadowAlpha(0f);
            PrepareGlow(presentation.AccentColor);
            PrepareCard();
            CaptureShineHome();
            PrepareShine();
            _rootTransform.localScale = WheelLootCardMotion.EntranceStartScale;
            _rootTransform.localRotation = Quaternion.Euler(WheelLootCardMotion.EntranceStartRotation);
            _iconImage.transform.localScale = WheelLootCardMotion.IconEntranceStartScale;
        }

        public void ShowWithoutEntrance()
        {
            _rootTransform.localScale = Vector3.one;
            _rootTransform.localRotation = Quaternion.identity;
            SetShadowAlpha(WheelLootCardMotion.RestingShadowAlpha);
            _canvasGroup.alpha = 1f;
            WheelUiGraphicUtility.SetGraphicAlpha(_glowImage, WheelLootCardMotion.RestingGlowAlpha);
            _glowImage.rectTransform.localScale = Vector3.one;
            ResetShine();
        }

        public void Hide()
        {
            _canvasGroup.alpha = 0f;
            ResetTransientVfx();
        }

        public void ResetTransientVfx()
        {
            WheelUiGraphicUtility.SetGraphicAlpha(_shineImage, 0f);
            ResetShine();
            SetShadowAlpha(0f);
        }

        public void SetLostVisualState(bool isLost)
        {
            _canvasGroup.alpha = isLost ? WheelLootCardMotion.LostCardAlpha : 1f;

            if (isLost)
            {
                _cardImage.color = WheelLootCardMotion.LostCardColor;
                _glowImage.color = WheelLootCardMotion.LostGlowColor;
            }
        }

        public void PrepareLandingPulse()
        {
            _rootTransform.localScale = Vector3.one;
            _rootTransform.localRotation = Quaternion.identity;
        }

        public TweenState CaptureShimmerState()
        {
            CaptureShineHome();
            RectTransform shineRect = _shineImage.rectTransform;
            float travel = Mathf.Max(WheelLootCardMotion.MinimumShimmerTravel, ((RectTransform)_rootTransform).rect.width * WheelLootCardMotion.ShimmerTravelWidthMultiplier);
            shineRect.localRotation = Quaternion.Euler(WheelLootCardMotion.ShimmerRotation);
            shineRect.anchoredPosition = _shineHomePosition + WheelLootCardMotion.ShimmerStartOffset(travel);
            return new TweenState(_shineHomePosition, travel);
        }

        public void CompleteShimmer()
        {
            ResetShine();
        }

        public void PrepareGlowTimeline()
        {
            _glowImage.rectTransform.localScale = Vector3.one;
        }

        public void SetShadowAlpha(float alpha)
        {
            WheelUiGraphicUtility.SetGraphicAlpha(_shadowImage, alpha);
        }

        private void PrepareGlow(Color accentColor)
        {
            Color glowColor = accentColor;
            glowColor.a = WheelLootCardMotion.PreparedGlowAlpha;
            _glowImage.color = glowColor;
            _glowImage.enabled = true;
            _glowImage.rectTransform.localScale = Vector3.one;
        }

        private void PrepareCard()
        {
            _cardImage.color = Color.white;
            _cardImage.enabled = true;
        }

        private void CaptureShineHome()
        {
            if (_hasShineHomePosition) return;

            _shineHomePosition = _shineImage.rectTransform.anchoredPosition;
            _hasShineHomePosition = true;
        }

        private void PrepareShine()
        {
            WheelUiGraphicUtility.SetGraphicAlpha(_shineImage, 0f);
            _shineImage.enabled = true;
            _shineImage.rectTransform.anchoredPosition = _shineHomePosition;
            _shineImage.rectTransform.localRotation = Quaternion.Euler(WheelLootCardMotion.ShimmerRotation);
            _shineImage.rectTransform.localScale = Vector3.one;
        }

        private void ResetShine()
        {
            if (!_hasShineHomePosition) return;

            _shineImage.rectTransform.anchoredPosition = _shineHomePosition;
        }

        internal readonly struct TweenState
        {
            public readonly Vector2 Home;
            public readonly float Travel;

            public TweenState(Vector2 home, float travel)
            {
                Home = home;
                Travel = travel;
            }
        }
    }
}
