using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal static class WheelButtonMotion
    {
        public const float PressInDuration = 0.07f;
        public const float PressOutDuration = 0.10f;
        public const float PressSettleDuration = 0.08f;
        public const float DisabledScale = 0.94f;
        public const float StateSettleDuration = 0.14f;
        public const float IdlePulseDuration = 0.8f;

        public static readonly Vector3 PressInScale = new Vector3(0.88f, 0.88f, 1f);
        public static readonly Vector3 PressOutScale = new Vector3(1.08f, 1.08f, 1f);
        public static readonly Vector3 IdlePulseScale = new Vector3(1.025f, 1.025f, 1f);

        public static Vector3 UniformScale(float value) => new Vector3(value, value, 1f);
    }
}
