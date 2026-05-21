using System;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public static class WheelRuntimeLocator
    {
        public static event Action<WheelEventBus> RuntimeReady;
        public static event Action RuntimeStopped;

        public static WheelEventBus EventBus { get; private set; }
        public static WheelGameSettings Settings { get; private set; }
        public static WheelGameState State { get; private set; }
        public static WheelStatePublisher Publisher { get; private set; }
        public static WheelSpinner Spinner { get; private set; }
        public static bool IsReady { get; private set; }

        public static void RegisterSettings(WheelGameSettings settings)
        {
            Settings = settings;
        }

        public static void RegisterSpinner(WheelSpinner spinner)
        {
            Spinner = spinner;
        }

        public static void RegisterGameplay(WheelEventBus eventBus, WheelGameState state, WheelStatePublisher publisher)
        {
            EventBus = eventBus;
            State = state;
            Publisher = publisher;
        }

        public static void NotifyRuntimeReady()
        {
            IsReady = true;
            RuntimeReady?.Invoke(EventBus);
        }

        public static void Clear()
        {
            IsReady = false;
            RuntimeStopped?.Invoke();
            RuntimeReady = null;
            RuntimeStopped = null;
            EventBus = null;
            Settings = null;
            State = null;
            Publisher = null;
            Spinner = null;
        }
    }
}
