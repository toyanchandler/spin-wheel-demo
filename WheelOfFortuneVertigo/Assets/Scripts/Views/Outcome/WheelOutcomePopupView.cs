using DG.Tweening;
#if UNITY_EDITOR
using System.Collections;
using System.IO;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private CanvasGroup _rewardChromeGroup;
        [SerializeField] private Image _flashImage;
        [SerializeField] private Image _shineImage;
        [SerializeField] private Image[] _flightIconPool;
        [SerializeField] private Camera _rewardBurstCamera;
        [SerializeField] private RawImage _rewardBurstDisplay;
        [SerializeField] private ParticleSystem _rewardBurstParticle;
        [SerializeField] private ParticleSystemRenderer _rewardBurstRenderer;

        private WheelEventBus _eventBus;
        private WheelRewardPanelView _rewardPanelView;
        private Presenter _presenter;
        private Vector2 _iconHomeAnchoredPosition;
#if UNITY_EDITOR
        private Coroutine _rewardBurstCaptureRoutine;
        private static int _rewardBurstCaptureIndex;
#endif

        private void Awake()
        {
            if (_iconImage != null)
            {
                _iconHomeAnchoredPosition = _iconImage.rectTransform.anchoredPosition;
            }
        }

        public void Bind(WheelEventBus eventBus)
        {
            Bind(eventBus, null);
        }

        public void Bind(WheelEventBus eventBus, WheelRewardPanelView rewardPanelView)
        {
            _eventBus = eventBus;
            _rewardPanelView = rewardPanelView;
            _presenter = new Presenter(CreateBinding(), this);
            _presenter.Reset();
            _eventBus.OutcomeResolved += OnOutcomeResolved;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.OutcomeResolved -= OnOutcomeResolved;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
            _presenter.Reset();
            _presenter = null;
            _rewardPanelView = null;
        }

        private void OnOutcomeResolved(WheelOutcomeSnapshot snapshot)
        {
            _presenter.HandleOutcome(snapshot);
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _presenter.HandleHud(snapshot.IsOutcomePopupAllowed);
        }

        private Binding CreateBinding()
        {
            return new Binding(
                _root,
                _iconImage,
                _titleText,
                _resultText,
                _summaryText,
                _canvasGroup,
                _contentRoot,
                _rewardChromeGroup,
                _flashImage,
                _shineImage,
                _flightIconPool,
                _rewardBurstCamera,
                _rewardBurstDisplay,
                _rewardBurstParticle,
                _rewardBurstRenderer,
                ResolveRewardPanel(),
                _iconHomeAnchoredPosition,
                this);
        }

        private WheelRewardPanelView ResolveRewardPanel()
        {
            return _rewardPanelView;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static Color ResolveVisibleIconColor(Color color)
        {
            color.a = 1f;
            if (color.maxColorComponent <= 0.02f)
            {
                return Color.white;
            }

            return color;
        }

#if UNITY_EDITOR
        private void CaptureRewardBurstDebug(Binding binding, WheelOutcomeSnapshot snapshot)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (_rewardBurstCaptureRoutine != null)
            {
                StopCoroutine(_rewardBurstCaptureRoutine);
            }

            _rewardBurstCaptureRoutine = StartCoroutine(CaptureRewardBurstDebugRoutine(binding, snapshot));
        }

        private IEnumerator CaptureRewardBurstDebugRoutine(Binding binding, WheelOutcomeSnapshot snapshot)
        {
            yield return new WaitForSecondsRealtime(0.14f);
            yield return new WaitForEndOfFrame();

            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/UIParticleBurstCaptures"));
            Directory.CreateDirectory(folder);
            int captureId = ++_rewardBurstCaptureIndex;
            string captureLabel = captureId < 10 ? string.Concat("0", captureId) : string.Concat(captureId);
            string prefix = Path.Combine(folder, string.Concat("burst_", captureLabel, "_", snapshot.RewardId));
            string screenPath = string.Concat(prefix, "_game.png");
            string renderTexturePath = string.Concat(prefix, "_render_texture.png");
            string revealPath = string.Concat(prefix, "_reveal_hold_game.png");

            UnityEngine.ScreenCapture.CaptureScreenshot(screenPath);
            CaptureRenderTexture(binding.RewardBurstCamera, renderTexturePath);

            RectTransform displayRect = binding.RewardBurstDisplay != null ? binding.RewardBurstDisplay.rectTransform : null;
            RectTransform iconRect = binding.IconImage != null ? binding.IconImage.rectTransform : null;
            Vector2 displayPosition = displayRect != null ? displayRect.anchoredPosition : Vector2.zero;
            Vector2 displaySize = displayRect != null ? displayRect.sizeDelta : Vector2.zero;
            Vector2 iconPosition = iconRect != null ? iconRect.anchoredPosition : Vector2.zero;
            Debug.Log(
                string.Concat(
                    "UIParticle burst capture saved: ",
                    screenPath,
                    " | ",
                    renderTexturePath,
                    " | icon=",
                    iconPosition,
                    " display=",
                    displayPosition,
                    " size=",
                    displaySize));

            yield return new WaitForSecondsRealtime(0.96f);
            yield return new WaitForEndOfFrame();
            UnityEngine.ScreenCapture.CaptureScreenshot(revealPath);
            _rewardBurstCaptureRoutine = null;
        }

        private static void CaptureRenderTexture(Camera camera, string path)
        {
            if (camera == null || camera.targetTexture == null)
            {
                return;
            }

            RenderTexture renderTexture = camera.targetTexture;
            RenderTexture active = RenderTexture.active;
            camera.Render();
            RenderTexture.active = renderTexture;
            var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply(false);
            File.WriteAllBytes(path, UnityEngine.ImageConversion.EncodeToPNG(texture));
            RenderTexture.active = active;
        }
