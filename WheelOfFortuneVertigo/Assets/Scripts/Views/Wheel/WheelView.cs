using System;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelView : MonoBehaviour, IWheelRuntimeComponent
    {
        [SerializeField] private WheelSliceView[] _sliceViews;

        private WheelEventBus _eventBus;

        public void Initialize(WheelEventBus eventBus)
        {
            _eventBus = eventBus;
            _eventBus.ZoneChanged += OnZoneChanged;
        }

        public void Dispose()
        {
            _eventBus.ZoneChanged -= OnZoneChanged;
            _eventBus = null;
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            SlicePoolRenderer.Render(
                _sliceViews,
                snapshot.Slices,
                snapshot.SliceCount,
                snapshot.SlicePositions,
                snapshot.SliceIconSize);
        }

        private static class SlicePoolRenderer
        {
            public static void Render(
                WheelSliceView[] pool,
                WheelSliceDefinition[] slices,
                int sliceCount,
                Vector2[] slicePositions,
                Vector2 iconSize)
            {
                if (sliceCount > pool.Length)
                {
                    throw new InvalidOperationException("Wheel slice pool is smaller than configured slice count. Rebuild the scene hierarchy.");
                }

                if (slicePositions == null || slicePositions.Length < sliceCount)
                {
                    throw new InvalidOperationException("Wheel zone snapshot is missing precomputed slice positions.");
                }

                int visibleCount = Mathf.Min(sliceCount, pool.Length);
                for (int i = 0; i < visibleCount; i++)
                {
                    WheelSlicePresentation presentation = new WheelSlicePresentation(
                        slices[i].displayIcon,
                        slices[i].displayAmount,
                        slices[i].displayColor,
                        slicePositions[i],
                        slices[i].showAmountLabel);
                    pool[i].Apply(presentation, iconSize);
                }

                for (int i = visibleCount; i < pool.Length; i++)
                {
                    pool[i].Hide();
                }
            }
        }
    }
}
