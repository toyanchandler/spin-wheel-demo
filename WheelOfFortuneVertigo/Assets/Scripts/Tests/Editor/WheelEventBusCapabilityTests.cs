using NUnit.Framework;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelEventBusCapabilityTests
    {
        [Test]
        public void WheelEventBus_ImplementsReadWriteCapabilityInterfaces()
        {
            var bus = new WheelEventBus();

            Assert.IsInstanceOf<IWheelUiIntentPublisher>(bus);
            Assert.IsInstanceOf<IWheelUiIntentSubscriber>(bus);
            Assert.IsInstanceOf<IWheelSnapshotPublisher>(bus);
            Assert.IsInstanceOf<IWheelSnapshotSubscriber>(bus);
        }
    }
}
