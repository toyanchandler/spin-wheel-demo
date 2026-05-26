using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelExitConfirmationBindings : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Button _comeBackButton;
        [SerializeField] private TextMeshProUGUI _collectButtonLabelText;
        [SerializeField] private TextMeshProUGUI _comeBackButtonLabelText;

        public bool IsVisible { get { return _root.activeSelf; } }

        public void Validate()
        {
            Require(_root, nameof(_root));
            Require(_canvasGroup, nameof(_canvasGroup));
            Require(_contentRoot, nameof(_contentRoot));
            Require(_titleText, nameof(_titleText));
            Require(_bodyText, nameof(_bodyText));
            Require(_collectButton, nameof(_collectButton));
            Require(_comeBackButton, nameof(_comeBackButton));
            Require(_collectButtonLabelText, nameof(_collectButtonLabelText));
            Require(_comeBackButtonLabelText, nameof(_comeBackButtonLabelText));
        }

        public void AddButtonListeners(UnityAction collect, UnityAction comeBack)
        {
            _collectButton.onClick.AddListener(collect);
            _comeBackButton.onClick.AddListener(comeBack);
        }

        public void RemoveButtonListeners(UnityAction collect, UnityAction comeBack)
        {
            _collectButton.onClick.RemoveListener(collect);
            _comeBackButton.onClick.RemoveListener(comeBack);
        }

        public void ApplyCopy(WheelHudSnapshot snapshot)
        {
            _titleText.text = snapshot.ExitConfirmationTitle;
            _bodyText.text = snapshot.ExitConfirmationBody;
            _collectButtonLabelText.text = snapshot.ExitCollectButtonLabel;
            _comeBackButtonLabelText.text = snapshot.ExitComeBackButtonLabel;
        }

        public Sequence Show()
        {
            _root.SetActive(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _contentRoot.localScale = new Vector3(0.92f, 0.92f, 1f);

            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(_canvasGroup.DOFade(1f, 0.16f))
                .Join(_contentRoot.DOScale(Vector3.one, 0.22f).SetEase(Ease.OutBack));
        }

        public Sequence HideAnimated(TweenCallback onComplete)
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            return DOTween.Sequence()
                .SetUpdate(true)
                .Append(_canvasGroup.DOFade(0f, 0.12f))
                .Join(_contentRoot.DOScale(new Vector3(0.96f, 0.96f, 1f), 0.12f).SetEase(Ease.OutQuad))
                .OnComplete(onComplete);
        }

        public void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _root.SetActive(false);
        }

        private void Require(UnityEngine.Object value, string fieldName)
        {
            if (value == null)
            {
                throw new InvalidOperationException(name + " requires exit confirmation binding " + fieldName + ".");
            }
        }
    }
}
