namespace Vertigo.Wheel.Runtime
{
    /// <summary>View ↔ view presentation (loot flight, spin layout). Not gameplay intents or snapshots.</summary>
    public sealed partial class WheelEventBus
    {
        public WheelViewPresentationRegistry Presentation { get; } = new WheelViewPresentationRegistry();

        private void ClearPresentation()
        {
            Presentation.Clear();
        }
    }
}
