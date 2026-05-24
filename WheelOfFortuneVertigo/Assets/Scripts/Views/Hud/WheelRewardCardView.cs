using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardCardView : MonoBehaviour
    {
        private const float OpeningAmountBottomOffset = 78f;
        private const float OpeningAmountWidth = 272f;
        private const float OpeningAmountHeight = 54f;

        [SerializeField] private Image _shadowImage;
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _frontPanelImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private CanvasGroup _frontGroup;
        [SerializeField] private CanvasGroup _backGroup;
        [SerializeField] private Image _backImage;
        [SerializeField] private Image _backGlowImage;
        [SerializeField] private Image _glowImage;
        [SerializeField] private Image _shineImage;
        [SerializeField] private Image _sparkImage;

        private Vector2 _shineHomePosition;
        private bool _hasShineHomePosition;

        public void ApplyFrames(Sprite cardFrameSprite)
        {
            _frameImage.sprite = cardFrameSprite;
            _frameImage.color = Color.white;
            _frameImage.maskable = true;
        }

        public void Apply(RewardInventoryEntry entry)
        {
            Apply(entry, 0, false, true);
        }

        public void Apply(RewardInventoryEntry entry, int displayIndex, bool featured)
        {
            Apply(entry, displayIndex, featured, true);
        }

        public void Apply(RewardInventoryEntry entry, int displayIndex, bool featured, bool animated)
        {
            _iconImage.sprite = entry.Icon;
            _iconImage.color = Color.white;
            _iconImage.preserveAspect = true;
            _iconImage.maskable = true;
            _amountText.maskable = true;
            AmountTable.Apply(_amountText, entry.Amount, entry.AccentColor);
            ApplyOpeningAmountLayout();
            gameObject.SetActive(true);
            PreparePremiumChrome(entry, featured);
            if (animated)
            {
                PlayEntrance(displayIndex, featured, entry.AccentColor);
            }
            else
            {
                ShowWithoutEntrance(featured);
            }
        }

        public void PlayLandingPulse(float delay)
        {
            DOTween.Kill(this);
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(transform.DOScale(new Vector3(1.14f, 1.14f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .Join(GlowFade(0.44f, 0.12f))
                .Append(transform.DOScale(Vector3.one, 0.20f).SetEase(Ease.OutBack))
                .Join(GlowFade(0.18f, 0.24f))
                .Join(PlayShimmer(0.02f, 0.34f));
        }

        public void Hide()
        {
            transform.DOKill();
            DOTween.Kill(this);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            SetCanvasAlpha(_frontGroup, 1f);
            SetCanvasAlpha(_backGroup, 0f);
            SetShadowAlpha(0f);
            ResetVfxAlpha();
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            transform.DOKill();
            DOTween.Kill(this);
            ResetVfxAlpha();
        }

        private void PreparePremiumChrome(RewardInventoryEntry entry, bool featured)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            SetCanvasAlpha(_frontGroup, 1f);
            SetCanvasAlpha(_backGroup, 0f);
            SetShadowAlpha(0f);
            if (_glowImage != null)
            {
                Color glowColor = entry.AccentColor;
                glowColor.a = featured ? 0.30f : 0.12f;
                _glowImage.color = glowColor;
                _glowImage.enabled = true;
                _glowImage.maskable = true;
                _glowImage.rectTransform.localScale = featured ? new Vector3(1.18f, 1.18f, 1f) : Vector3.one;
            }

            if (_frameImage != null)
            {
                Color frameColor = featured
                    ? Color.Lerp(Color.white, entry.AccentColor, 0.10f)
                    : Color.white;
                frameColor.a = featured ? 0.92f : 1f;
                _frameImage.color = frameColor;
            }

            if (_frontPanelImage != null)
            {
                Color panelColor = featured
                    ? Color.Lerp(new Color(0.07f, 0.20f, 0.24f, 0.72f), entry.AccentColor, 0.10f)
                    : new Color(0.06f, 0.18f, 0.22f, 0.58f);
                _frontPanelImage.color = panelColor;
                _frontPanelImage.enabled = true;
                _frontPanelImage.maskable = true;
            }

            if (_shineImage != null)
            {
                if (!_hasShineHomePosition)
                {
                    _shineHomePosition = _shineImage.rectTransform.anchoredPosition;
                    _hasShineHomePosition = true;
                }

                Color shineColor = Color.white;
                shineColor.a = 0f;
                _shineImage.color = shineColor;
                _shineImage.enabled = true;
                _shineImage.maskable = true;
                _shineImage.rectTransform.anchoredPosition = _shineHomePosition;
                _shineImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -12f);
                _shineImage.rectTransform.localScale = Vector3.one;
            }

            if (_backImage != null)
            {
                Color backColor = Color.Lerp(Color.white, entry.AccentColor, featured ? 0.18f : 0.08f);
                backColor.a = 1f;
                _backImage.color = backColor;
                _backImage.enabled = true;
                _backImage.maskable = true;
            }

            if (_backGlowImage != null)
            {
                Color backGlowColor = entry.AccentColor;
                backGlowColor.a = 0f;
                _backGlowImage.color = backGlowColor;
                _backGlowImage.enabled = true;
                _backGlowImage.maskable = true;
                _backGlowImage.rectTransform.localScale = Vector3.one;
            }
        }

        private void PlayEntrance(int displayIndex, bool featured, Color accentColor)
        {
            DOTween.Kill(this);
            transform.DOKill();
            if (featured && _frontGroup != null && _backGroup != null)
            {
                PlayFlipReveal(displayIndex, accentColor);
                return;
            }

            SetCanvasAlpha(_frontGroup, 1f);
            SetCanvasAlpha(_backGroup, 0f);
            float delay = Mathf.Min(0.42f, displayIndex * (featured ? 0.045f : 0.025f));
            float startScale = featured ? 0.72f : 0.86f;
            transform.localScale = new Vector3(startScale, startScale, 1f);
            transform.localRotation = Quaternion.Euler(0f, featured ? -9f : -4f, featured ? -2.5f : 0f);

            Sequence sequence = DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(FadeGroup(1f, featured ? 0.18f : 0.12f))
                .Join(transform.DOScale(Vector3.one, featured ? 0.34f : 0.20f).SetEase(Ease.OutBack))
                .Join(transform.DOLocalRotate(Vector3.zero, featured ? 0.34f : 0.18f, RotateMode.Fast).SetEase(Ease.OutCubic))
                .Join(GlowPulse(featured))
                .Append(PlayShimmer(featured ? 0.03f : 0f, featured ? 0.46f : 0.28f));

            if (_iconImage != null)
            {
                _iconImage.transform.localScale = featured ? new Vector3(0.86f, 0.86f, 1f) : new Vector3(0.92f, 0.92f, 1f);
                sequence.Join(_iconImage.transform.DOScale(Vector3.one, featured ? 0.32f : 0.18f).SetEase(Ease.OutBack));
            }
        }

        private void PlayFlipReveal(int displayIndex, Color accentColor)
        {
            float delay = Mathf.Min(2.35f, displayIndex * 0.165f);
            transform.localScale = new Vector3(0.62f, 0.62f, 1f);
            transform.localRotation = Quaternion.Euler(0f, -74f, -8f);
            SetCanvasAlpha(_frontGroup, 0f);
            SetCanvasAlpha(_backGroup, 1f);
            SetBackGlowAlpha(0f);
            PrepareShadow();

            Sequence sequence = DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(FadeGroup(1f, 0.10f))
                .Join(ShadowFade(0.38f, 0.16f))
                .Join(transform.DOScale(new Vector3(0.94f, 0.94f, 1f), 0.20f).SetEase(Ease.OutCubic))
                .Join(transform.DOLocalRotate(new Vector3(0f, -22f, -3.2f), 0.22f, RotateMode.Fast).SetEase(Ease.OutCubic))
                .Join(BackGlowPulse(accentColor))
                .Append(transform.DOLocalRotate(new Vector3(0f, 92f, 2.4f), 0.20f, RotateMode.Fast).SetEase(Ease.InCubic))
                .Join(transform.DOScale(new Vector3(1.16f, 1.16f, 1f), 0.20f).SetEase(Ease.InQuad))
                .Join(ShadowFade(0.58f, 0.18f))
                .AppendCallback(ShowFrontAfterFlip)
                .Append(transform.DOLocalRotate(new Vector3(0f, -12f, 0.8f), 0.18f, RotateMode.Fast).SetEase(Ease.OutCubic))
                .Join(transform.DOScale(new Vector3(1.06f, 1.06f, 1f), 0.18f).SetEase(Ease.OutQuad))
                .Append(transform.DOLocalRotate(Vector3.zero, 0.28f, RotateMode.Fast).SetEase(Ease.OutBack))
                .Join(transform.DOScale(Vector3.one, 0.30f).SetEase(Ease.OutBack))
                .Join(ShadowFade(0.30f, 0.34f))
                .Join(GlowPulse(true))
                .Join(SparkPop())
                .Append(PlayShimmer(0.00f, 0.52f))
                .Append(PlayShimmer(0.06f, 0.38f));

            if (_iconImage != null)
            {
                _iconImage.transform.localScale = new Vector3(0.42f, 0.42f, 1f);
                sequence.Insert(delay + 0.56f, _iconImage.transform.DOScale(new Vector3(1.10f, 1.10f, 1f), 0.20f).SetEase(Ease.OutBack));
                sequence.Insert(delay + 0.76f, _iconImage.transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutQuad));
            }

            if (_amountText != null)
            {
                _amountText.transform.localScale = new Vector3(0.82f, 0.82f, 1f);
                sequence.Insert(delay + 0.66f, _amountText.transform.DOScale(new Vector3(1.12f, 1.12f, 1f), 0.18f).SetEase(Ease.OutBack));
                sequence.Insert(delay + 0.84f, _amountText.transform.DOScale(Vector3.one, 0.16f).SetEase(Ease.OutQuad));
            }
        }

        private void ShowFrontAfterFlip()
        {
            SetCanvasAlpha(_backGroup, 0f);
            SetCanvasAlpha(_frontGroup, 1f);
            transform.localRotation = Quaternion.Euler(0f, -86f, 2f);
            transform.localScale = new Vector3(1.08f, 1.08f, 1f);
        }

        private void ShowWithoutEntrance(bool featured)
        {
            DOTween.Kill(this);
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;
            SetCanvasAlpha(_frontGroup, 1f);
            SetCanvasAlpha(_backGroup, 0f);
            PrepareShadow();
            SetShadowAlpha(featured ? 0.24f : 0.10f);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }

            if (_glowImage != null)
            {
                Color glowColor = _glowImage.color;
                glowColor.a = featured ? 0.22f : 0.10f;
                _glowImage.color = glowColor;
                _glowImage.rectTransform.localScale = featured ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one;
            }

            ResetShine();
        }

        private void ApplyOpeningAmountLayout()
        {
            if (_amountText == null || _backGroup == null)
            {
                return;
            }

            RectTransform rect = _amountText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, OpeningAmountBottomOffset);
            rect.sizeDelta = new Vector2(OpeningAmountWidth, OpeningAmountHeight);
        }

        private Tween FadeGroup(float targetAlpha, float duration)
        {
            if (_canvasGroup == null)
            {
                return DOVirtual.DelayedCall(duration, () => { }, false);
            }

            return _canvasGroup.DOFade(targetAlpha, duration);
        }

        private Tween GlowPulse(bool featured)
        {
            if (_glowImage == null)
            {
                return DOVirtual.DelayedCall(0f, () => { }, false);
            }

            float peakAlpha = featured ? 0.42f : 0.22f;
            float settleAlpha = featured ? 0.22f : 0.10f;
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(GlowFade(peakAlpha, featured ? 0.18f : 0.12f))
                .Join(_glowImage.rectTransform.DOScale(featured ? new Vector3(1.22f, 1.22f, 1f) : new Vector3(1.10f, 1.10f, 1f), featured ? 0.22f : 0.14f).SetEase(Ease.OutQuad))
                .Append(GlowFade(settleAlpha, featured ? 0.42f : 0.24f))
                .Join(_glowImage.rectTransform.DOScale(featured ? new Vector3(1.08f, 1.08f, 1f) : Vector3.one, featured ? 0.42f : 0.24f).SetEase(Ease.OutCubic));
        }

        private Tween GlowFade(float targetAlpha, float duration)
        {
            if (_glowImage == null)
            {
                return DOVirtual.DelayedCall(duration, () => { }, false);
            }

            return _glowImage.DOFade(targetAlpha, duration);
        }

        private Tween BackGlowPulse(Color accentColor)
        {
            if (_backGlowImage == null)
            {
                return DOVirtual.DelayedCall(0f, () => { }, false);
            }

            Color glowColor = accentColor;
            glowColor.a = 0f;
            _backGlowImage.color = glowColor;
            _backGlowImage.rectTransform.localScale = Vector3.one;
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(_backGlowImage.DOFade(0.36f, 0.14f))
                .Join(_backGlowImage.rectTransform.DOScale(new Vector3(1.22f, 1.22f, 1f), 0.20f).SetEase(Ease.OutQuad))
                .Append(_backGlowImage.DOFade(0.10f, 0.18f))
                .Join(_backGlowImage.rectTransform.DOScale(new Vector3(1.05f, 1.05f, 1f), 0.20f).SetEase(Ease.OutCubic));
        }

        private Tween SparkPop()
        {
            if (_sparkImage == null)
            {
                return DOVirtual.DelayedCall(0f, () => { }, false);
            }

            RectTransform sparkRect = _sparkImage.rectTransform;
            sparkRect.localScale = new Vector3(0.55f, 0.55f, 1f);
            sparkRect.localRotation = Quaternion.identity;
            _sparkImage.DOFade(0f, 0f);
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(_sparkImage.DOFade(0.78f, 0.08f))
                .Join(sparkRect.DOScale(new Vector3(1.52f, 1.52f, 1f), 0.24f).SetEase(Ease.OutQuad))
                .Join(sparkRect.DOLocalRotate(new Vector3(0f, 0f, 42f), 0.28f, RotateMode.Fast).SetEase(Ease.OutCubic))
                .Append(_sparkImage.DOFade(0f, 0.20f));
        }

        private void PrepareShadow()
        {
            if (_shadowImage == null)
            {
                return;
            }

            RectTransform shadowRect = _shadowImage.rectTransform;
            shadowRect.anchoredPosition = new Vector2(18f, -24f);
            shadowRect.localRotation = Quaternion.Euler(0f, 0f, -2f);
            shadowRect.localScale = new Vector3(1.02f, 1.04f, 1f);
        }

        private Tween ShadowFade(float targetAlpha, float duration)
        {
            if (_shadowImage == null)
            {
                return DOVirtual.DelayedCall(duration, () => { }, false);
            }

            return _shadowImage.DOFade(targetAlpha, duration);
        }

        private void SetBackGlowAlpha(float alpha)
        {
            if (_backGlowImage == null)
            {
                return;
            }

            Color color = _backGlowImage.color;
            color.a = alpha;
            _backGlowImage.color = color;
        }

        private Tween PlayShimmer(float delay, float duration)
        {
            if (_shineImage == null)
            {
                return DOVirtual.DelayedCall(delay + duration, () => { }, false);
            }

            RectTransform shineRect = _shineImage.rectTransform;
            Vector2 home = _hasShineHomePosition ? _shineHomePosition : shineRect.anchoredPosition;
            float travel = Mathf.Max(220f, ((RectTransform)transform).rect.width * 1.22f);
            shineRect.localRotation = Quaternion.Euler(0f, 0f, -12f);
            shineRect.anchoredPosition = home + new Vector2(-travel, -12f);
            return DOTween.Sequence()
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(_shineImage.DOFade(0.66f, 0.08f))
                .Join(shineRect.DOAnchorPos(home + new Vector2(travel, 14f), duration).SetEase(Ease.InOutSine))
                .Append(_shineImage.DOFade(0f, 0.12f))
                .OnComplete(() => shineRect.anchoredPosition = home);
        }

        private void ResetVfxAlpha()
        {
            if (_shineImage != null)
            {
                Color shineColor = _shineImage.color;
                shineColor.a = 0f;
                _shineImage.color = shineColor;
                ResetShine();
            }

            SetBackGlowAlpha(0f);
            SetShadowAlpha(0f);
            SetGraphicAlpha(_sparkImage, 0f);
        }

        private void ResetShine()
        {
            if (_shineImage == null || !_hasShineHomePosition)
            {
                return;
            }

            _shineImage.rectTransform.anchoredPosition = _shineHomePosition;
        }

        private static void SetCanvasAlpha(CanvasGroup group, float alpha)
        {
            if (group != null)
            {
                group.alpha = alpha;
            }
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private void SetShadowAlpha(float alpha)
        {
            SetGraphicAlpha(_shadowImage, alpha);
        }

        private static class AmountTable
        {
            private static readonly System.Action<TextMeshProUGUI, int, Color>[] ApplyActions =
            {
                HideAmount,
                ShowAmount
            };

            public static void Apply(TextMeshProUGUI amountText, int amount, Color accentColor)
            {
                ApplyActions[System.Convert.ToInt32(amount > 1)](amountText, amount, accentColor);
            }

            private static void HideAmount(TextMeshProUGUI amountText, int amount, Color accentColor)
            {
                amountText.text = string.Empty;
                amountText.enabled = false;
            }

            private static void ShowAmount(TextMeshProUGUI amountText, int amount, Color accentColor)
            {
                amountText.SetText("{0}", amount);
                amountText.color = accentColor;
                amountText.enabled = true;
            }
        }
    }
}
