using DG.Tweening;
using UnityEngine;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Unity.Profiling;
#endif

namespace Vertigo.Wheel.Diagnostics
{
    public sealed class WheelPerformanceMonitor : MonoBehaviour
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const float NanosecondsToMilliseconds = 0.000001f;
        private const float BytesToKilobytes = 1f / 1024f;
        private const float BytesToMegabytes = 1f / (1024f * 1024f);

        private ProfilerRecorder _mainThreadTime;
        private ProfilerRecorder _renderThreadTime;
        private ProfilerRecorder _gcAllocatedInFrame;
        private ProfilerRecorder _totalUsedMemory;
        private bool _running;
#endif

        public bool IsRunning
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                return _running;
#else
                return false;
#endif
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StartRecorders();
#else
            gameObject.SetActive(false);
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StopRecorders();
#endif
        }

        public void StartRecorders()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_running) return;
            _mainThreadTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            _renderThreadTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Render Thread", 15);
            _gcAllocatedInFrame = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame", 15);
            _totalUsedMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "Total Used Memory", 1);
            _running = true;
#endif
        }

        public void StopRecorders()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!_running) return;
            DisposeRecorder(ref _mainThreadTime);
            DisposeRecorder(ref _renderThreadTime);
            DisposeRecorder(ref _gcAllocatedInFrame);
            DisposeRecorder(ref _totalUsedMemory);
            _running = false;
#endif
        }

        public WheelPerformanceMetricSnapshot Snapshot(float unscaledDeltaTime)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            float frameMilliseconds = unscaledDeltaTime * 1000f;
            float framesPerSecond = frameMilliseconds > 0f ? 1000f / frameMilliseconds : 0f;
            return new WheelPerformanceMetricSnapshot(
                frameMilliseconds,
                framesPerSecond,
                RecorderAverage(_mainThreadTime) * NanosecondsToMilliseconds,
                RecorderAverage(_renderThreadTime) * NanosecondsToMilliseconds,
                LastValue(_gcAllocatedInFrame) * BytesToKilobytes,
                LastValue(_totalUsedMemory) * BytesToMegabytes,
                DOTween.TotalActiveTweens());
#else
            return default(WheelPerformanceMetricSnapshot);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void DisposeRecorder(ref ProfilerRecorder recorder)
        {
            if (recorder.Valid) recorder.Dispose();
        }

        private static float RecorderAverage(ProfilerRecorder recorder)
        {
            if (!recorder.Valid || recorder.Count == 0) return 0f;
            double total = 0d;
            for (int i = 0; i < recorder.Count; i++) total += recorder.GetSample(i).Value;
            return (float)(total / recorder.Count);
        }

        private static long LastValue(ProfilerRecorder recorder) => recorder.Valid ? recorder.LastValue : 0L;
#endif
    }
}
