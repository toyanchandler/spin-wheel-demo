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
        [SerializeField] private GameObject _amountBadge;
        [SerializeField] private WheelCircleGraphic _amountBadgeBackground;
        [SerializeField] private WheelCircleGraphic _amountBadgeRim;
        [SerializeField] private TextMeshProUGUI _amount;

        private bool _showsAmountBadge;

        public RectTransform IconRect => _icon.rectTransform;
        public Sprite IconSprite => _icon.sprite;
        public Color IconColor => _icon.color;
        public float PointerAngle
        {
            get
            {
                Vector2 position = _root.anchoredPosition;
                // UI wheel convention: 0° points up, positive angle follows wheel layout (Atan2(x, y), not Atan2(y, x)).
                return Mathf.Atan2(position.x, position.y) * Mathf.Rad2Deg;
            }
        }

        private void Awake()
        {
            RequireReferences();
        }

        public void Apply(WheelSlicePresentation presentation)
        {
            Vector2 maxIconSize = _root.sizeDelta;
            _iconRoot.sizeDelta = WheelSliceIconFitter.FitWithinBounds(maxIconSize, presentation.Icon);
            _icon.sprite = presentation.Icon;
            _icon.color = Color.white;
            _icon.enabled = presentation.Icon != null;
            ApplyMedallionColors(presentation);
            ApplyAmountLabel(_amount, presentation);
            _showsAmountBadge = presentation.ShowAmountLabel;
            _amountBadge.SetActive(_showsAmountBadge);
            gameObject.SetActive(true);
        }

        public void SetRewardVisualVisible(bool isVisible)
        {
            _icon.enabled = isVisible && _icon.sprite != null;
            _amountBadge.SetActive(isVisible && _showsAmountBadge);
        }

        public void PlayBombHitPulse()
        {
            WheelSliceImpactAnimator.PlayBombHitPulse(this, _root);
        }

        public void PlayRewardHitPulse(Color accentColor)
        {
            WheelSliceImpactAnimator.PlayRewardHitPulse(this, _root);
        }

        public void ClearImpactVisual()
        {
            WheelSliceImpactAnimator.Clear(this, _root);
        }

        public void Hide()
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        /// <summary>
        /// Counter-rotates icon and amount badge so they stay upright while the wheel root spins.
        /// Assumes slice prefab hierarchy: wheel root → slice root → icon/badge transforms.
        /// </summary>
        public void ApplyCounterRotationForWheelRoot(float wheelRootLocalEulerZ)
        {
            float counterZ = -WheelAngleUtility.NormalizeSignedAngle(wheelRootLocalEulerZ);
            ApplyUprightLocalZ(_iconRoot, counterZ);
            ApplyUprightLocalZ(_amountBadge.transform, counterZ);
        }

        private static void ApplyUprightLocalZ(Transform target, float localZ)
        {
            Vector3 euler = target.localEulerAngles;
            target.localEulerAngles = new Vector3(euler.x, euler.y, localZ);
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
                _amount == null) throw new System.InvalidOperationException(
                    name + " wheel slice prefab has missing serialized references. Rebuild the wheel slice hierarchy.");
        }

        private static void ApplyAmountLabel(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
        {
            if (!presentation.ShowAmountLabel)
            {
                amountLabel.text = string.Empty;
                amountLabel.enabled = false;
                return;
            }

            amountLabel.SetText(presentation.AmountText);
            amountLabel.color = Color.white;
            amountLabel.enabled = true;
        }
    }
}
