#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    public static class VertigoPlayModeCommands
    {
        [MenuItem("Vertigo Case/Play Commands/Spin", true)]
        [MenuItem("Vertigo Case/Play Commands/Retry", true)]
        [MenuItem("Vertigo Case/Play Commands/Exit", true)]
        private static bool ValidatePlayCommand()
        {
            return Application.isPlaying;
        }

        [MenuItem("Vertigo Case/Play Commands/Spin")]
        private static void Spin()
        {
            Runtime().RequestSpin();
        }

        [MenuItem("Vertigo Case/Play Commands/Retry")]
        private static void Retry()
        {
            Runtime().RequestRestart();
        }

        [MenuItem("Vertigo Case/Play Commands/Exit")]
        private static void Exit()
        {
            Runtime().RequestLeave();
        }

        private static WheelRuntimeCompositionRoot Runtime()
        {
            return Object.FindObjectOfType<WheelRuntimeCompositionRoot>();
        }
    }
}
#endif
