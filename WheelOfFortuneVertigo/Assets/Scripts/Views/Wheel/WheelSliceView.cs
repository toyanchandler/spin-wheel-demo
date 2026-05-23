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

        public void Apply(WheelSlicePresentation presentation, Vector2 maxIconSize)
        {
            _root.anchoredPosition = presentation.AnchoredPosition;
            _root.sizeDelta = maxIconSize;
            _iconRoot.sizeDelta = IconFitter.FitWithinBounds(maxIconSize * 0.86f, presentation.Icon);
            _icon.sprite = presentation.Icon;
            _icon.color = Color.white;
            _icon.enabled = presentation.Icon != null;
            ApplyMedallionColors(presentation);
            AmountTable.Apply(_amount, presentation);
            _amountBadge.transform.SetAsLastSibling();
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

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
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
