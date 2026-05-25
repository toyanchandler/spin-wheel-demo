using System;
using DG.Tweening;
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
        private int _suppressedRewardVisualSliceIndex = -1;
        private int _highlightedSliceIndex = -1;
        private bool _areAllRewardVisualsSuppressed;

        public void Bind(WheelEventBus eventBus)
        {
            RequireSliceViews();
            WheelRuntimeLocator.RegisterWheelView(this);
            _eventBus = eventBus;
            _eventBus.ZoneChanged += OnZoneChanged;
            _eventBus.SpinLanded += OnSpinLanded;
            _eventBus.OutcomeResolved += OnOutcomeResolved;
        }

        public void Unbind()
        {
            if (WheelRuntimeLocator.WheelView == this)
            {
                WheelRuntimeLocator.RegisterWheelView(null);
            }

            _eventBus.ZoneChanged -= OnZoneChanged;
            _eventBus.SpinLanded -= OnSpinLanded;
            _eventBus.OutcomeResolved -= OnOutcomeResolved;
            _eventBus = null;
        }

        public void ApplyUprightSlicePresentations()
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

                sliceView.ApplyUprightPresentation();
            }
        }

        public bool TryResolveSliceIconAnchoredPosition(int sliceIndex, RectTransform targetParent, out Vector2 anchoredPosition)
        {
            anchoredPosition = default(Vector2);
            return TryResolveSliceIconPresentation(sliceIndex, targetParent, out anchoredPosition, out Sprite _, out Color _);
        }

        public bool TryCopySlicePointerAngles(int sliceCount, float[] pointerAngles)
        {
            if (sliceCount <= 0 || pointerAngles == null || pointerAngles.Length < sliceCount || _sliceViews == null || _sliceViews.Length < sliceCount)
            {
                return false;
            }

            for (int i = 0; i < sliceCount; i++)
            {
                WheelSliceView sliceView = _sliceViews[i];
                if (sliceView == null)
                {
                    return false;
                }

                pointerAngles[i] = NormalizeAngle(sliceView.PointerAngle);
            }

            return true;
        }

        public bool TryResolveSliceIconPresentation(
            int sliceIndex,
            RectTransform targetParent,
            out Vector2 anchoredPosition,
            out Sprite sprite,
            out Color color)
        {
            anchoredPosition = default(Vector2);
            sprite = null;
            color = Color.white;
            if (sliceIndex < 0 || sliceIndex >= _sliceViews.Length)
            {
                return false;
            }

            WheelSliceView sliceView = _sliceViews[sliceIndex];
            if (sliceView == null || !sliceView.gameObject.activeInHierarchy)
            {
                return false;
            }

            RectTransform iconRect = sliceView.IconRect;
            sprite = sliceView.IconSprite;
            color = sliceView.IconColor;
            if (iconRect == null || sprite == null)
            {
                return false;
            }

            if (targetParent == null)
            {
                return true;
            }

            Canvas canvas = targetParent.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return false;
            }

            Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camera, iconRect.position);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(targetParent, screenPoint, camera, out anchoredPosition);
        }

        public void SuppressSliceRewardVisual(int sliceIndex)
        {
            RestoreSuppressedSliceRewardVisual();
            if (sliceIndex < 0 || sliceIndex >= _sliceViews.Length)
            {
                return;
            }

            WheelSliceView sliceView = _sliceViews[sliceIndex];
            if (sliceView == null || !sliceView.gameObject.activeInHierarchy)
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
            if (_suppressedRewardVisualSliceIndex < 0 || _suppressedRewardVisualSliceIndex >= _sliceViews.Length)
            {
                _suppressedRewardVisualSliceIndex = -1;
                return;
            }

            WheelSliceView sliceView = _sliceViews[_suppressedRewardVisualSliceIndex];
            if (sliceView != null && sliceView.gameObject.activeInHierarchy)
            {
                sliceView.SetRewardVisualVisible(true);
            }

            _suppressedRewardVisualSliceIndex = -1;
        }

        private void OnZoneChanged(WheelZoneSnapshot snapshot)
        {
            ClearBombHitHighlight();
            SlicePoolRenderer.Render(
                _sliceViews,
                snapshot.Slices,
                snapshot.SliceCount);
            ApplySuppressedSliceRewardVisual();
        }

        private void OnOutcomeResolved(WheelOutcomeSnapshot snapshot)
        {
            if (snapshot.Phase == WheelGamePhase.Bombed)
            {
                EnsureBombSliceHighlight(snapshot.SourceSliceIndex);
                return;
            }
        }

        private void OnSpinLanded(WheelSpinResult result)
        {
            HighlightLandedSlice(result.SliceIndex, result.IsBomb, result.AccentColor);
        }

        private void HighlightLandedSlice(int sliceIndex, bool isBomb, Color accentColor)
        {
            ClearBombHitHighlight();
            if (sliceIndex < 0 || sliceIndex >= _sliceViews.Length)
            {
                return;
            }

            WheelSliceView sliceView = _sliceViews[sliceIndex];
            if (sliceView == null || !sliceView.gameObject.activeInHierarchy)
            {
                return;
            }

            _highlightedSliceIndex = sliceIndex;
            if (isBomb)
            {
                sliceView.PlayBombHitPulse();
                PlayWheelBombShake();
                return;
            }

            sliceView.PlayRewardHitPulse(accentColor);
        }

        private void EnsureBombSliceHighlight(int sliceIndex)
        {
            if (_highlightedSliceIndex == sliceIndex)
            {
                return;
            }

            HighlightLandedSlice(sliceIndex, true, Color.white);
        }

        private void ClearBombHitHighlight()
        {
            if (_highlightedSliceIndex < 0 || _highlightedSliceIndex >= _sliceViews.Length)
            {
                _highlightedSliceIndex = -1;
                return;
            }

            WheelSliceView sliceView = _sliceViews[_highlightedSliceIndex];
            if (sliceView != null)
            {
                sliceView.ClearImpactVisual();
            }

            _highlightedSliceIndex = -1;
        }

        private void PlayWheelBombShake()
        {
            WheelSpinner spinner = WheelRuntimeLocator.Spinner;
            if (spinner == null || spinner.WheelTransform == null)
            {
                return;
            }

            spinner.WheelTransform.DOKill();
            spinner.WheelTransform
                .DOPunchRotation(new Vector3(0f, 0f, 5f), 0.20f, 8, 0.45f)
                .SetTarget(spinner.WheelTransform)
                .SetUpdate(true)
                .SetLink(spinner.gameObject, LinkBehaviour.KillOnDisable);
        }

        private void ApplySuppressedSliceRewardVisual()
        {
            if (_areAllRewardVisualsSuppressed)
            {
                SetAllRewardVisualsVisible(false);
                return;
            }

            if (_suppressedRewardVisualSliceIndex < 0 || _suppressedRewardVisualSliceIndex >= _sliceViews.Length)
            {
                return;
            }

            WheelSliceView sliceView = _sliceViews[_suppressedRewardVisualSliceIndex];
            if (sliceView != null && sliceView.gameObject.activeInHierarchy)
            {
                sliceView.SetRewardVisualVisible(false);
            }
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

        private void RequireSliceViews()
        {
            if (_sliceViews == null || _sliceViews.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no slice views. Collect children in the inspector or rebuild the scene.");
            }
        }

        private static float NormalizeAngle(float angle)
        {
            angle %= 360f;
            return angle < 0f ? angle + 360f : angle;
        }

        private static class SlicePoolRenderer
        {
            public static void Render(
                WheelSliceView[] pool,
                WheelSliceDefinition[] slices,
                int sliceCount)
            {
                if (sliceCount > pool.Length)
                {
                    throw new InvalidOperationException("Wheel slice pool is smaller than configured slice count. Rebuild the scene hierarchy.");
                }

                int visibleCount = Mathf.Min(sliceCount, pool.Length);
                for (int i = 0; i < visibleCount; i++)
                {
                    WheelSlicePresentation presentation = new WheelSlicePresentation(
                        slices[i].DisplayIcon,
                        slices[i].DisplayAmount,
                        slices[i].DisplayColor,
                        slices[i].ShowAmountLabel,
                        slices[i].DisplayLabel);
                    pool[i].Apply(presentation);
                }

                for (int i = visibleCount; i < pool.Length; i++)
                {
                    pool[i].Hide();
                }
            }
        }
    }
}
