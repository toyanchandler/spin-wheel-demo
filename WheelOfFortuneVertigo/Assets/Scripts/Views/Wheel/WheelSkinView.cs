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
    [WheelBind]
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

        [WheelInject] private WheelEventBus _eventBus;
        [WheelInject] private WheelSpinner _spinner;
        private RectTransform _rectTransform;
        private float _indicatorBaseRotation;
        private float _lastWheelAngle;
        private float _indicatorTickOffset;
        private int _lastTickIndex;
        private bool _hasTickSample;
        private bool _hasIndicatorBaseRotation;

        private const float IndicatorTickAngle = 30f;
        private const float IndicatorTickReturnSpeed = 260f;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            ConfigureIndicatorTransform();
            _eventBus.ZoneChanged += OnZoneChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.ZoneChanged -= OnZoneChanged;
        }

        private void LateUpdate()
        {
            if (_slot != WheelSkinSlot.Indicator)
            {
                return;
            }

            if (_spinner == null || _spinner.WheelTransform == null || !_spinner.IsSpinning)
            {
                _hasTickSample = false;
                SetIndicatorTickOffset(Mathf.MoveTowards(_indicatorTickOffset, 0f, IndicatorTickReturnSpeed * Time.deltaTime));
                return;
            }

            int sliceCount = Mathf.Max(1, _spinner.CurrentSliceCount);
            float wheelAngle = Mathf.Repeat(_spinner.WheelTransform.eulerAngles.z, 360f);
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
                _lastWheelAngle = wheelAngle;
                return;
            }

            _lastWheelAngle = wheelAngle;
            SetIndicatorTickOffset(Mathf.MoveTowards(_indicatorTickOffset, 0f, IndicatorTickReturnSpeed * Time.deltaTime));
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            ConfigureIndicatorTransform();
            _image.sprite = SpriteResolvers[(int)_slot](_catalog, snapshot.SkinTier);
        }

        private void ValidateWiring()
        {
            if (_catalog == null || _image == null)
            {
                throw new System.InvalidOperationException(name + " requires WheelSkinCatalog and Image references.");
            }
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

            if (!_hasIndicatorBaseRotation)
            {
                _indicatorBaseRotation = NormalizeSignedAngle(_rectTransform.localEulerAngles.z);
                _hasIndicatorBaseRotation = true;
            }

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
    }
}
