using NUnit.Framework;
using UnityEngine;
using Vertigo.Wheel.Runtime;
using Vertigo.Wheel.Views;

namespace Vertigo.Wheel.Tests.Editor
{
    public sealed class WheelSlicePresentationResolverTests
    {
        [Test]
        public void CopyPointerAngles_RejectsNullBuffer()
        {
            var resolver = new WheelSlicePresentationResolver(new WheelSliceView[1]);

            WheelSlicePointerAnglesCopy result = resolver.CopyPointerAngles(1, null);

            Assert.IsFalse(result.Succeeded);
        }

        [Test]
        public void CopyPointerAngles_RejectsUndersizedBuffer()
        {
            var resolver = new WheelSlicePresentationResolver(new WheelSliceView[2]);

            WheelSlicePointerAnglesCopy result = resolver.CopyPointerAngles(2, new float[1]);

            Assert.IsFalse(result.Succeeded);
        }

        [Test]
        public void ResolveIconPresentation_ReturnsInvalidForMissingSlice()
        {
            var resolver = new WheelSlicePresentationResolver(System.Array.Empty<WheelSliceView>());

            WheelSliceIconPresentation presentation = resolver.ResolveIconPresentation(0, null);

            Assert.IsFalse(presentation.IsValid);
        }

        [Test]
        public void WheelSliceArrayLookup_GetActive_RequiresActiveHierarchy()
        {
            var host = new GameObject("slice");
            host.SetActive(false);
            var sliceView = host.AddComponent<WheelSliceView>();

            WheelSliceView resolved = WheelSliceArrayLookup.GetActive(new[] { sliceView }, 0);

            Assert.IsNull(resolved);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void WheelAngleUtility_NormalizeUnsignedAngle_WrapsNegative()
        {
            Assert.AreEqual(350f, WheelAngleUtility.NormalizeUnsignedAngle(-10f), 0.001f);
        }
    }
}
