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

        protected WheelEventBus EventBus { get { return _eventBus; } }

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
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
            transform.DOKill();
            transform.localScale = Vector3.one;
            transform.DOPunchScale(new Vector3(0.08f, 0.08f, 0f), 0.16f, 6, 0.35f).SetUpdate(true);
            Execute();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _button.interactable = IsInteractable(snapshot);
            transform.DOKill();
            float targetScale = _button.interactable ? 1f : 0.94f;
            transform.DOScale(new Vector3(targetScale, targetScale, 1f), 0.14f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        private void OnDisable()
        {
            transform.DOKill();
        }

        protected abstract bool IsInteractable(WheelHudSnapshot snapshot);
        protected abstract void Execute();
    }
}
