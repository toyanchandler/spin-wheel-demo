using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelRewardCardView : MonoBehaviour
    {
        [SerializeField] private Image _frameImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _amountText;

        public void ApplyFrames(Sprite cardFrameSprite)
        {
            _frameImage.sprite = cardFrameSprite;
            _frameImage.color = Color.white;
            _frameImage.maskable = true;
        }

        public void Apply(RewardInventoryEntry entry)
        {
            _iconImage.sprite = entry.Icon;
            _iconImage.color = Color.white;
            _iconImage.preserveAspect = true;
            _iconImage.maskable = true;
            _amountText.maskable = true;
            AmountTable.Apply(_amountText, entry.Amount, entry.AccentColor);
            gameObject.SetActive(true);
            transform.DOKill();
            transform.localScale = new Vector3(0.88f, 0.88f, 1f);
            transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        public void PlayLandingPulse(float delay)
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            DOTween.Sequence()
                .SetTarget(this)
                .SetUpdate(true)
                .AppendInterval(delay)
                .Append(transform.DOScale(new Vector3(1.14f, 1.14f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(Vector3.one, 0.18f).SetEase(Ease.OutBack));
        }

        public void Hide()
        {
            transform.DOKill();
            DOTween.Kill(this);
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            transform.DOKill();
            DOTween.Kill(this);
        }

        private static class AmountTable
        {
            private static readonly System.Action<TextMeshProUGUI, int, Color>[] ApplyActions =
            {
                HideAmount,
                ShowAmount
            };

            public static void Apply(TextMeshProUGUI amountText, int amount, Color accentColor)
            {
                ApplyActions[System.Convert.ToInt32(amount > 1)](amountText, amount, accentColor);
            }

            private static void HideAmount(TextMeshProUGUI amountText, int amount, Color accentColor)
            {
                amountText.text = string.Empty;
                amountText.enabled = false;
            }

            private static void ShowAmount(TextMeshProUGUI amountText, int amount, Color accentColor)
            {
                amountText.SetText("{0}", amount);
                amountText.color = accentColor;
                amountText.enabled = true;
            }
        }
    }
}
