using System;
using UnityEngine;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelSliceVisualState
    {
        private readonly WheelSliceView[] _sliceViews;
        private readonly Action _playBombShake;

        private int _suppressedRewardVisualSliceIndex = -1;
        private int _highlightedSliceIndex = -1;
        private bool _areAllRewardVisualsSuppressed;

        public WheelSliceVisualState(WheelSliceView[] sliceViews, Action playBombShake)
        {
            _sliceViews = sliceViews;
            _playBombShake = playBombShake;
        }

        public void SuppressSliceRewardVisual(int sliceIndex)
        {
            RestoreSuppressedSliceRewardVisual();
            WheelSliceView sliceView = ResolveActiveSlice(sliceIndex);
            if (sliceView == null)
            {
                return;
            }

            _suppressedRewardVisualSliceIndex = sliceIndex;
            sliceView.SetRewardVisualVisible(false);
        }

        public void SuppressAllRewardVisuals()
        {
            _areAllRewardVisualsSuppressed = true;
            SetAllRewardVisualsVisible(false);
        }

        public void RestoreAllRewardVisuals()
        {
            _areAllRewardVisualsSuppressed = false;
            SetAllRewardVisualsVisible(true);
        }

        public void RestoreSuppressedSliceRewardVisual()
        {
            if (!TryGetSlice(_suppressedRewardVisualSliceIndex, out WheelSliceView sliceView))
            {
                _suppressedRewardVisualSliceIndex = -1;
                return;
            }

            if (sliceView.gameObject.activeInHierarchy)
            {
                sliceView.SetRewardVisualVisible(true);
            }

            _suppressedRewardVisualSliceIndex = -1;
        }

        public void ApplySuppressedSliceRewardVisual()
        {
            if (_areAllRewardVisualsSuppressed)
            {
                SetAllRewardVisualsVisible(false);
                return;
            }

            WheelSliceView sliceView = ResolveActiveSlice(_suppressedRewardVisualSliceIndex);
            if (sliceView != null)
            {
                sliceView.SetRewardVisualVisible(false);
            }
        }

        public void HighlightLandedSlice(int sliceIndex, bool isBomb, Color accentColor)
        {
            ClearImpactHighlight();
            WheelSliceView sliceView = ResolveActiveSlice(sliceIndex);
            if (sliceView == null)
            {
                return;
            }

            _highlightedSliceIndex = sliceIndex;
            if (isBomb)
            {
                sliceView.PlayBombHitPulse();
                _playBombShake?.Invoke();
                return;
            }

            sliceView.PlayRewardHitPulse(accentColor);
        }

        public void EnsureBombSliceHighlight(int sliceIndex)
        {
            if (_highlightedSliceIndex == sliceIndex)
            {
                return;
            }

            HighlightLandedSlice(sliceIndex, true, Color.white);
        }

        public void ClearImpactHighlight()
        {
            if (!TryGetSlice(_highlightedSliceIndex, out WheelSliceView sliceView))
            {
                _highlightedSliceIndex = -1;
                return;
            }

            sliceView.ClearImpactVisual();
            _highlightedSliceIndex = -1;
        }

        private void SetAllRewardVisualsVisible(bool isVisible)
        {
            if (_sliceViews == null)
            {
                return;
            }

            for (int i = 0; i < _sliceViews.Length; i++)
            {
                WheelSliceView sliceView = _sliceViews[i];
                if (sliceView == null || !sliceView.gameObject.activeInHierarchy)
                {
                    continue;
                }

                sliceView.SetRewardVisualVisible(isVisible);
            }
        }

        private WheelSliceView ResolveActiveSlice(int sliceIndex)
        {
            if (!TryGetSlice(sliceIndex, out WheelSliceView sliceView) || !sliceView.gameObject.activeInHierarchy)
            {
                return null;
            }

            return sliceView;
        }

        private bool TryGetSlice(int sliceIndex, out WheelSliceView sliceView)
        {
            sliceView = null;
            if (_sliceViews == null || sliceIndex < 0 || sliceIndex >= _sliceViews.Length)
            {
                return false;
            }

            sliceView = _sliceViews[sliceIndex];
            return sliceView != null;
        }
    }
}
