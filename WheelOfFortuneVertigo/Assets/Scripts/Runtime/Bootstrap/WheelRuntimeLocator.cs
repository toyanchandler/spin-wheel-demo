using System;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Session-scoped access to gameplay services. Populated by <see cref="WheelRuntimeCompositionRoot"/>
    /// before views bind via <see cref="RuntimeReady"/>.
    /// </summary>
    public static class WheelRuntimeLocator
    {
        public static event Action<WheelEventBus> RuntimeReady;
        public static event Action RuntimeStopped;

        public static bool IsReady { get; private set; }

        /// <summary>UI intents and snapshot events for the active play session.</summary>
        public static WheelEventBus EventBus { get; private set; }

        /// <summary>Designer config loaded from <c>Assets/Config/WheelGameSettings.asset</c>.</summary>
        public static WheelGameSettings Settings { get; private set; }

        /// <summary>Mutable zone, phase, inventory, and slice buffer.</summary>
        public static WheelGameState State { get; private set; }

        /// <summary>Raises snapshot events derived from <see cref="State"/>.</summary>
        public static WheelStatePublisher Publisher { get; private set; }

        /// <summary>Spin / leave / restart orchestration.</summary>
        public static WheelGameFlowController Flow { get; private set; }

        /// <summary>Wheel spin animation driver (typically <see cref="WheelSpinner"/>).</summary>
        public static IWheelSpinDriver Spinner { get; private set; }

        public static void RegisterSession(
            WheelEventBus eventBus,
            WheelGameSettings settings,
            WheelGameState state,
            WheelStatePublisher publisher,
            WheelGameFlowController flow,
            IWheelSpinDriver spinner)
        {
            EventBus = eventBus;
            Settings = settings;
            State = state;
            Publisher = publisher;
            Flow = flow;
            Spinner = spinner;
        }

        public static void NotifyRuntimeReady()
        {
            if (EventBus == null)
            {
                throw new InvalidOperationException(
                    "WheelRuntimeLocator cannot notify ready before RegisterSession.");
            }

            IsReady = true;
            RuntimeReady?.Invoke(EventBus);
        }

        public static bool TryGetSession(out WheelGameplaySession session)
        {
            return WheelGameplaySession.TryGet(out session);
        }

        public static void Clear()
        {
            IsReady = false;
            EventBus = null;
            Settings = null;
            State = null;
            Publisher = null;
            Flow = null;
            Spinner = null;
            RuntimeStopped?.Invoke();
        }
    }
}
