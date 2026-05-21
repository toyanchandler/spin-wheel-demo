using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelStaticUiHost : WheelUiHostBase
    {
        [SerializeField, RequiredSceneReference]
        private WheelBackgroundView _background;

        protected override void BindChildren(WheelEventBus eventBus)
        {
            RequireAssigned(_background, nameof(_background)).Bind(eventBus);
        }

        protected override void UnbindChildren()
        {
            _background.Unbind();
        }
    }
}
