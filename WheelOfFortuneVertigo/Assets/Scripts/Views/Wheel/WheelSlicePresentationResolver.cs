using UnityEngine;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    internal sealed class WheelSlicePresentationResolver
    {
        private readonly WheelSliceView[] _sliceViews;

        public WheelSlicePresentationResolver(WheelSliceView[] sliceViews)
        {
            _sliceViews = sliceViews;
        }

        public WheelSlicePointerAnglesCopy CopyPointerAngles(int sliceCount, float[] pointerAngles)
        {
            if (!CanCopyPointerAngles(sliceCount, pointerAngles)) return WheelSlicePointerAnglesCopy.Failed;

            for (int i = 0; i < sliceCount; i++)
            {
                WheelSliceView sliceView = _sliceViews[i];
                if (sliceView == null) return WheelSlicePointerAnglesCopy.Failed;

                pointerAngles[i] = WheelAngleUtility.NormalizeUnsignedAngle(sliceView.PointerAngle);
            }

            return WheelSlicePointerAnglesCopy.Ok;
        }

        public WheelSliceIconPresentation ResolveIconPresentation(int sliceIndex, RectTransform targetParent)
        {
            WheelSliceView sliceView = ResolveIconSlice(sliceIndex);
            if (sliceView == null) return WheelSliceIconPresentation.Invalid;

            Sprite sprite = sliceView.IconSprite;
            Color color = sliceView.IconColor;
            RectTransform iconRect = sliceView.IconRect;
            if (iconRect == null || sprite == null) return WheelSliceIconPresentation.Invalid;

            if (targetParent == null)
            {
                return new WheelSliceIconPresentation(sprite, color, false, default);
            }

            if (!WheelUiRectProjection.TryProjectToAnchoredPosition(iconRect, targetParent, out Vector2 anchoredPosition))
            {
                return WheelSliceIconPresentation.Invalid;
            }

            return new WheelSliceIconPresentation(sprite, color, true, anchoredPosition);
        }

        private bool CanCopyPointerAngles(int sliceCount, float[] pointerAngles)
        {
            return sliceCount > 0
                && pointerAngles != null
                && pointerAngles.Length >= sliceCount
                && _sliceViews != null
                && _sliceViews.Length >= sliceCount;
        }

        private WheelSliceView ResolveIconSlice(int sliceIndex)
        {
            WheelSliceView sliceView = WheelSliceArrayLookup.Get(_sliceViews, sliceIndex);
            if (sliceView == null || !sliceView.gameObject.activeInHierarchy) return null;

            return sliceView;
        }
    }
}
