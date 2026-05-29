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

        public CanvasGroup CanvasGroup => _canvasGroup;
        public RectTransform ContentRoot => _contentRoot;
        public bool IsVisible => _root.activeSelf;

        public void Validate()
        {
            WheelWiringValidation.Require(this, "exit confirmation", _root, nameof(_root));
            WheelWiringValidation.Require(this, "exit confirmation", _canvasGroup, nameof(_canvasGroup));
            WheelWiringValidation.Require(this, "exit confirmation", _contentRoot, nameof(_contentRoot));
            WheelWiringValidation.Require(this, "exit confirmation", _titleText, nameof(_titleText));
            WheelWiringValidation.Require(this, "exit confirmation", _bodyText, nameof(_bodyText));
            WheelWiringValidation.Require(this, "exit confirmation", _collectButton, nameof(_collectButton));
            WheelWiringValidation.Require(this, "exit confirmation", _comeBackButton, nameof(_comeBackButton));
            WheelWiringValidation.Require(this, "exit confirmation", _collectButtonLabelText, nameof(_collectButtonLabelText));
            WheelWiringValidation.Require(this, "exit confirmation", _comeBackButtonLabelText, nameof(_comeBackButtonLabelText));
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
            WheelHudExitConfirmationSnapshot copy = snapshot.ExitConfirmation;
            _titleText.text = copy.Title;
            _bodyText.text = copy.Body;
            _collectButtonLabelText.text = copy.CollectButtonLabel;
            _comeBackButtonLabelText.text = copy.ComeBackButtonLabel;
        }

        public void PrepareShow()
        {
            _root.SetActive(true);
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _contentRoot.localScale = new Vector3(0.92f, 0.92f, 1f);
        }

        public void PrepareHide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        public void HideImmediate()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _root.SetActive(false);
        }
    }
}
