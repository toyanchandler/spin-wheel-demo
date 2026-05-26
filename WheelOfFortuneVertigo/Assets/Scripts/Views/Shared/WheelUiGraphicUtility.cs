using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public static class WheelUiGraphicUtility
    {
        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static void SetActive(GameObject target, bool active)
        {
            target?.SetActive(active);
        }

        public static void SetEnabled(Behaviour target, bool enabled)
        {
            if (target == null) return;
            target.enabled = enabled;
        }

        public static void SetLocalScale(RectTransform rect, Vector3 scale)
        {
            if (rect == null) return;
            rect.localScale = scale;
        }

        public static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            graphic.color = WithAlpha(graphic.color, alpha);
        }

        public static void SetTextAlpha(TextMeshProUGUI text, float alpha)
        {
            if (text == null)
            {
                return;
            }

            text.color = WithAlpha(text.color, alpha);
        }
    }
}
