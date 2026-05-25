using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [RequireComponent(typeof(Button))]
    public abstract class WheelButtonAction : MonoBehaviour
    {
        [SerializeField] private Button _button;
        private WheelEventBus _eventBus;
        private CanvasGroup _canvasGroup;

        protected WheelEventBus EventBus { get { return _eventBus; } }
        protected Button Button { get { return _button; } }

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _canvasGroup = GetComponent<CanvasGroup>();
            _button.onClick.AddListener(HandleClick);
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _button.onClick.RemoveListener(HandleClick);
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
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
                .Append(transform.DOScale(new Vector3(0.88f, 0.88f, 1f), 0.07f).SetEase(Ease.OutQuad))
                .Append(transform.DOScale(new Vector3(1.08f, 1.08f, 1f), 0.10f).SetEase(Ease.OutCubic))
                .Append(transform.DOScale(Vector3.one, 0.08f).SetEase(Ease.OutQuad));
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            bool visible = IsVisible(snapshot);
            _button.interactable = visible && IsInteractable(snapshot);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
                _canvasGroup.interactable = visible;
                _canvasGroup.blocksRaycasts = visible;
            }

            transform.DOKill();
            float targetScale = visible && _button.interactable ? 1f : 0.94f;
            Tween settle = transform.DOScale(new Vector3(targetScale, targetScale, 1f), 0.14f).SetEase(Ease.OutQuad).SetUpdate(true);
            if (visible && _button.interactable && name.Contains("spin"))
            {
                settle.OnComplete(() =>
                {
                    transform.DOScale(new Vector3(1.025f, 1.025f, 1f), 0.8f)
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

        protected virtual bool IsVisible(WheelHudSnapshot snapshot)
        {
            return true;
        }

        protected abstract bool IsInteractable(WheelHudSnapshot snapshot);
        protected abstract void Execute();
    }
}
