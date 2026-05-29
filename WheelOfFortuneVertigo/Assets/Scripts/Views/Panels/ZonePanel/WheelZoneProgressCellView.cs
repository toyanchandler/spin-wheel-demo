using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneProgressCellView : MonoBehaviour
    {
        [SerializeField] private Image _frameImage;
        [SerializeField] private TextMeshProUGUI _valueText;

        private string _lastZoneText;

        public void Apply(WheelZoneProgressCellPresentation presentation)
        {
            gameObject.SetActive(presentation.IsActive);
            if (!presentation.IsActive) return;

            SetTextIfChanged(presentation.ZoneText);
            _frameImage.gameObject.SetActive(presentation.ShowFrame);
            if (presentation.ShowFrame)
            {
                _frameImage.color = presentation.FrameColor;
            }
        }

        public bool IsWired()
        {
            return _frameImage != null
                && _valueText != null;
        }

        private void SetTextIfChanged(string zoneText)
        {
            if (zoneText == _lastZoneText) return;
            _valueText.SetText(zoneText);
            _lastZoneText = zoneText;
        }
    }
}
