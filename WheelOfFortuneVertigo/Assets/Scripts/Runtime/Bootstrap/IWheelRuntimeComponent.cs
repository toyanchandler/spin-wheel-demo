namespace Vertigo.Wheel.Runtime
{
    public interface IWheelRuntimeComponent
    {
        void Initialize(WheelEventBus eventBus);
        void Dispose();
    }
}
