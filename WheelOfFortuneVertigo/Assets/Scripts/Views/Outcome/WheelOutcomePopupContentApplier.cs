using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal static class WheelOutcomePopupContentApplier
    {
        public static void Apply(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            Sprite icon = WheelOutcomePopupAnimator.ResolvePresentationIcon(snapshot);
            ApplyOverlayTone(binding, snapshot);
            ApplyStateCopy(binding, snapshot);
            binding.IconImage.sprite = icon;
            binding.IconImage.enabled = icon != null;
            binding.IconImage.color = WheelOutcomePopupAnimator.ResolvePresentationIconColor(snapshot);
            binding.IconImage.preserveAspect = true;
        }

        private static void ApplyOverlayTone(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            if (binding.Root == null)
            {
                return;
            }

            Image overlay = binding.Root.GetComponent<Image>();
            if (overlay == null)
            {
                return;
            }

            Color color = overlay.color;
            color.a = snapshot.Phase == WheelGamePhase.Bombed ? 0.56f : 0.02f;
            overlay.color = color;
        }

        private static void ApplyStateCopy(WheelOutcomePopupRefs binding, WheelOutcomeSnapshot snapshot)
        {
            binding.TitleText.text = snapshot.Title;
            binding.ResultText.text = snapshot.ResultText;
            binding.ResultText.color = snapshot.ResultColor;
            SetSummaryActive(binding, !string.IsNullOrEmpty(snapshot.SummaryText));
            if (binding.SummaryText != null)
            {
                binding.SummaryText.text = snapshot.SummaryText ?? string.Empty;
                binding.SummaryText.color = snapshot.ResultColor;
            }
        }

        private static void SetSummaryActive(WheelOutcomePopupRefs binding, bool isActive)
        {
            if (binding.SummaryText != null)
            {
                binding.SummaryText.gameObject.SetActive(isActive);
            }
        }
    }
}
