namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// View-to-view coordination on the session bus (not gameplay state).
    /// Views register handlers in <c>[WheelAfterInject]</c>; consumers use <see cref="WheelEventBus.Presentation"/> only.
    /// </summary>
    public sealed class WheelViewPresentationRegistry
    {
        public WheelLootPresentationChannel Loot { get; } = new WheelLootPresentationChannel();
        public WheelSpinPresentationChannel Spin { get; } = new WheelSpinPresentationChannel();

        public void Clear()
        {
            Loot.Clear();
            Spin.Clear();
        }
    }
}
