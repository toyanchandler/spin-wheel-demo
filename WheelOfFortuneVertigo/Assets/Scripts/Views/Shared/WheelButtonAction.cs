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

        [WheelInject] private WheelEventBus _eventBus;
        private CanvasGroup _canvasGroup;

        protected WheelEventBus EventBus { get { return _eventBus; } }
        protected Button Button { get { return _button; } }
        protected virtual bool ShouldPlayIdlePulse { get { return false; } }

        [WheelAfterInject]
        private void Connect()
        {
            RequireReferences();
            _canvasGroup = GetComponent<CanvasGroup>();
            _button.onClick.AddListener(HandleClick);
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _button.onClick.RemoveListener(HandleClick);
            _eventBus.HudStateChanged -= OnHudStateChanged;
            transform.DOKill();
        }

        private void HandleClick()
        {
            PlayClickFeedback();
            Execute();
        }

        protected virtual void PlayClickFeedback()
        {
            transform.DOKill();
            transform.localScale = Vector3.one;
            DOTween.Sequence()
                .SetTarget(transform)
                .SetUpdate(true)
                .Append(transform.DOScale(WheelButtonMotion.PressInScale, WheelButtonMotion.PressInDuration).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(WheelButtonMotion.PressOutScale, WheelButtonMotion.PressOutDuration).SetEase(Ease.OutCubic))
                .Append(transform.DOScale(Vector3.one, WheelButtonMotion.PressSettleDuration).SetEase(Ease.OutQuad));
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            bool visible = IsVisible(snapshot);
            _button.interactable = visible && IsInteractable(snapshot);
            ApplyLabel(snapshot);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }

            transform.DOKill();
            float targetScale = visible && _button.interactable ? 1f : WheelButtonMotion.DisabledScale;
            Tween settle = transform
                .DOScale(WheelButtonMotion.UniformScale(targetScale), WheelButtonMotion.StateSettleDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
            if (visible && _button.interactable && ShouldPlayIdlePulse)
            {
                settle.OnComplete(() =>
                {
                    transform.DOScale(WheelButtonMotion.IdlePulseScale, WheelButtonMotion.IdlePulseDuration)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetTarget(transform)
                        .SetUpdate(true);
                });
            }
        }

        private void OnDisable()
        {
            transform.DOKill();
        }

        private void RequireReferences()
        {
            if (_button == null)
            {
                throw new System.InvalidOperationException(name + " requires a serialized Button reference.");
            }
        }

        protected virtual bool IsVisible(WheelHudSnapshot snapshot)
        {
            return true;
        }

        protected virtual string ResolveLabel(WheelHudSnapshot snapshot)
        {
            return null;
        }

        private void ApplyLabel(WheelHudSnapshot snapshot)
        {
            if (_labelText == null)
            {
                return;
            }

            string label = ResolveLabel(snapshot);
            if (!string.IsNullOrEmpty(label))
            {
                _labelText.text = label;
            }
        }

        protected abstract bool IsInteractable(WheelHudSnapshot snapshot);
        protected abstract void Execute();
    }
}
