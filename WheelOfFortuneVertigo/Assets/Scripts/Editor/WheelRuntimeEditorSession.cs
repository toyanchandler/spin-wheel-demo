using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    /// <summary>Editor-only access to the active play-mode gameplay session.</summary>
    internal static class WheelRuntimeEditorSession
    {
        public static bool IsReady => WheelGameplaySession.TryGet(out WheelGameplaySession session) && session.IsValid;

        public static bool TryGet(out WheelGameplaySession session)
        {
            return WheelGameplaySession.TryGet(out session) && session.IsValid;
        }
    }
}
