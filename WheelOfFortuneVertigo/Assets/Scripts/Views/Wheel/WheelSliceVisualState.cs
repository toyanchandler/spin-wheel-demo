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
            WheelSliceView sliceView = WheelSliceArrayLookup.GetActive(_sliceViews, sliceIndex);
            if (sliceView == null) return;

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
            WheelSliceView sliceView = WheelSliceArrayLookup.Get(_sliceViews, _suppressedRewardVisualSliceIndex);
            _suppressedRewardVisualSliceIndex = -1;
            if (sliceView == null) return;

            if (sliceView.gameObject.activeInHierarchy) sliceView.SetRewardVisualVisible(true);
        }

        public void ApplySuppressedSliceRewardVisual()
        {
            if (_areAllRewardVisualsSuppressed)
            {
                SetAllRewardVisualsVisible(false);
                return;
            }

            WheelSliceView sliceView = WheelSliceArrayLookup.GetActive(_sliceViews, _suppressedRewardVisualSliceIndex);
            sliceView?.SetRewardVisualVisible(false);
        }

        public void HighlightLandedSlice(int sliceIndex, bool isBomb, Color accentColor)
        {
            ClearImpactHighlight();
            WheelSliceView sliceView = WheelSliceArrayLookup.GetActive(_sliceViews, sliceIndex);
            if (sliceView == null) return;

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
            if (_highlightedSliceIndex == sliceIndex) return;

            HighlightLandedSlice(sliceIndex, true, Color.white);
        }

        public void ClearImpactHighlight()
        {
            WheelSliceView sliceView = WheelSliceArrayLookup.Get(_sliceViews, _highlightedSliceIndex);
            _highlightedSliceIndex = -1;
            if (sliceView == null) return;

            sliceView.ClearImpactVisual();
        }

        private void SetAllRewardVisualsVisible(bool isVisible)
        {
            if (_sliceViews == null) return;

            for (int i = 0; i < _sliceViews.Length; i++)
            {
                WheelSliceView sliceView = _sliceViews[i];
                if (sliceView == null || !sliceView.gameObject.activeInHierarchy) continue;

                sliceView.SetRewardVisualVisible(isVisible);
            }
        }
    }
}
