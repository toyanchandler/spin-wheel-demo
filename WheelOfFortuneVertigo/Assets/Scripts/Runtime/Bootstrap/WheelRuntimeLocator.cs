using System;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelRuntimeLocator
    {
        public static event Action<WheelEventBus> RuntimeReady;
        public static event Action RuntimeStopped;

        public static bool IsReady { get; private set; }

        public static void NotifyRuntimeReady(WheelEventBus eventBus)
        {
            IsReady = true;
            RuntimeReady?.Invoke(eventBus);
        }

        public static void Clear()
        {
            IsReady = false;
            RuntimeStopped?.Invoke();
        }
    }
}
