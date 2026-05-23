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
        [SerializeField] private Image _superBadgeGlowImage;
        [SerializeField] private Image _safeBadgeGlowImage;
        [SerializeField] private TextMeshProUGUI _superBadgeText;
        [SerializeField] private TextMeshProUGUI _safeBadgeText;
        [SerializeField] private TextMeshProUGUI _superBadgeLabelText;
        [SerializeField] private TextMeshProUGUI _safeBadgeLabelText;
        [SerializeField] private TextMeshProUGUI _superBadgeNumberText;
        [SerializeField] private TextMeshProUGUI _safeBadgeNumberText;

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
            ApplyBadge(
                _superBadgeImage,
                _superBadgeGlowImage,
                _superBadgeText,
                _superBadgeLabelText,
                _superBadgeNumberText,
                "SUPER\nZONE",
                ExtractMilestoneNumber(snapshot.SuperMilestoneBadgeText),
                snapshot.SuperMilestoneBadgeColor,
                IsIntervalZone(snapshot.Zone, snapshot.SuperZoneInterval));
            ApplyBadge(
                _safeBadgeImage,
                _safeBadgeGlowImage,
                _safeBadgeText,
                _safeBadgeLabelText,
                _safeBadgeNumberText,
                "SAFE\nZONE",
                ExtractMilestoneNumber(snapshot.SafeMilestoneBadgeText),
                snapshot.SafeMilestoneBadgeColor,
                IsIntervalZone(snapshot.Zone, snapshot.SafeZoneInterval));
        }

        private void ApplyBadge(
            Image badgeImage,
            Image glowImage,
            TextMeshProUGUI fallbackText,
            TextMeshProUGUI labelText,
            TextMeshProUGUI numberText,
            string label,
            string number,
            Color color,
            bool isActive)
        {
            Color badgeColor = color;
            badgeColor.a = Mathf.Max(color.a, isActive ? 0.96f : 0.84f);
            badgeImage.color = badgeColor;

            if (glowImage != null)
            {
                glowImage.color = new Color(1f, 1f, 1f, isActive ? 0.36f : 0.18f);
            }

            if (labelText == null || numberText == null)
            {
                fallbackText.SetText(label + "\n" + number);
                StyleText(fallbackText, 30f, 4f, isActive);
                return;
            }

            fallbackText.gameObject.SetActive(false);
            labelText.SetText(label);
            numberText.SetText(number);
            StyleText(labelText, 32f, 12f, isActive);
            StyleText(numberText, 74f, 0f, isActive);
        }

        private void StyleText(TextMeshProUGUI text, float size, float spacing, bool isActive)
        {
            text.enableAutoSizing = true;
            text.fontSizeMax = isActive ? size + 4f : size;
            text.fontSizeMin = Mathf.Min(16f, size * 0.45f);
            text.fontStyle = FontStyles.Normal;
            text.characterSpacing = spacing;
            text.outlineWidth = isActive ? 0.1f : 0.07f;
            text.outlineColor = new Color(0f, 0f, 0f, 0.82f);
        }

        private static string ExtractMilestoneNumber(string value)
        {
            string[] tokens = value.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length == 0 ? string.Empty : tokens[tokens.Length - 1];
        }

        private static bool IsIntervalZone(int zone, int interval)
        {
            return interval > 0 && zone % interval == 0;
        }
    }
}
