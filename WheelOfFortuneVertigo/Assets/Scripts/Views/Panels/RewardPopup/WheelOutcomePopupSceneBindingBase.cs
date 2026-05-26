using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public abstract class WheelOutcomePopupSceneComponentBinding<TComponent> : MonoBehaviour
        where TComponent : Component
    {
        private TComponent _component;

        protected TComponent RequiredComponent
        {
            get
            {
                if (_component == null && !TryGetComponent(out _component))
                {
                    throw new InvalidOperationException(name + " requires " + typeof(TComponent).Name + ".");
                }

                return _component;
            }
        }

        protected TRequired RequireSiblingComponent<TRequired>()
            where TRequired : Component
        {
            TRequired required;
            if (!TryGetComponent(out required))
            {
                throw new InvalidOperationException(name + " requires " + typeof(TRequired).Name + ".");
            }

            return required;
        }
    }

    public abstract class WheelOutcomePopupRectBinding : WheelOutcomePopupSceneComponentBinding<RectTransform>
    {
        private Vector2 _homeAnchorMin;
        private Vector2 _homeAnchorMax;
        private Vector2 _homePivot;
        private Vector2 _homeAnchoredPosition;
        private Vector3 _homeScale;

        public RectTransform RectTransform { get { return RequiredComponent; } }
        public Vector2 HomeAnchoredPosition { get { return _homeAnchoredPosition; } }
        public Vector3 HomeScale { get { return _homeScale; } }

        public void CaptureHome()
        {
            _homeAnchorMin = RectTransform.anchorMin;
            _homeAnchorMax = RectTransform.anchorMax;
            _homePivot = RectTransform.pivot;
            _homeAnchoredPosition = RectTransform.anchoredPosition;
            _homeScale = RectTransform.localScale;
        }

        public void RestoreHome()
        {
            RectTransform.anchorMin = _homeAnchorMin;
            RectTransform.anchorMax = _homeAnchorMax;
            RectTransform.pivot = _homePivot;
            RectTransform.anchoredPosition = _homeAnchoredPosition;
            RectTransform.localScale = _homeScale;
        }

        public void SetScale(Vector3 scale)
        {
            RectTransform.localScale = scale;
        }

        public Tween TweenHomeScale(float duration, Ease ease)
        {
            return RectTransform.DOScale(_homeScale, duration).SetEase(ease);
        }
    }

    public abstract class WheelOutcomePopupImageBinding : WheelOutcomePopupSceneComponentBinding<Image>
    {
        private Vector2 _homeAnchoredPosition;

        public Image Image { get { return RequiredComponent; } }
        public RectTransform RectTransform { get { return RequiredComponent.rectTransform; } }
        public Vector2 HomeAnchoredPosition { get { return _homeAnchoredPosition; } }

        public void CaptureHome()
        {
            _homeAnchoredPosition = RectTransform.anchoredPosition;
        }

        public void KillTweens()
        {
            WheelUiTweenUtility.KillImageTweens(Image);
        }

        public void SetGameObjectActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void Clear()
        {
            KillTweens();
            Image.sprite = null;
            SetAlpha(0f);
            ResetTransform(Vector3.one);
        }

        public void ApplySprite(Sprite sprite, Color tint, float alpha)
        {
            Image.sprite = sprite;
            Image.enabled = sprite != null;
            Image.color = WheelUiGraphicUtility.WithAlpha(tint, alpha);
        }

        public void SetAlpha(float alpha)
        {
            WheelUiGraphicUtility.SetGraphicAlpha(Image, alpha);
        }

        public Tween FadeTo(float alpha, float duration)
        {
            return Image.DOFade(alpha, duration);
        }

        public Tween TweenHomePosition(float duration, Ease ease)
        {
            return RectTransform.DOAnchorPos(_homeAnchoredPosition, duration).SetEase(ease);
        }

        public Tween TweenScale(Vector3 scale, float duration, Ease ease)
        {
            return RectTransform.DOScale(scale, duration).SetEase(ease);
        }

        public Tween TweenWorldPosition(Vector3 position, float duration, Ease ease)
        {
            return RectTransform.DOMove(position, duration).SetEase(ease);
        }

        public Tween TweenLocalRotate(Vector3 rotation, float duration, Ease ease)
        {
            return RectTransform.DOLocalRotate(rotation, duration, RotateMode.Fast).SetEase(ease);
        }

        public void ResetTransform(Vector3 scale)
        {
            RectTransform.anchoredPosition = _homeAnchoredPosition;
            RectTransform.localScale = scale;
            RectTransform.localRotation = Quaternion.identity;
        }
    }

    public abstract class WheelOutcomePopupTextBinding : WheelOutcomePopupSceneComponentBinding<TextMeshProUGUI>
    {
        public TextMeshProUGUI Text { get { return RequiredComponent; } }

        public bool IsActiveInHierarchy { get { return gameObject.activeInHierarchy; } }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void SetAlpha(float alpha)
        {
            WheelUiGraphicUtility.SetTextAlpha(Text, alpha);
        }

        public void Apply(string value, Color color)
        {
            Text.text = value ?? string.Empty;
            Text.color = color;
        }

        public Tween FadeTo(float alpha, float duration)
        {
            return Text.DOFade(alpha, duration);
        }
    }

    public abstract class WheelOutcomePopupObjectBinding : MonoBehaviour
    {
        public GameObject Target { get { return gameObject; } }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }
    }
}
