using TMPro;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelRewardAmountLabel
    {
        public static void Apply(TextMeshProUGUI amountText, int amount, Color visibleColor)
        {
            if (amount <= 1)
            {
                amountText.text = string.Empty;
                amountText.enabled = false;
                return;
            }

            amountText.SetText("x{0}", amount);
            amountText.color = visibleColor;
            amountText.enabled = true;
        }
    }
}
