using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Typed facade over <see cref="WheelRuntimeLocator"/> for gameplay and editor tools.
    /// Prefer this over reaching for individual static properties.
    /// </summary>
    public readonly struct WheelGameplaySession
    {
        public WheelEventBus EventBus { get; }
        public WheelGameSettings Settings { get; }
        public WheelGameState State { get; }
        public WheelStatePublisher Publisher { get; }
        public WheelGameFlowController Flow { get; }
        public IWheelSpinDriver Spinner { get; }

        public WheelGameplaySession(
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

        public bool IsValid =>
            EventBus != null
            && Settings != null
            && State != null
            && Publisher != null
            && Flow != null
            && Spinner != null;

        public static bool TryGet(out WheelGameplaySession session)
        {
            if (!WheelRuntimeLocator.IsReady)
            {
                session = default;
                return false;
            }

            session = new WheelGameplaySession(
                WheelRuntimeLocator.EventBus,
                WheelRuntimeLocator.Settings,
                WheelRuntimeLocator.State,
                WheelRuntimeLocator.Publisher,
                WheelRuntimeLocator.Flow,
                WheelRuntimeLocator.Spinner);

            return session.IsValid;
        }
    }
}
