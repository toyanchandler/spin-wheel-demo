using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelOutcomePopupHandleSet
    {
        public WheelOutcomePopupRootHandle Root { get; }
        public WheelOutcomePopupRectHandle ContentRoot { get; }
        public WheelOutcomePopupImageHandle Icon { get; }
        public WheelOutcomePopupTextHandle ResultText { get; }
        public WheelOutcomePopupChromeHandle Chrome { get; }
        public WheelOutcomePopupObjectHandle RewardPopupBackground { get; }
        public WheelOutcomePopupObjectHandle BombCardShadow { get; }
        public WheelOutcomePopupObjectHandle OutcomeRetryButton { get; }
        public WheelOutcomePopupImageHandle Flash { get; }
        public WheelOutcomePopupImageHandle Shine { get; }
        public WheelOutcomePopupImageHandle FlightIcon { get; }
        public WheelOutcomePopupComponentHandle<Camera> RewardBurstCamera { get; }
        public WheelOutcomePopupComponentHandle<RawImage> RewardBurstDisplay { get; }
        public WheelOutcomePopupBurstParticleHandle RewardBurstParticle { get; }

        private WheelOutcomePopupHandleSet(
            WheelOutcomePopupRootHandle root,
            WheelOutcomePopupRectHandle contentRoot,
            WheelOutcomePopupImageHandle icon,
            WheelOutcomePopupTextHandle resultText,
            WheelOutcomePopupChromeHandle chrome,
            WheelOutcomePopupObjectHandle rewardPopupBackground,
            WheelOutcomePopupObjectHandle bombCardShadow,
            WheelOutcomePopupObjectHandle outcomeRetryButton,
            WheelOutcomePopupImageHandle flash,
            WheelOutcomePopupImageHandle shine,
            WheelOutcomePopupImageHandle flightIcon,
            WheelOutcomePopupComponentHandle<Camera> rewardBurstCamera,
            WheelOutcomePopupComponentHandle<RawImage> rewardBurstDisplay,
            WheelOutcomePopupBurstParticleHandle rewardBurstParticle)
        {
            Root = root;
            ContentRoot = contentRoot;
            Icon = icon;
            ResultText = resultText;
            Chrome = chrome;
            RewardPopupBackground = rewardPopupBackground;
            BombCardShadow = bombCardShadow;
            OutcomeRetryButton = outcomeRetryButton;
            Flash = flash;
            Shine = shine;
            FlightIcon = flightIcon;
            RewardBurstCamera = rewardBurstCamera;
            RewardBurstDisplay = rewardBurstDisplay;
            RewardBurstParticle = rewardBurstParticle;
        }

        public static WheelOutcomePopupHandleSet FromWiring(Component owner, WheelOutcomePopupSceneWiring wiring)
        {
            ValidateWiring(owner, wiring);
            return new WheelOutcomePopupHandleSet(
                new WheelOutcomePopupRootHandle(wiring.RootCanvas, wiring.RootOverlay),
                new WheelOutcomePopupRectHandle(wiring.ContentRoot),
                new WheelOutcomePopupImageHandle(wiring.Icon),
                new WheelOutcomePopupTextHandle(wiring.ResultText),
                new WheelOutcomePopupChromeHandle(wiring.Chrome),
                new WheelOutcomePopupObjectHandle(wiring.RewardPopupBackground),
                new WheelOutcomePopupObjectHandle(wiring.BombCardShadow),
                new WheelOutcomePopupObjectHandle(wiring.OutcomeRetryButton),
                new WheelOutcomePopupImageHandle(wiring.Flash),
                new WheelOutcomePopupImageHandle(wiring.Shine),
                new WheelOutcomePopupImageHandle(wiring.FlightIcon),
                new WheelOutcomePopupComponentHandle<Camera>(wiring.RewardBurstCamera),
                new WheelOutcomePopupComponentHandle<RawImage>(wiring.RewardBurstDisplay),
                new WheelOutcomePopupBurstParticleHandle(wiring.RewardBurstParticle));
        }

        public void CaptureHome()
        {
            ContentRoot.CaptureHome();
            Icon.CaptureHome();
            FlightIcon.CaptureHome();
        }

        private static void ValidateWiring(Component owner, WheelOutcomePopupSceneWiring wiring)
        {
            const string panel = "outcome popup";
            WheelWiringValidation.Require(owner, panel, wiring.RootCanvas, nameof(wiring.RootCanvas));
            WheelWiringValidation.Require(owner, panel, wiring.ContentRoot, nameof(wiring.ContentRoot));
            WheelWiringValidation.Require(owner, panel, wiring.Icon, nameof(wiring.Icon));
            WheelWiringValidation.Require(owner, panel, wiring.ResultText, nameof(wiring.ResultText));
            WheelWiringValidation.Require(owner, panel, wiring.Chrome, nameof(wiring.Chrome));
            WheelWiringValidation.Require(owner, panel, wiring.RewardPopupBackground, nameof(wiring.RewardPopupBackground));
            WheelWiringValidation.Require(owner, panel, wiring.BombCardShadow, nameof(wiring.BombCardShadow));
            WheelWiringValidation.Require(owner, panel, wiring.OutcomeRetryButton, nameof(wiring.OutcomeRetryButton));
            WheelWiringValidation.Require(owner, panel, wiring.Flash, nameof(wiring.Flash));
            WheelWiringValidation.Require(owner, panel, wiring.Shine, nameof(wiring.Shine));
            WheelWiringValidation.Require(owner, panel, wiring.FlightIcon, nameof(wiring.FlightIcon));
            WheelWiringValidation.Require(owner, panel, wiring.RewardBurstCamera, nameof(wiring.RewardBurstCamera));
            WheelWiringValidation.Require(owner, panel, wiring.RewardBurstDisplay, nameof(wiring.RewardBurstDisplay));
            WheelWiringValidation.Require(owner, panel, wiring.RewardBurstParticle, nameof(wiring.RewardBurstParticle));
        }
    }

    internal sealed class WheelOutcomePopupRootHandle
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly Image _overlay;

        public WheelOutcomePopupRootHandle(CanvasGroup canvasGroup, Image overlay)
        {
            _canvasGroup = canvasGroup;
            _overlay = overlay;
        }

        public GameObject Root => _canvasGroup.gameObject;
        public bool IsVisible => Root.activeSelf;

        public void Show() => Root.SetActive(true);
        public void Hide() => Root.SetActive(false);
        public void SetAlpha(float alpha) => _canvasGroup.alpha = alpha;
        public Tween FadeTo(float alpha, float duration) => _canvasGroup.DOFade(alpha, duration);

        public void SetOverlayAlpha(float alpha)
        {
            if (_overlay == null) return;
            Color color = _overlay.color;
            color.a = alpha;
            _overlay.color = color;
        }
    }

    internal sealed class WheelOutcomePopupRectHandle
    {
        private readonly RectTransform _rect;
        private Vector2 _homeAnchorMin;
        private Vector2 _homeAnchorMax;
        private Vector2 _homePivot;
        private Vector2 _homeAnchoredPosition;
        private Vector3 _homeScale;

        public WheelOutcomePopupRectHandle(RectTransform rect) => _rect = rect;

        public RectTransform RectTransform => _rect;
        public Vector3 HomeScale => _homeScale;

        public void CaptureHome()
        {
            _homeAnchorMin = _rect.anchorMin;
            _homeAnchorMax = _rect.anchorMax;
            _homePivot = _rect.pivot;
            _homeAnchoredPosition = _rect.anchoredPosition;
            _homeScale = _rect.localScale;
        }

        public void RestoreHome()
        {
            _rect.anchorMin = _homeAnchorMin;
            _rect.anchorMax = _homeAnchorMax;
            _rect.pivot = _homePivot;
            _rect.anchoredPosition = _homeAnchoredPosition;
            _rect.localScale = _homeScale;
        }

        public void SetScale(Vector3 scale) => _rect.localScale = scale;
        public Tween TweenHomeScale(float duration, Ease ease) => _rect.DOScale(_homeScale, duration).SetEase(ease);
    }

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

    internal sealed class WheelOutcomePopupTextHandle
    {
        private readonly TextMeshProUGUI _text;

        public WheelOutcomePopupTextHandle(TextMeshProUGUI text) => _text = text;

        public void SetAlpha(float alpha) => WheelUiGraphicUtility.SetTextAlpha(_text, alpha);
        public void Apply(string value, Color color)
        {
            _text.text = value ?? string.Empty;
            _text.color = color;
        }

        public Tween FadeTo(float alpha, float duration) => _text.DOFade(alpha, duration);
    }

    internal sealed class WheelOutcomePopupChromeHandle
    {
        private readonly CanvasGroup _canvasGroup;

        public WheelOutcomePopupChromeHandle(CanvasGroup canvasGroup) => _canvasGroup = canvasGroup;

        public void Hide() => _canvasGroup.alpha = 0f;
        public Tween FadeIn() => _canvasGroup.DOFade(1f, WheelOutcomePopupAnimationConfig.ChromeFadeInDuration).SetUpdate(true);
    }

    internal sealed class WheelOutcomePopupObjectHandle
    {
        private readonly GameObject _target;

        public WheelOutcomePopupObjectHandle(GameObject target) => _target = target;
        public void SetActive(bool active) => _target.SetActive(active);
    }

    internal sealed class WheelOutcomePopupComponentHandle<TComponent> where TComponent : Component
    {
        public TComponent Component { get; }

        public WheelOutcomePopupComponentHandle(TComponent component) => Component = component;
    }

    internal sealed class WheelOutcomePopupBurstParticleHandle
    {
        private readonly ParticleSystem _particle;

        public WheelOutcomePopupBurstParticleHandle(ParticleSystem particle) => _particle = particle;

        public void Clear()
        {
            DOTween.Kill(_particle);
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        public void Play(float emissionDuration)
        {
            DOTween.Kill(_particle);
            _particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            _particle.Play(true);
            DOVirtual.DelayedCall(
                    emissionDuration,
                    () => _particle.Stop(true, ParticleSystemStopBehavior.StopEmitting),
                    false)
                .SetUpdate(true)
                .SetTarget(_particle);
        }
    }
}
