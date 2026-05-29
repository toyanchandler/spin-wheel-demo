namespace Vertigo.Wheel.Views
{
    internal static class WheelAngleUtility
    {
        public static float NormalizeUnsignedAngle(float angle)
        {
            angle %= 360f;
            return angle < 0f ? angle + 360f : angle;
        }

        public static float NormalizeSignedAngle(float angle)
        {
            angle %= 360f;
            if (angle > 180f)
            {
                angle -= 360f;
            }

            if (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }
    }
}
