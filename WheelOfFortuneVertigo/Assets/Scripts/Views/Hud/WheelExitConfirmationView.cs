using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelExitConfirmationView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Button _comeBackButton;

        private WheelEventBus _eventBus;
        private Sequence _sequence;

        public void Bind(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.LeaveConfirmationRequested += Show;
            _eventBus.HudStateChanged += OnHudStateChanged;
            _collectButton.onClick.AddListener(CollectRewards);
            _comeBackButton.onClick.AddListener(ComeBack);
            _titleText.text = "COLLECT REWARDS?";
            _bodyText.text = "Are you sure you want to go out and collect your rewards? We have saved the best rewards for last!";
            HideImmediate();
        }

        public void Unbind()
        {
            _eventBus.LeaveConfirmationRequested -= Show;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _collectButton.onClick.RemoveListener(CollectRewards);
            _comeBackButton.onClick.RemoveListener(ComeBack);
            _eventBus = null;
            KillSequence();
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            if (_root.activeSelf && !snapshot.CanLeave)
            {
                HideImmediate();
            }
        }

        private void Show()
        {
            KillSequence();
            _root.SetActive(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _contentRoot.localScale = new Vector3(0.92f, 0.92f, 1f);
            _sequence = DOTween.Sequence()
                .SetUpdate(true)
                .Append(_canvasGroup.DOFade(1f, 0.16f))
                .Join(_contentRoot.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack));
        }

        private void ComeBack()
        {
            KillSequence();
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _sequence = DOTween.Sequence()
                .SetUpdate(true)
                .Append(_canvasGroup.DOFade(0f, 0.12f))
                .Join(_contentRoot.DOScale(new Vector3(0.96f, 0.96f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .OnComplete(HideImmediate);
        }

        private void CollectRewards()
        {
            HideImmediate();
            _eventBus.RequestLeave();
        }

        private void HideImmediate()
        {
            KillSequence();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _root.SetActive(false);
        }

        private void OnDisable()
        {
            KillSequence();
        }

        private void KillSequence()
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }
        }
    }
}
