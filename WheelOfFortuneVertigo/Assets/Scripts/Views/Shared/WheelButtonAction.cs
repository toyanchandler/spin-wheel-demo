using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    [RequireComponent(typeof(Button))]
    public abstract class WheelButtonAction : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _labelText;

        [WheelInject] private IWheelUiIntentPublisher _uiIntents;
        [WheelInject] private IWheelSnapshotSubscriber _snapshots;
        private CanvasGroup _canvasGroup;

        protected IWheelUiIntentPublisher UiIntents => _uiIntents;
        protected Button Button => _button;
        protected virtual bool ShouldPlayIdlePulse => false;

        [WheelAfterInject]
        private void Connect()
        {
            RequireReferences();
            _canvasGroup = GetComponent<CanvasGroup>();
            _button.onClick.AddListener(HandleClick);
            _snapshots.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _button.onClick.RemoveListener(HandleClick);
            _snapshots.HudStateChanged -= OnHudStateChanged;
            DOTween.Kill(transform);
        }

        private void HandleClick()
        {
            PlayClickFeedback();
            Execute();
        }

        protected virtual void PlayClickFeedback()
        {
            WheelButtonFeedbackAnimator.PlayClickFeedback(transform);
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            WheelHudButtonPresentation.Apply(
                transform,
                _button,
                _canvasGroup,
                snapshot,
                IsVisible(snapshot),
                IsInteractable(snapshot),
                ResolveLabel(snapshot),
                _labelText,
                ShouldPlayIdlePulse);
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
        }

        private void RequireReferences()
        {
            if (_button == null) throw new System.InvalidOperationException(name + " requires a serialized Button reference.");
        }

        protected virtual bool IsVisible(WheelHudSnapshot snapshot) => true;

        protected virtual string ResolveLabel(WheelHudSnapshot snapshot) => null;

        protected abstract bool IsInteractable(WheelHudSnapshot snapshot);
        protected abstract void Execute();
    }
}
