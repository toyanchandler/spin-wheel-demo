using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelWheelUiHost : WheelUiHostBase
    {
        [SerializeField, RequiredSceneReference]
        private WheelView _wheel;
        [SerializeField, RequiredSceneReference]
        private WheelSkinView _wheelBaseSkin;
        [SerializeField, RequiredSceneReference]
        private WheelSkinView _indicatorSkin;

        protected override void BindChildren(WheelEventBus eventBus)
        {
            RequireAssigned(_wheel, nameof(_wheel)).Bind(eventBus);
            RequireAssigned(_wheelBaseSkin, nameof(_wheelBaseSkin)).Bind(eventBus);
            RequireAssigned(_indicatorSkin, nameof(_indicatorSkin)).Bind(eventBus);
        }

        protected override void UnbindChildren()
        {
            _indicatorSkin.Unbind();
            _wheelBaseSkin.Unbind();
            _wheel.Unbind();
        }
    }
}
