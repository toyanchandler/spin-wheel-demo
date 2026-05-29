namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Session event hub split into two directions:
    /// <see cref="UiIntents"/> (views → gameplay) and <see cref="Snapshots"/> (gameplay → views).
    /// See <c>ARCHITECTURE_AND_LOGIC.md</c> section 2.
    /// </summary>
    public sealed partial class WheelEventBus :
        IWheelUiIntentPublisher,
        IWheelUiIntentSubscriber,
        IWheelSnapshotPublisher,
        IWheelSnapshotSubscriber
    {
        public void Clear()
        {
            ClearUiIntents();
            ClearSnapshotEvents();
            ClearPresentation();
        }
    }
}
