using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelSliceView : MonoBehaviour
    {
        [SerializeField] private RectTransform _root;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _amount;

        public void Apply(WheelSlicePresentation presentation, Vector2 maxIconSize)
        {
            _root.anchoredPosition = presentation.AnchoredPosition;
            _root.sizeDelta = IconFitter.FitWithinBounds(maxIconSize, presentation.Icon);
            _icon.sprite = presentation.Icon;
            _icon.color = Color.white;
            AmountTable.Apply(_amount, presentation);
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        private static class IconFitter
        {
            public static Vector2 FitWithinBounds(Vector2 maxSize, Sprite sprite)
            {
                if (sprite == null)
                {
                    return maxSize;
                }

                float spriteWidth = sprite.rect.width;
                float spriteHeight = sprite.rect.height;
                if (spriteWidth <= 0f || spriteHeight <= 0f)
                {
                    return maxSize;
                }

                float spriteAspect = spriteWidth / spriteHeight;
                float boundsAspect = maxSize.x / maxSize.y;
                if (spriteAspect > boundsAspect)
                {
                    return new Vector2(maxSize.x, maxSize.x / spriteAspect);
                }

                return new Vector2(maxSize.y * spriteAspect, maxSize.y);
            }
        }

        private static class AmountTable
        {
            private static readonly System.Action<TextMeshProUGUI, WheelSlicePresentation>[] ApplyActions =
            {
                HideAmount,
                ShowAmount
            };

            public static void Apply(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
            {
                ApplyActions[System.Convert.ToInt32(presentation.ShowAmountLabel)](amountLabel, presentation);
            }

            private static void HideAmount(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
            {
                amountLabel.text = string.Empty;
                amountLabel.enabled = false;
            }

            private static void ShowAmount(TextMeshProUGUI amountLabel, WheelSlicePresentation presentation)
            {
                amountLabel.SetText("{0}", presentation.Amount);
                amountLabel.color = presentation.AmountColor;
                amountLabel.enabled = true;
            }
        }
    }
}
