using System.Text;
using TMPro;
using UnityEngine;

namespace Vertigo.Wheel.Diagnostics
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    [RequireComponent(typeof(WheelPerformanceMonitor))]
    public sealed class WheelPerformanceOverlay : MonoBehaviour
    {
        [SerializeField] private float _refreshInterval = 0.5f;

        private WheelPerformanceMonitor _monitor;
        private TextMeshProUGUI _text;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly StringBuilder _builder = new StringBuilder(192);
        private float _elapsed;
#endif

        private void Awake()
        {
            _monitor = GetComponent<WheelPerformanceMonitor>();
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
#if !(UNITY_EDITOR || DEVELOPMENT_BUILD)
            gameObject.SetActive(false);
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < _refreshInterval)
            {
                return;
            }

            _elapsed = 0f;
            Render(_monitor.Snapshot(Time.unscaledDeltaTime));
        }

        private void Render(WheelPerformanceMetricSnapshot snapshot)
        {
            _builder.Length = 0;
            _builder.Append("PERF\n");
            _builder.Append("Frame ");
            _builder.Append(snapshot.FrameMilliseconds.ToString("0.00"));
            _builder.Append(" ms / ");
            _builder.Append(snapshot.FramesPerSecond.ToString("0"));
            _builder.Append(" fps\nMain ");
            _builder.Append(snapshot.MainThreadMilliseconds.ToString("0.00"));
            _builder.Append(" ms  Render ");
            _builder.Append(snapshot.RenderThreadMilliseconds.ToString("0.00"));
            _builder.Append(" ms\nGC ");
            _builder.Append(snapshot.GcAllocatedKilobytes.ToString("0.0"));
            _builder.Append(" KB  Mem ");
            _builder.Append(snapshot.UsedMemoryMegabytes.ToString("0.0"));
            _builder.Append(" MB\nTweens ");
            _builder.Append(snapshot.ActiveTweenCount);
            _builder.Append("  Batches smoke: canvas/raycast OK");
            _text.text = _builder.ToString();
        }
#endif
    }
}
