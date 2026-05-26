using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private RectTransform _iconRoot;
        [SerializeField] private Image _icon;
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
            RequireReferences();
            Vector2 maxIconSize = _root != null ? _root.sizeDelta : Vector2.zero;
            _iconRoot.sizeDelta = WheelSliceIconFitter.FitWithinBounds(maxIconSize, presentation.Icon);
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
            RequireReferences();
            WheelSliceImpactAnimator.PlayBombHitPulse(this, _root);
        }

        public void PlayRewardHitPulse(Color accentColor)
        {
            RequireReferences();
            WheelSliceImpactAnimator.PlayRewardHitPulse(this, _root);
        }

        public void ClearImpactVisual()
        {
            WheelSliceImpactAnimator.Clear(this, _root);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void ApplyUprightPresentation(float wheelLocalEulerZ)
        {
            float counterZ = -NormalizeSignedAngle(wheelLocalEulerZ);
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
            Color badgeColor = presentation.AmountColor;
            badgeColor.a = WheelSliceMotion.AmountBadgeAlpha;
            _amountBadgeBackground.color = new Color(badgeColor.r, badgeColor.g, badgeColor.b, WheelSliceMotion.AmountBadgeBackgroundAlpha);
            _amountBadgeRim.color = new Color(1f, 1f, 1f, WheelSliceMotion.AmountBadgeRimAlpha);
        }

        private void RequireReferences()
        {
            if (_root == null ||
                _iconRoot == null ||
                _icon == null ||
                _amountBadge == null ||
                _amountBadgeBackground == null ||
                _amountBadgeRim == null ||
                _amount == null)
            {
                throw new System.InvalidOperationException(
                    name + " wheel slice prefab has missing serialized references. Rebuild the wheel slice hierarchy.");
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
