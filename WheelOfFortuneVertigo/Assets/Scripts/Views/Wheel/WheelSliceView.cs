using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _iconRoot;
        [SerializeField] private Image _icon;
        [SerializeField] private WheelCircleGraphic _background;
        [SerializeField] private WheelCircleGraphic _rim;
        [SerializeField] private GameObject _amountBadge;
        [SerializeField] private WheelCircleGraphic _amountBadgeBackground;
        [SerializeField] private WheelCircleGraphic _amountBadgeRim;
        [SerializeField] private TextMeshProUGUI _amount;

        private bool _showsAmountBadge;

        public RectTransform IconRect { get { return _icon.rectTransform; } }
        public Sprite IconSprite { get { return _icon != null ? _icon.sprite : null; } }
        public Color IconColor { get { return _icon != null ? _icon.color : Color.white; } }
        public float PointerAngle
        {
            get
            {
                Vector2 position = _root != null ? _root.anchoredPosition : Vector2.zero;
                return Mathf.Atan2(position.x, position.y) * Mathf.Rad2Deg;
            }
        }

        public void Apply(WheelSlicePresentation presentation)
        {
            Vector2 maxIconSize = _root != null ? _root.sizeDelta : Vector2.zero;
            _iconRoot.sizeDelta = IconFitter.FitWithinBounds(maxIconSize * 0.86f, presentation.Icon);
            _icon.sprite = presentation.Icon;
            _icon.color = Color.white;
            _icon.enabled = presentation.Icon != null;
            ApplyMedallionColors(presentation);
            AmountTable.Apply(_amount, presentation);
            _showsAmountBadge = presentation.ShowAmountLabel;
            _amountBadge.SetActive(_showsAmountBadge);
            gameObject.SetActive(true);
        }

        public void SetRewardVisualVisible(bool isVisible)
        {
            if (_icon != null)
            {
                _icon.enabled = isVisible && _icon.sprite != null;
            }

            if (_amountBadge != null)
            {
                _amountBadge.SetActive(isVisible && _showsAmountBadge);
            }
        }

        public void PlayBombHitPulse()
        {
            DOTween.Kill(this);
            _root.DOKill();
            _root.localScale = Vector3.one;
            ConfigureImpactCircle(_background, new Color(1f, 0.12f, 0.02f, 0.66f), new Vector3(1.02f, 1.02f, 1f));
            ConfigureImpactCircle(_rim, new Color(1f, 0.54f, 0.10f, 1f), new Vector3(1.18f, 1.18f, 1f));

            DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .Append(_root.DOScale(new Vector3(1.18f, 1.18f, 1f), 0.11f).SetEase(Ease.OutQuad))
                .Join(FadeCircle(_rim, 1f, 0.08f))
                .Join(FadeCircle(_background, 0.70f, 0.08f))
                .Append(_root.DOScale(new Vector3(0.98f, 0.98f, 1f), 0.10f).SetEase(Ease.InOutSine))
                .Append(_root.DOScale(new Vector3(1.12f, 1.12f, 1f), 0.10f).SetEase(Ease.OutQuad))
                .Join(ScaleCircle(_rim, new Vector3(1.60f, 1.60f, 1f), 0.16f))
                .Append(_root.DOScale(Vector3.one, 0.16f).SetEase(Ease.OutBack))
                .Join(FadeCircle(_rim, 1f, 0.20f))
                .Join(FadeCircle(_background, 0.58f, 0.20f));
        }

        public void PlayRewardHitPulse(Color accentColor)
        {
            DOTween.Kill(this);
            _root.DOKill();
            _root.localScale = Vector3.one;

            Color glowColor = accentColor.maxColorComponent > 0.03f ? accentColor : new Color(0.22f, 0.92f, 1f, 1f);
            glowColor.a = 0.76f;
            Color rimColor = Color.Lerp(glowColor, new Color(1f, 0.78f, 0.22f, 1f), 0.48f);
            rimColor.a = 1f;

            ConfigureImpactCircle(_background, glowColor, new Vector3(1.02f, 1.02f, 1f));
            ConfigureImpactCircle(_rim, rimColor, new Vector3(1.20f, 1.20f, 1f));

            DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .Append(_root.DOScale(new Vector3(1.18f, 1.18f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .Join(FadeCircle(_rim, 1f, 0.10f))
                .Join(FadeCircle(_background, 0.78f, 0.10f))
                .Append(_root.DOScale(new Vector3(0.96f, 0.96f, 1f), 0.09f).SetEase(Ease.InOutSine))
                .Append(_root.DOScale(new Vector3(1.12f, 1.12f, 1f), 0.11f).SetEase(Ease.OutQuad))
                .Join(ScaleCircle(_rim, new Vector3(1.62f, 1.62f, 1f), 0.20f))
                .Append(_root.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack))
                .Join(FadeCircle(_background, 0.30f, 0.22f))
                .Join(FadeCircle(_rim, 0.46f, 0.22f));
        }

        public void ClearImpactVisual()
        {
            DOTween.Kill(this);
            _root.DOKill();
            _root.localScale = Vector3.one;
            SetCircleVisible(_background, false);
            SetCircleVisible(_rim, false);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private static void ConfigureImpactCircle(WheelCircleGraphic graphic, Color color, Vector3 scale)
        {
            if (graphic == null)
            {
                return;
            }

            graphic.gameObject.SetActive(true);
            graphic.color = color;
            graphic.rectTransform.localScale = scale;
        }

        private static Tween FadeCircle(WheelCircleGraphic graphic, float alpha, float duration)
        {
            return graphic == null
                ? DOVirtual.DelayedCall(duration, () => { }, false)
                : graphic.DOFade(alpha, duration);
        }

        private static Tween ScaleCircle(WheelCircleGraphic graphic, Vector3 scale, float duration)
        {
            return graphic == null
                ? DOVirtual.DelayedCall(duration, () => { }, false)
                : graphic.rectTransform.DOScale(scale, duration).SetEase(Ease.OutQuad);
        }

        public void ApplyUprightPresentation()
        {
            WheelSpinner spinner = WheelRuntimeLocator.Spinner;
            if (spinner == null || spinner.WheelTransform == null)
            {
                return;
            }

            float counterZ = -NormalizeSignedAngle(spinner.WheelTransform.localEulerAngles.z);
            ApplyUprightLocalZ(_background != null ? _background.rectTransform : null, counterZ);
            ApplyUprightLocalZ(_rim != null ? _rim.rectTransform : null, counterZ);
            ApplyUprightLocalZ(_iconRoot, counterZ);
            if (_amountBadge != null)
            {
                ApplyUprightLocalZ(_amountBadge.transform, counterZ);
            }
        }

        private static void ApplyUprightLocalZ(Transform target, float localZ)
        {
            if (target == null)
            {
                return;
            }

            Vector3 euler = target.localEulerAngles;
            target.localEulerAngles = new Vector3(euler.x, euler.y, localZ);
        }

        private static float NormalizeSignedAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }

        private void ApplyMedallionColors(WheelSlicePresentation presentation)
        {
            Color rimColor = presentation.AmountColor;
            rimColor.a = 0.92f;
            SetCircleVisible(_rim, false);
            SetCircleVisible(_background, false);
            _amountBadgeBackground.color = new Color(rimColor.r, rimColor.g, rimColor.b, 0.96f);
            _amountBadgeRim.color = new Color(1f, 1f, 1f, 0.88f);
        }

        private static void SetCircleVisible(WheelCircleGraphic graphic, bool isVisible)
        {
            if (graphic != null && graphic.gameObject.activeSelf != isVisible)
            {
                graphic.gameObject.SetActive(isVisible);
            }
        }

        private static class IconFitter
        {
            public static Vector2 FitWithinBounds(Vector2 maxSize, Sprite sprite)
            {
                if (sprite == null)
                {
                    return maxSize;
                }

                float spriteWidth = sprite.rect.width;
                float spriteHeight = sprite.rect.height;
                if (spriteWidth <= 0f || spriteHeight <= 0f)
                {
                    return maxSize;
                }

                float spriteAspect = spriteWidth / spriteHeight;
                float boundsAspect = maxSize.x / maxSize.y;
                if (spriteAspect > boundsAspect)
                {
                    return new Vector2(maxSize.x, maxSize.x / spriteAspect);
                }

                return new Vector2(maxSize.y * spriteAspect, maxSize.y);
            }
        }

        private static class AmountTable
        {
            private static readonly System.Action<TextMeshProUGUI, WheelSlicePresentation>[] ApplyActions =
            {
                HideAmount,
                ShowAmount
            };

            public static void Apply(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
            {
                ApplyActions[System.Convert.ToInt32(presentation.ShowAmountLabel)](amountLabel, presentation);
            }

            private static void HideAmount(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
            {
                amountLabel.text = string.Empty;
                amountLabel.enabled = false;
            }

            private static void ShowAmount(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
            {
                amountLabel.SetText("{0}", presentation.Amount);
                amountLabel.color = Color.white;
                amountLabel.enabled = true;
            }
        }
    }
}