#endif

        private readonly struct Binding
        {
            public readonly GameObject Root;
            public readonly Image IconImage;
            public readonly TextMeshProUGUI TitleText;
            public readonly TextMeshProUGUI ResultText;
            public readonly TextMeshProUGUI SummaryText;
            public readonly CanvasGroup CanvasGroup;
            public readonly RectTransform ContentRoot;
            public readonly CanvasGroup RewardChromeGroup;
            public readonly Image FlashImage;
            public readonly Image ShineImage;
            public readonly Image[] FlightIconPool;
            public readonly Camera RewardBurstCamera;
            public readonly RawImage RewardBurstDisplay;
            public readonly ParticleSystem RewardBurstParticle;
            public readonly ParticleSystemRenderer RewardBurstRenderer;
            public readonly WheelRewardPanelView RewardPanelView;
            public readonly Vector2 IconHomeAnchoredPosition;
            public readonly WheelOutcomePopupView Owner;

            public Binding(
                GameObject root,
                Image iconImage,
                TextMeshProUGUI titleText,
                TextMeshProUGUI resultText,
                TextMeshProUGUI summaryText,
                CanvasGroup canvasGroup,
                RectTransform contentRoot,
                CanvasGroup rewardChromeGroup,
                Image flashImage,
                Image shineImage,
                Image[] flightIconPool,
                Camera rewardBurstCamera,
                RawImage rewardBurstDisplay,
                ParticleSystem rewardBurstParticle,
                ParticleSystemRenderer rewardBurstRenderer,
                WheelRewardPanelView rewardPanelView,
                Vector2 iconHomeAnchoredPosition,
                WheelOutcomePopupView owner)
            {
                Root = root;
                IconImage = iconImage;
                TitleText = titleText;
                ResultText = resultText;
                SummaryText = summaryText;
                CanvasGroup = canvasGroup;
                ContentRoot = contentRoot;
                RewardChromeGroup = rewardChromeGroup;
                FlashImage = flashImage;
                ShineImage = shineImage;
                FlightIconPool = flightIconPool;
                RewardBurstCamera = rewardBurstCamera;
                RewardBurstDisplay = rewardBurstDisplay;
                RewardBurstParticle = rewardBurstParticle;
                RewardBurstRenderer = rewardBurstRenderer;
                RewardPanelView = rewardPanelView;
                IconHomeAnchoredPosition = iconHomeAnchoredPosition;
                Owner = owner;
            }
        }

        private sealed class Presenter
        {
            private struct RuntimeState
            {
                public bool HasOutcome;
                public bool PopupAllowed;
            }

            private readonly Binding _binding;
            private readonly Object _tweenTarget;
            private RuntimeState _state;
            private Sequence _sequence;
            private WheelOutcomePopupMotion _motion;
            private WheelOutcomeSnapshot _currentSnapshot;

            public WheelOutcomeSnapshot CurrentSnapshot { get { return _currentSnapshot; } }

            public Presenter(Binding binding, Object tweenTarget)
            {
                _binding = binding;
                _tweenTarget = tweenTarget;
                _motion = WheelOutcomePopupMotion.Default();
                _state.PopupAllowed = true;
            }

            public void Reset()
            {
                _state = default(RuntimeState);
                _state.PopupAllowed = true;
                Animator.Hide(_binding, ref _sequence);
            }

            public void HandleOutcome(WheelOutcomeSnapshot snapshot)
            {
                ContentApplier.Apply(_binding, snapshot);
                _motion = snapshot.Motion;
                _currentSnapshot = snapshot;
                _state.HasOutcome = snapshot.HasPresentableOutcome;
                SyncVisibility();
            }

            public void HandleHud(bool isOutcomePopupAllowed)
            {
                _state.PopupAllowed = isOutcomePopupAllowed;
                if (isOutcomePopupAllowed)
                {
                    SyncVisibility();
                }
            }

            public void MarkPresentationComplete()
            {
                _state.HasOutcome = false;
                Animator.ClearMainIcon(_binding);
            }

            private void SyncVisibility()
            {
                int show = System.Convert.ToInt32(_state.HasOutcome && _state.PopupAllowed);
                VisibilityTable.Apply(_binding, _motion, _tweenTarget, _currentSnapshot, show, _state.HasOutcome, ref _sequence);
            }
        }

        private static class ContentApplier
        {
            public static void Apply(Binding binding, WheelOutcomeSnapshot snapshot)
            {
                Sprite icon = Animator.ResolvePresentationIcon(snapshot);
                binding.TitleText.text = snapshot.Title;
                binding.ResultText.text = snapshot.ResultText;
                binding.ResultText.color = snapshot.ResultColor;
                binding.SummaryText.text = snapshot.SummaryText;
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = icon != null;
                binding.IconImage.color = Animator.ResolvePresentationIconColor(snapshot);
                binding.IconImage.preserveAspect = true;
            }
        }

        private static class Animator
        {
            public static void Hide(Binding binding, ref Sequence sequence)
            {
                Kill(ref sequence);
                RestoreSourceSliceRewardVisual();
                StopRewardBurst(binding);
                ClearMainIcon(binding);
                binding.Root.SetActive(false);
            }

            public static void ClearMainIcon(Binding binding)
            {
                if (binding.IconImage == null)
                {
                    return;
                }

                RectTransform iconRect = binding.IconImage.rectTransform;
                binding.IconImage.sprite = null;
                Color color = binding.IconImage.color;
                color.a = 0f;
                binding.IconImage.color = color;
                iconRect.anchoredPosition = binding.IconHomeAnchoredPosition;
                iconRect.localScale = Vector3.one;
                iconRect.localRotation = Quaternion.identity;
            }

            public static void Show(
                Binding binding,
                WheelOutcomePopupMotion motion,
                Object tweenTarget,
                WheelOutcomeSnapshot snapshot,
                bool hasOutcome,
                ref Sequence sequence)
            {
                if (binding.Root.activeSelf)
                {
                    return;
                }

                Kill(ref sequence);
                binding.Root.transform.SetAsLastSibling();
                binding.Root.SetActive(true);

                if (binding.CanvasGroup == null || binding.ContentRoot == null)
                {
                    return;
                }

                const float burstStageDuration = 0.36f;
                const float revealStageStart = burstStageDuration;
                const float burstVisibleDuration = 2.4f;
                const float rewardHoldDuration = 1.42f;
                const float flightStageStart = revealStageStart + rewardHoldDuration;

                binding.CanvasGroup.alpha = 0f;
                float startScale = motion.StartScale;
                binding.ContentRoot.localScale = new Vector3(startScale, startScale, 1f);
                RectTransform iconRect = binding.IconImage.rectTransform;
                Vector2 revealIconPosition = ResolveRewardRevealPosition(binding);
                ApplyRewardRevealLayout(binding, revealIconPosition);
                iconRect.anchoredPosition = ResolveIconStartPosition(binding, snapshot, iconRect);
                Sprite icon = ResolvePresentationIcon(snapshot);
                SuppressSourceSliceRewardVisual(snapshot);
                iconRect.localScale = new Vector3(0.38f, 0.38f, 1f);
                iconRect.localRotation = Quaternion.identity;
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = icon != null;
                binding.IconImage.color = WheelOutcomePopupView.WithAlpha(ResolvePresentationIconColor(snapshot), 0f);
                SetTextAlpha(binding.TitleText, 0f);
                SetTextAlpha(binding.ResultText, 0f);
                SetTextAlpha(binding.SummaryText, 0f);
                PrepareVfx(binding);
                PlayRewardBurst(binding, snapshot, revealIconPosition);

                sequence = DOTween.Sequence()
                    .SetTarget(tweenTarget)
                    .SetUpdate(true)
                    .Append(binding.CanvasGroup.DOFade(1f, motion.FadeDuration))
                    .Join(binding.ContentRoot.DOScale(Vector3.one, motion.ScaleDuration).SetEase(motion.ScaleEase))
                    .InsertCallback(revealStageStart, () => PrepareRewardReveal(binding, snapshot, icon, revealIconPosition))
                    .Insert(revealStageStart, PlayChromeReveal(binding))
                    .Insert(revealStageStart, binding.IconImage.DOFade(1f, 0.12f))
                    .Insert(revealStageStart, iconRect.DOScale(new Vector3(1.02f, 1.02f, 1f), 0.28f).SetEase(Ease.OutBack))
                    .Insert(revealStageStart + 0.28f, iconRect.DOScale(new Vector3(0.86f, 0.86f, 1f), 0.16f).SetEase(Ease.OutQuad))
                    .Insert(revealStageStart + 0.04f, PlayFlash(binding))
                    .Insert(revealStageStart + 0.10f, binding.TitleText.DOFade(1f, 0.16f))
                    .Insert(revealStageStart + 0.20f, binding.ResultText.DOFade(1f, 0.18f))
                    .Insert(revealStageStart + 0.52f, PlayShine(binding))
                    .InsertCallback(burstVisibleDuration, () => StopRewardBurst(binding))
                    .Insert(flightStageStart - 0.18f, binding.SummaryText.DOFade(1f, 0.18f))
                    .InsertCallback(flightStageStart, () => PlayRewardFlight(binding, tweenTarget, hasOutcome));
            }

            private static void Kill(ref Sequence sequence)
            {
                if (sequence != null)
                {
                    sequence.Kill();
                    sequence = null;
                }
            }

            private static void PrepareVfx(Binding binding)
            {
                HideFlightIconPool(binding);
                PrepareChrome(binding);

                if (binding.FlashImage != null)
                {
                    Color color = binding.FlashImage.color;
                    color.a = 0f;
                    binding.FlashImage.color = color;
                    binding.FlashImage.rectTransform.localScale = new Vector3(0.54f, 0.54f, 1f);
                    binding.FlashImage.rectTransform.localRotation = Quaternion.identity;
                }

                if (binding.ShineImage != null)
                {
                    Color color = binding.ShineImage.color;
                    color.a = 0f;
                    binding.ShineImage.color = color;
                    binding.ShineImage.rectTransform.localScale = Vector3.one;
                }
            }

            private static void PrepareChrome(Binding binding)
            {
                if (binding.RewardChromeGroup == null)
                {
                    return;
                }

                binding.RewardChromeGroup.alpha = 0f;
                binding.RewardChromeGroup.transform.localScale = new Vector3(0.94f, 0.94f, 1f);
            }

            private static Tween PlayChromeReveal(Binding binding)
            {
                if (binding.RewardChromeGroup == null)
                {
                    return DOVirtual.DelayedCall(0f, () => { }, false);
                }

                return DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(binding.RewardChromeGroup.DOFade(1f, 0.14f))
                    .Join(binding.RewardChromeGroup.transform.DOScale(new Vector3(1.02f, 1.02f, 1f), 0.22f).SetEase(Ease.OutCubic))
                    .Append(binding.RewardChromeGroup.transform.DOScale(Vector3.one, 0.14f).SetEase(Ease.OutQuad));
            }

            private static void PrepareRewardReveal(
                Binding binding,
                WheelOutcomeSnapshot snapshot,
                Sprite icon,
                Vector2 revealIconPosition)
            {
                RectTransform iconRect = binding.IconImage.rectTransform;
                iconRect.anchoredPosition = revealIconPosition;
                iconRect.localScale = new Vector3(0.48f, 0.48f, 1f);
                iconRect.localRotation = Quaternion.identity;
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = icon != null;
                binding.IconImage.color = WheelOutcomePopupView.WithAlpha(ResolvePresentationIconColor(snapshot), 0f);
                binding.IconImage.preserveAspect = true;
                MoveRewardVfx(binding, revealIconPosition);
            }

            private static Vector2 ResolveRewardRevealPosition(Binding binding)
            {
                return binding.IconHomeAnchoredPosition;
            }

            private static void ApplyRewardRevealLayout(Binding binding, Vector2 revealIconPosition)
            {
                SetAnchoredY(binding.TitleText.rectTransform, revealIconPosition.y + 136f);
                SetAnchoredY(binding.ResultText.rectTransform, revealIconPosition.y - 180f);
                SetAnchoredY(binding.SummaryText.rectTransform, revealIconPosition.y - 226f);
            }

            private static void SetAnchoredY(RectTransform rect, float y)
            {
                Vector2 position = rect.anchoredPosition;
                position.y = y;
                rect.anchoredPosition = position;
            }

            private static void MoveRewardVfx(Binding binding, Vector2 revealIconPosition)
            {
                if (binding.FlashImage != null)
                {
                    binding.FlashImage.rectTransform.anchoredPosition = revealIconPosition;
                }

                if (binding.ShineImage != null)
                {
                    binding.ShineImage.rectTransform.anchoredPosition = revealIconPosition;
                }
            }

            private static Tween PlayFlash(Binding binding)
            {
                if (binding.FlashImage == null)
                {
                    return DOVirtual.DelayedCall(0f, () => { }, false);
                }

                RectTransform rect = binding.FlashImage.rectTransform;
                return DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(binding.FlashImage.DOFade(0.9f, 0.10f))
                    .Join(rect.DOScale(new Vector3(1.36f, 1.36f, 1f), 0.30f).SetEase(Ease.OutCubic))
                    .Join(rect.DORotate(new Vector3(0f, 0f, 32f), 0.30f, RotateMode.Fast))
                    .Append(binding.FlashImage.DOFade(0.16f, 0.26f));
            }

            private static Tween PlayShine(Binding binding)
            {
                if (binding.ShineImage == null)
                {
                    return DOVirtual.DelayedCall(0f, () => { }, false);
                }

                RectTransform rect = binding.ShineImage.rectTransform;
                Vector2 home = rect.anchoredPosition;
                rect.anchoredPosition = home + new Vector2(-180f, 0f);
                return DOTween.Sequence()
                    .SetUpdate(true)
                    .Append(binding.ShineImage.DOFade(0.58f, 0.10f))
                    .Join(rect.DOAnchorPos(home + new Vector2(220f, 0f), 0.48f).SetEase(Ease.InOutSine))
                    .Append(binding.ShineImage.DOFade(0f, 0.16f))
                    .OnComplete(() => rect.anchoredPosition = home);
            }

            private static void PlayRewardFlight(Binding binding, Object tweenTarget, bool hasOutcome)
            {
                if (!hasOutcome || binding.RewardPanelView == null)
                {
                    CompletePresentation(binding);
                    return;
                }

                WheelOutcomeSnapshot snapshot = default(WheelOutcomeSnapshot);
                WheelOutcomePopupView view = binding.Owner;
                if (view == null || view._presenter == null)
                {
                    CompletePresentation(binding);
                    return;
                }

                snapshot = view._presenter.CurrentSnapshot;
                if (snapshot.Phase != WheelGamePhase.Won || snapshot.RewardAmount <= 0)
                {
                    CompletePresentation(binding);
                    return;
                }

                Sprite icon = ResolvePresentationIcon(snapshot);
                if (icon == null)
                {
                    CompletePresentation(binding);
                    return;
                }

                const float delay = 0.07f;
                const float duration = 0.72f;
                float latestLanding = delay + duration;
                PlayPopupIconFlight(binding, snapshot, icon, delay, duration);
                binding.RewardPanelView.CommitPendingRewards(latestLanding + 0.03f);
                DOTween.Sequence()
                    .SetTarget(tweenTarget)
                    .SetUpdate(true)
                    .AppendInterval(latestLanding + 0.16f)
                    .Append(binding.CanvasGroup.DOFade(0f, 0.12f))
                    .AppendCallback(() => CompletePresentation(binding));
            }

            private static void CompletePresentation(Binding binding)
            {
                RestoreSourceSliceRewardVisual();
                StopRewardBurst(binding);
                ClearMainIcon(binding);
                binding.Root.SetActive(false);
                WheelOutcomePopupView view = binding.Owner;
                if (view != null && view._presenter != null)
                {
                    view._presenter.MarkPresentationComplete();
                }
            }

            private static void PlayRewardBurst(Binding binding, WheelOutcomeSnapshot snapshot, Vector2 burstCenter)
            {
                if (snapshot.Phase != WheelGamePhase.Won || snapshot.Icon == null || binding.RewardBurstParticle == null)
                {
                    StopRewardBurst(binding);
                    return;
                }

                if (binding.RewardBurstCamera != null)
                {
                    binding.RewardBurstCamera.enabled = true;
                }

                if (binding.RewardBurstDisplay != null)
                {
                    binding.RewardBurstDisplay.enabled = true;
                    binding.RewardBurstDisplay.color = Color.white;
                    binding.RewardBurstDisplay.transform.SetAsLastSibling();
                    RectTransform displayRect = binding.RewardBurstDisplay.rectTransform;
                    RectTransform iconRect = binding.IconImage.rectTransform;
                    displayRect.anchoredPosition = burstCenter;
                    displayRect.sizeDelta = ResolveBurstDisplaySize(iconRect);
                    displayRect.localRotation = Quaternion.identity;
                    displayRect.localScale = Vector3.one;
                }

                if (binding.RewardBurstRenderer != null && binding.RewardBurstRenderer.sharedMaterial != null)
                {
                    binding.RewardBurstRenderer.sharedMaterial.mainTexture = snapshot.Icon.texture;
                }

                ParticleSystem.TextureSheetAnimationModule textureSheet = binding.RewardBurstParticle.textureSheetAnimation;
                if (textureSheet.enabled && textureSheet.spriteCount > 0)
                {
                    textureSheet.SetSprite(0, snapshot.Icon);
                }

                Transform particleTransform = binding.RewardBurstParticle.transform;
                particleTransform.localPosition = Vector3.zero;
                particleTransform.localRotation = Quaternion.identity;
                particleTransform.localScale = Vector3.one;
                binding.RewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                binding.RewardBurstParticle.Play(true);
#if UNITY_EDITOR
                if (binding.Owner != null)
                {
                    binding.Owner.CaptureRewardBurstDebug(binding, snapshot);
                }
#endif
            }

            private static Vector2 ResolveBurstDisplaySize(RectTransform iconRect)
            {
                float baseSize = Mathf.Max(iconRect.rect.width, iconRect.rect.height);
                float size = Mathf.Clamp(baseSize * 11.4f, 1680f, 1824f);
                return new Vector2(size, size);
            }

            private static void StopRewardBurst(Binding binding)
            {
                if (binding.RewardBurstParticle != null)
                {
                    binding.RewardBurstParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }

                if (binding.RewardBurstDisplay != null)
                {
                    binding.RewardBurstDisplay.enabled = false;
                }

                if (binding.RewardBurstCamera != null)
                {
                    binding.RewardBurstCamera.enabled = false;
                }
            }

            private static void PlayPopupIconFlight(
                Binding binding,
                WheelOutcomeSnapshot snapshot,
                Sprite icon,
                float delay,
                float duration)
            {
                RectTransform rect = binding.IconImage.rectTransform;
                rect.DOKill();
                binding.IconImage.DOKill();
                binding.IconImage.sprite = icon;
                binding.IconImage.enabled = true;
                binding.IconImage.color = ResolvePresentationIconColor(snapshot);
                binding.IconImage.preserveAspect = true;
                binding.IconImage.transform.SetAsLastSibling();

                Vector3 target = binding.RewardPanelView.ResolveLandingWorldPosition(snapshot.RewardId, 0, 1);
                Vector3 start = rect.position;
                Vector3 control = Vector3.Lerp(start, target, 0.54f) + ResolveFlightArcOffset(start, target);
                DOTween.Sequence()
                    .SetTarget(binding.IconImage)
                    .SetUpdate(true)
                    .AppendInterval(delay)
                    .Append(DOVirtual.Float(0f, 1f, duration, t => rect.position = EvaluateQuadraticBezier(start, control, target, t)).SetEase(Ease.InOutSine))
                    .Join(rect.DOScale(new Vector3(0.42f, 0.42f, 1f), duration).SetEase(Ease.InOutQuad))
                    .Join(rect.DOLocalRotate(new Vector3(0f, 0f, 10f), duration * 0.48f, RotateMode.Fast).SetEase(Ease.OutSine))
                    .Join(binding.IconImage.DOFade(1f, duration * 0.68f))
                    .Append(binding.IconImage.DOFade(0f, 0.08f));
            }

            private static Vector3 ResolveFlightArcOffset(Vector3 start, Vector3 target)
            {
                float horizontalDistance = Mathf.Abs(target.x - start.x);
                float verticalLift = Mathf.Clamp(horizontalDistance * 0.18f, 72f, 140f);
                return new Vector3(0f, verticalLift, 0f);
            }

            private static Vector3 EvaluateQuadraticBezier(Vector3 start, Vector3 control, Vector3 target, float t)
            {
                float inverse = 1f - t;
                return inverse * inverse * start + 2f * inverse * t * control + t * t * target;
            }

            public static Sprite ResolvePresentationIcon(WheelOutcomeSnapshot snapshot)
            {
                if (snapshot.Icon != null)
                {
                    return snapshot.Icon;
                }

                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView == null || snapshot.SourceSliceIndex < 0)
                {
                    return null;
                }

                RectTransform parent = null;
                if (wheelView.TryResolveSliceIconPresentation(
                    snapshot.SourceSliceIndex,
                    parent,
                    out Vector2 _,
                    out Sprite sprite,
                    out Color _))
                {
                    return sprite;
                }

                return null;
            }

            public static Color ResolvePresentationIconColor(WheelOutcomeSnapshot snapshot)
            {
                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView != null
                    && snapshot.SourceSliceIndex >= 0
                    && wheelView.TryResolveSliceIconPresentation(
                        snapshot.SourceSliceIndex,
                        null,
                        out Vector2 _,
                        out Sprite sprite,
                        out Color color)
                    && sprite != null)
                {
                    return WheelOutcomePopupView.ResolveVisibleIconColor(color);
                }

                return WheelOutcomePopupView.ResolveVisibleIconColor(Color.white);
            }

            private static void SuppressSourceSliceRewardVisual(WheelOutcomeSnapshot snapshot)
            {
                if (snapshot.Phase != WheelGamePhase.Won || snapshot.SourceSliceIndex < 0)
                {
                    return;
                }

                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView != null)
                {
                    wheelView.SuppressSliceRewardVisual(snapshot.SourceSliceIndex);
                }
            }

            private static void RestoreSourceSliceRewardVisual()
            {
                WheelView wheelView = WheelRuntimeLocator.WheelView;
                if (wheelView != null)
                {
                    wheelView.RestoreSuppressedSliceRewardVisual();
                }
            }

            private static Image ResolveFlightIcon(Binding binding)
            {
                if (binding.FlightIconPool == null || binding.FlightIconPool.Length == 0)
                {
                    return null;
                }

                for (int i = 0; i < binding.FlightIconPool.Length; i++)
                {
                    Image image = binding.FlightIconPool[i];
                    if (image != null)
                    {
                        return image;
                    }
                }

                return null;
            }

            private static void PlaySingleFlightIcon(
                Binding binding,
                WheelOutcomeSnapshot snapshot,
                Image image,
                float delay,
                float duration)
            {
                HideFlightIconPool(binding);
                image.gameObject.SetActive(true);
                image.transform.SetAsLastSibling();
                var rect = image.rectTransform;
                rect.position = binding.IconImage.rectTransform.position;
                rect.sizeDelta = binding.IconImage.rectTransform.sizeDelta * 0.46f;
                rect.localScale = Vector3.one;
                image.sprite = snapshot.Icon;
                image.color = Color.white;
                image.preserveAspect = true;
                image.raycastTarget = false;

                Vector3 target = binding.RewardPanelView.ResolveLandingWorldPosition(snapshot.RewardId, 0, 1);
                DOTween.Sequence()
                    .SetTarget(image)
                    .SetUpdate(true)
                    .AppendInterval(delay)
                    .Append(rect.DOMove(target, duration).SetEase(Ease.InOutCubic))
                    .Join(rect.DOScale(new Vector3(0.52f, 0.52f, 1f), duration).SetEase(Ease.InQuad))
                    .Join(image.DOFade(0.86f, duration))
                    .AppendCallback(() => image.gameObject.SetActive(false));
            }

            private static void HideFlightIconPool(Binding binding)
            {
                if (binding.FlightIconPool == null)
                {
                    return;
                }

                for (int i = 0; i < binding.FlightIconPool.Length; i++)
                {
                    Image image = binding.FlightIconPool[i];
                    if (image == null)
                    {
                        continue;
                    }

                    image.DOKill();
                    image.rectTransform.DOKill();
                    image.gameObject.SetActive(false);
                }
            }

            private static Vector2 ResolveIconStartPosition(
                Binding binding,
                WheelOutcomeSnapshot snapshot,
                RectTransform iconRect)
            {
                RectTransform parent = iconRect.parent as RectTransform;
                if (snapshot.SourceSliceIndex >= 0
                    && parent != null
                    && WheelRuntimeLocator.WheelView != null
                    && WheelRuntimeLocator.WheelView.TryResolveSliceIconAnchoredPosition(
                        snapshot.SourceSliceIndex,
                        parent,
                        out Vector2 anchoredPosition))
                {
                    return anchoredPosition;
                }

                return binding.IconHomeAnchoredPosition;
            }

            private static void SetTextAlpha(TextMeshProUGUI text, float alpha)
            {
                Color color = text.color;
                color.a = alpha;
                text.color = color;
            }

        }

        private static class VisibilityTable
        {
            public static void Apply(
                Binding binding,
                WheelOutcomePopupMotion motion,
                Object tweenTarget,
                WheelOutcomeSnapshot snapshot,
                int show,
                bool hasOutcome,
                ref Sequence sequence)
            {
                if (show == 0)
                {
                    Animator.Hide(binding, ref sequence);
                    return;
                }

                Animator.Show(binding, motion, tweenTarget, snapshot, hasOutcome, ref sequence);
            }
        }
    }
}
