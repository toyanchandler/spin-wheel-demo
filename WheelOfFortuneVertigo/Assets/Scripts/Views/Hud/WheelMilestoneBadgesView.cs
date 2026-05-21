using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelMilestoneBadgesView : MonoBehaviour
    {
        [SerializeField] private Image _superBadgeImage;
        [SerializeField] private Image _safeBadgeImage;
        [SerializeField] private TextMeshProUGUI _superBadgeText;
        [SerializeField] private TextMeshProUGUI _safeBadgeText;

        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _superBadgeText.SetText(snapshot.SuperMilestoneBadgeText);
            _safeBadgeText.SetText(snapshot.SafeMilestoneBadgeText);
            _superBadgeImage.color = snapshot.SuperMilestoneBadgeColor;
            _safeBadgeImage.color = snapshot.SafeMilestoneBadgeColor;
        }
    }
}
