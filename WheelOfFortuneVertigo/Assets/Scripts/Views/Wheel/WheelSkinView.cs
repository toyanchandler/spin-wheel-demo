using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public enum WheelSkinSlot
    {
        WheelBase,
        Indicator
    }

    /// <summary>
    /// Applies <see cref="WheelZoneSnapshot"/> skin sprites only — no catalog access at runtime.
    /// </summary>
    [RequireComponent(typeof(Image))]
    [WheelBind]
    public sealed class WheelSkinView : MonoBehaviour
    {
        [SerializeField] private WheelSkinSlot _slot;
        [SerializeField] private Image _image;

        [WheelInject] private WheelEventBus _eventBus;

        private readonly WheelIndicatorSpinPresenter _indicatorPresenter = new WheelIndicatorSpinPresenter();

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            if (_slot == WheelSkinSlot.Indicator)
            {
                _indicatorPresenter.Bind(transform as RectTransform);
            }

            _eventBus.ZoneChanged += OnZoneChanged;
            _eventBus.SpinLanded += OnLandingStarted;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.ZoneChanged -= OnZoneChanged;
            _eventBus.SpinLanded -= OnLandingStarted;
        }

        private void LateUpdate()
        {
            if (_slot != WheelSkinSlot.Indicator || _eventBus == null)
            {
                return;
            }

            _indicatorPresenter.LateUpdate(_eventBus.Presentation.Spin);
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            if (_slot == WheelSkinSlot.Indicator)
            {
                _indicatorPresenter.Bind(transform as RectTransform);
            }

            _image.sprite = _slot == WheelSkinSlot.Indicator
                ? snapshot.IndicatorSprite
                : snapshot.WheelBaseSprite;
        }

        private void OnLandingStarted(WheelSpinResult result)
        {
            if (_slot != WheelSkinSlot.Indicator || _eventBus == null)
            {
                return;
            }

            _indicatorPresenter.ApplyLandingTick(_eventBus.Presentation.Spin);
        }

        private void ValidateWiring()
        {
            if (_image == null)
            {
                throw new System.InvalidOperationException(name + " requires an Image reference.");
            }
        }
    }
}
