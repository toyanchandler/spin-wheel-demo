namespace Vertigo.Wheel.Diagnostics
{
    public readonly struct WheelPerformanceMetricSnapshot
    {
        public readonly float FrameMilliseconds;
        public readonly float FramesPerSecond;
        public readonly float MainThreadMilliseconds;
        public readonly float RenderThreadMilliseconds;
        public readonly float GcAllocatedKilobytes;
        public readonly float UsedMemoryMegabytes;
        public readonly int ActiveTweenCount;

        public WheelPerformanceMetricSnapshot(
            float frameMilliseconds,
            float framesPerSecond,
            float mainThreadMilliseconds,
            float renderThreadMilliseconds,
            float gcAllocatedKilobytes,
            float usedMemoryMegabytes,
            int activeTweenCount)
        {
            FrameMilliseconds = frameMilliseconds;
            FramesPerSecond = framesPerSecond;
            MainThreadMilliseconds = mainThreadMilliseconds;
            RenderThreadMilliseconds = renderThreadMilliseconds;
            GcAllocatedKilobytes = gcAllocatedKilobytes;
            UsedMemoryMegabytes = usedMemoryMegabytes;
            ActiveTweenCount = activeTweenCount;
        }
    }
}
