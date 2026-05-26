using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.Wheel.Views
{
    public abstract class WheelOutcomePopupComponentBinding<TComponent> : MonoBehaviour
        where TComponent : Component
    {
        private TComponent _component;

        protected TComponent Component
        {
            get
            {
                if (_component == null && !TryGetComponent(out _component))
                {
                    throw new InvalidOperationException(name + " requires " + typeof(TComponent).Name + ".");
                }

                return _component;
            }
        }
    }

    public abstract class WheelOutcomePopupRectBinding : WheelOutcomePopupComponentBinding<RectTransform>
    {
        public RectTransform RectTransform { get { return Component; } }
    }

    public abstract class WheelOutcomePopupImageBinding : WheelOutcomePopupComponentBinding<Image>
    {
        public Image Image { get { return Component; } }
        public RectTransform RectTransform { get { return Component.rectTransform; } }
    }

    public abstract class WheelOutcomePopupTextBinding : WheelOutcomePopupComponentBinding<TextMeshProUGUI>
    {
        public TextMeshProUGUI Text { get { return Component; } }
    }

    public abstract class WheelOutcomePopupObjectBinding : MonoBehaviour
    {
        public GameObject Target { get { return gameObject; } }
    }
}
