using System;
using UnityEngine;
using Vertigo.Collections;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelView : MonoBehaviour
    {
        [SerializeField] private Transform _slicePoolRoot;

        [CollectChildren(nameof(_slicePoolRoot))]
        [SerializeField] private WheelSliceView[] _sliceViews = Array.Empty<WheelSliceView>();

        private WheelEventBus _eventBus;

        public void Bind(WheelEventBus eventBus)
        {
            RequireSliceViews();
            _eventBus = eventBus;
            _eventBus.ZoneChanged += OnZoneChanged;
        }

        public void Unbind()
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

        private void RequireSliceViews()
        {
            if (_sliceViews == null || _sliceViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no slice views. Collect children in the inspector or rebuild the scene.");
            }
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
                        slices[i].DisplayIcon,
                        slices[i].DisplayAmount,
                        slices[i].DisplayColor,
                        slicePositions[i],
                        slices[i].ShowAmountLabel);
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
