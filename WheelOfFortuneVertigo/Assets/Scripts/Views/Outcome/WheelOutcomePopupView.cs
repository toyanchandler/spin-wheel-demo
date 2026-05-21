using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelOutcomePopupView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _summaryText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;

        private WheelEventBus _eventBus;
        private Presenter _presenter;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _presenter = new Presenter(CreateBinding(), this);
            _presenter.Reset();
            _eventBus.OutcomeResolved += OnOutcomeResolved;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Dispose()
        {
            _eventBus.OutcomeResolved -= OnOutcomeResolved;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
            _presenter.Reset();
            _presenter = null;
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
            return new Binding(_root, _iconImage, _titleText, _resultText, _summaryText, _canvasGroup, _contentRoot);
        }

        private readonly struct Binding
        {
            public readonly GameObject Root;
            public readonly Image IconImage;
            public readonly TextMeshProUGUI TitleText;
            public readonly TextMeshProUGUI ResultText;
            public readonly TextMeshProUGUI SummaryText;
            public readonly CanvasGroup CanvasGroup;
            public readonly RectTransform ContentRoot;

            public Binding(
                GameObject root,
                Image iconImage,
                TextMeshProUGUI titleText,
                TextMeshProUGUI resultText,
                TextMeshProUGUI summaryText,
                CanvasGroup canvasGroup,
                RectTransform contentRoot)
            {
                Root = root;
                IconImage = iconImage;
                TitleText = titleText;
                ResultText = resultText;
                SummaryText = summaryText;
                CanvasGroup = canvasGroup;
                ContentRoot = contentRoot;
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
                _state.HasOutcome = true;
                SyncVisibility();
            }

            public void HandleHud(bool isOutcomePopupAllowed)
            {
                _state.PopupAllowed = isOutcomePopupAllowed;
                _state.HasOutcome = isOutcomePopupAllowed && _state.HasOutcome;
                SyncVisibility();
            }

            private void SyncVisibility()
            {
                int show = System.Convert.ToInt32(_state.HasOutcome && _state.PopupAllowed);
                VisibilityTable.Apply(_binding, _motion, _tweenTarget, show, ref _sequence);
            }
        }

        private static class ContentApplier
        {
            public static void Apply(Binding binding, WheelOutcomeSnapshot snapshot)
            {
                binding.TitleText.text = snapshot.Title;
                binding.ResultText.text = snapshot.ResultText;
                binding.ResultText.color = snapshot.ResultColor;
                binding.SummaryText.text = snapshot.SummaryText;
                binding.IconImage.sprite = snapshot.Icon;
                binding.IconImage.color = snapshot.IconImageColor;
            }
        }

        private static class Animator
        {
            public static void Hide(Binding binding, ref Sequence sequence)
            {
                Kill(ref sequence);
                binding.Root.SetActive(false);
            }

            public static void Show(Binding binding, WheelOutcomePopupMotion motion, Object tweenTarget, ref Sequence sequence)
            {
                if (binding.Root.activeSelf)
                {
                    return;
                }

                Kill(ref sequence);
                binding.Root.SetActive(true);

                if (binding.CanvasGroup == null || binding.ContentRoot == null)
                {
                    return;
                }

                binding.CanvasGroup.alpha = 0f;
                float startScale = motion.startScale;
                binding.ContentRoot.localScale = new Vector3(startScale, startScale, 1f);
                sequence = DOTween.Sequence()
                    .SetTarget(tweenTarget)
                    .SetUpdate(true)
                    .Append(binding.CanvasGroup.DOFade(1f, motion.fadeDuration))
                    .Join(binding.ContentRoot.DOScale(Vector3.one, motion.scaleDuration).SetEase(motion.scaleEase));
            }

            private static void Kill(ref Sequence sequence)
            {
                if (sequence != null)
                {
                    sequence.Kill();
                    sequence = null;
                }
            }
        }

        private static class VisibilityTable
        {
            public static void Apply(Binding binding, WheelOutcomePopupMotion motion, Object tweenTarget, int show, ref Sequence sequence)
            {
                if (show == 0)
                {
                    Animator.Hide(binding, ref sequence);
                    return;
                }

                Animator.Show(binding, motion, tweenTarget, ref sequence);
            }
        }
    }
}
