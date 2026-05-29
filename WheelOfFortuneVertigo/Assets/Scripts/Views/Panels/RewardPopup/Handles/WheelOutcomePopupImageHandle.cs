using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupImageHandle
    {
        private readonly Image _image;
        private Vector2 _homeAnchoredPosition;

        public WheelOutcomePopupImageHandle(Image image) => _image = image;

        public Image Image => _image;
        public RectTransform RectTransform => _image.rectTransform;

        public void CaptureHome() => _homeAnchoredPosition = RectTransform.anchoredPosition;

        public void KillTweens() => WheelUiTweenUtility.KillImageTweens(_image);
        public void SetGameObjectActive(bool active) => _image.gameObject.SetActive(active);

        public void Clear()
        {
            KillTweens();
            _image.sprite = null;
            SetAlpha(0f);
            ResetTransform(Vector3.one);
        }

        public void ApplySprite(Sprite sprite, Color tint, float alpha)
        {
            _image.sprite = sprite;
            _image.enabled = sprite != null;
            _image.color = WheelUiGraphicUtility.WithAlpha(tint, alpha);
        }

        public void SetAlpha(float alpha) => WheelUiGraphicUtility.SetGraphicAlpha(_image, alpha);
        public Tween FadeTo(float alpha, float duration) => _image.DOFade(alpha, duration);
        public Tween TweenScale(Vector3 scale, float duration, Ease ease) => RectTransform.DOScale(scale, duration).SetEase(ease);
        public Tween TweenWorldPosition(Vector3 position, float duration, Ease ease) => RectTransform.DOMove(position, duration).SetEase(ease);
        public Tween TweenLocalRotate(Vector3 rotation, float duration, Ease ease) => RectTransform.DOLocalRotate(rotation, duration, RotateMode.Fast).SetEase(ease);

        public void ResetTransform(Vector3 scale)
        {
            RectTransform.anchoredPosition = _homeAnchoredPosition;
            RectTransform.localScale = scale;
            RectTransform.localRotation = Quaternion.identity;
        }
    }
}
