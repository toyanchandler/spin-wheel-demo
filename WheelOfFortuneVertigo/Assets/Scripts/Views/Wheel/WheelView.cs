using System;
using UnityEngine;
using Vertigo.Collections;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelView : MonoBehaviour, IWheelSliceLayoutPresenter
    {
        [SerializeField] private Transform _slicePoolRoot;

        [CollectChildren(nameof(_slicePoolRoot))]
        [SerializeField] private WheelSliceView[] _sliceViews = Array.Empty<WheelSliceView>();

        [WheelInject] private WheelEventBus _eventBus;
        private WheelSlicePresentationResolver _presentationResolver;
        private WheelSliceVisualState _visualState;
        private WheelViewImpactAnimator _impactAnimator;

        [WheelAfterInject]
        private void Connect()
        {
            RequireSliceViews();
            _presentationResolver = new WheelSlicePresentationResolver(_sliceViews);
            _impactAnimator = new WheelViewImpactAnimator(_eventBus.Presentation.Spin, this);
            _visualState = new WheelSliceVisualState(_sliceViews, _impactAnimator.PlayBombShake);
            _eventBus.Presentation.Spin.RegisterSliceLayout(this);
            _eventBus.ZoneChanged += OnZoneChanged;
            _eventBus.SpinLanded += OnSpinLanded;
            _eventBus.OutcomeResolved += OnOutcomeResolved;
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            _eventBus.Presentation.Spin.RegisterSliceLayout(null);
            _eventBus.ZoneChanged -= OnZoneChanged;
            _eventBus.SpinLanded -= OnSpinLanded;
            _eventBus.OutcomeResolved -= OnOutcomeResolved;
        }

        public void ApplyUprightSlicePresentations(float wheelLocalEulerZ)
        {
            if (_sliceViews == null) return;
            for (int i = 0; i < _sliceViews.Length; i++)
            {
                WheelSliceView sliceView = _sliceViews[i];
                if (sliceView == null || !sliceView.gameObject.activeInHierarchy) continue;

                sliceView.ApplyCounterRotationForWheelRoot(wheelLocalEulerZ);
            }
        }

        public WheelSlicePointerAnglesCopy CopySlicePointerAngles(int sliceCount, float[] pointerAngles)
        {
            return _presentationResolver.CopyPointerAngles(sliceCount, pointerAngles);
        }

        public void SuppressSliceRewardVisual(int sliceIndex)
        {
            _visualState.SuppressSliceRewardVisual(sliceIndex);
        }

        public void SuppressAllRewardVisuals()
        {
            _visualState.SuppressAllRewardVisuals();
        }

        public void RestoreAllRewardVisuals()
        {
            _visualState.RestoreAllRewardVisuals();
        }

        public void RestoreSuppressedSliceRewardVisual()
        {
            _visualState.RestoreSuppressedSliceRewardVisual();
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            _visualState.ClearImpactHighlight();
            WheelSlicePoolRenderer.Render(
                _sliceViews,
                snapshot.SlicePresentations,
                snapshot.SliceCount);
            _visualState.ApplySuppressedSliceRewardVisual();
        }

        private void OnOutcomeResolved(WheelOutcomeSnapshot snapshot)
        {
            if (snapshot.Phase == WheelGamePhase.Bombed)
            {
                _visualState.EnsureBombSliceHighlight(snapshot.SourceSliceIndex);
            }
        }

        private void OnSpinLanded(WheelSpinResult result)
        {
            _visualState.HighlightLandedSlice(result.SliceIndex, result.IsBomb, result.AccentColor);
        }

        private void RequireSliceViews()
        {
            if (_sliceViews == null || _sliceViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no slice views. Collect children in the inspector or rebuild the scene.");
            }
        }
    }
}
