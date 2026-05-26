using TMPro;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelRewardOpeningRootBinding
    {
        private readonly GameObject _root;
        private readonly TextMeshProUGUI _titleText;
        private readonly CanvasGroup _canvasGroup;
        private readonly RectTransform _contentRoot;

        public CanvasGroup CanvasGroup { get { return _canvasGroup; } }
        public RectTransform ContentRoot { get { return _contentRoot; } }

        public WheelRewardOpeningRootBinding(
            GameObject root,
            TextMeshProUGUI titleText,
            CanvasGroup canvasGroup,
            RectTransform contentRoot)
        {
            _root = root;
            _titleText = titleText;
            _canvasGroup = canvasGroup;
            _contentRoot = contentRoot;
        }

        public void Deactivate()
        {
            _root.SetActive(false);
        }

        public void SetTitle(string title)
        {
            _titleText.text = title;
        }

        public void PrepareShow()
        {
            _root.SetActive(true);
            _root.transform.localScale = Vector3.one;
            _canvasGroup.alpha = 0f;
            _contentRoot.localScale = WheelRewardOpeningMotion.ShowStartScale;
        }

        public void CompleteHide()
        {
            _root.SetActive(false);
        }
    }
}
