using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public enum WheelSkinSlot
    {
        WheelBase,
        Indicator
    }

    [RequireComponent(typeof(Image))]
    public sealed class WheelSkinView : MonoBehaviour
    {
        private delegate Sprite SkinSpriteResolver(WheelSkinCatalog catalog, WheelSkinTier tier);

        private static readonly SkinSpriteResolver[] SpriteResolvers =
        {
            (catalog, tier) => catalog.GetWheelBase(tier),
            (catalog, tier) => catalog.GetIndicator(tier)
        };

        [SerializeField] private WheelSkinCatalog _catalog;
        [SerializeField] private WheelSkinSlot _slot;
        [SerializeField] private Image _image;

        private WheelEventBus _eventBus;
        private RectTransform _rectTransform;
        private float _indicatorBaseRotation;
        private float _lastWheelAngle;
        private float _indicatorTickOffset;
        private int _lastTickIndex;
        private bool _hasTickSample;

        private const float IndicatorFlippedRotation = 180f;
        private const float IndicatorTickAngle = 17f;
        private const float IndicatorTickReturnSpeed = 220f;

        public void Bind(WheelEventBus eventBus)
        {
            ConfigureIndicatorTransform();
            _eventBus = eventBus;
            _eventBus.ZoneChanged += OnZoneChanged;
        }

        public void Unbind()
        {
            _eventBus.ZoneChanged -= OnZoneChanged;
            _eventBus = null;
        }

        private void LateUpdate()
        {
            if (_slot != WheelSkinSlot.Indicator)
            {
                return;
            }

            WheelSpinner spinner = WheelRuntimeLocator.Spinner;
            if (spinner == null || spinner.WheelTransform == null || !spinner.IsSpinning)
            {
                _hasTickSample = false;
                SetIndicatorTickOffset(Mathf.MoveTowards(_indicatorTickOffset, 0f, IndicatorTickReturnSpeed * Time.deltaTime));
                return;
            }

            int sliceCount = Mathf.Max(1, spinner.CurrentSliceCount);
            float wheelAngle = Mathf.Repeat(spinner.WheelTransform.eulerAngles.z, 360f);
            int tickIndex = Mathf.FloorToInt(wheelAngle / (360f / sliceCount));

            if (!_hasTickSample)
            {
                _lastWheelAngle = wheelAngle;
                _lastTickIndex = tickIndex;
                _hasTickSample = true;
                return;
            }

            if (tickIndex != _lastTickIndex)
            {
                float spinDirection = Mathf.Sign(Mathf.DeltaAngle(_lastWheelAngle, wheelAngle));
                if (Mathf.Approximately(spinDirection, 0f))
                {
                    spinDirection = 1f;
                }

                SetIndicatorTickOffset(spinDirection * IndicatorTickAngle);
                _lastTickIndex = tickIndex;
            }

            _lastWheelAngle = wheelAngle;
            SetIndicatorTickOffset(Mathf.MoveTowards(_indicatorTickOffset, 0f, IndicatorTickReturnSpeed * Time.deltaTime));
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            ConfigureIndicatorTransform();
            _image.sprite = SpriteResolvers[(int)_slot](_catalog, snapshot.SkinTier);
        }

        private void ConfigureIndicatorTransform()
        {
            if (_slot != WheelSkinSlot.Indicator)
            {
                return;
            }

            if (_rectTransform == null)
            {
                _rectTransform = transform as RectTransform;
            }

            if (_rectTransform == null)
            {
                return;
            }

            _rectTransform.pivot = new Vector2(0.5f, 1f);
            _indicatorBaseRotation = IndicatorFlippedRotation;
            SetIndicatorTickOffset(_indicatorTickOffset);
        }

        private void SetIndicatorTickOffset(float offset)
        {
            _indicatorTickOffset = offset;
            if (_rectTransform != null)
            {
                _rectTransform.localRotation = Quaternion.Euler(0f, 0f, _indicatorBaseRotation + _indicatorTickOffset);
            }
        }
    }
}
