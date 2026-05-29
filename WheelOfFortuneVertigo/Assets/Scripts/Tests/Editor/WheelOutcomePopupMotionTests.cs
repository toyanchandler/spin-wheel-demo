using NUnit.Framework;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelOutcomePopupMotionTests
    {
        [Test]
        public void Default_IncludesFlashPresentationValues()
        {
            WheelOutcomePopupMotion motion = WheelOutcomePopupMotion.Default();

            Assert.Greater(motion.BombFlashAlpha, 0f);
            Assert.Greater(motion.RewardFlashAlpha, 0f);
            Assert.Greater(motion.BombFlashColor.r, 0f);
            Assert.Greater(motion.RewardFlashColor.g, 0f);
        }

        [Test]
        public void ResolveFlash_UsesPhaseSpecificValues()
        {
            WheelOutcomePopupMotion motion = WheelOutcomePopupMotion.Default();

            Assert.AreEqual(motion.BombFlashColor, motion.ResolveFlashColor(WheelGamePhase.Bombed));
            Assert.AreEqual(motion.RewardFlashColor, motion.ResolveFlashColor(WheelGamePhase.Won));
            Assert.AreEqual(motion.BombFlashAlpha, motion.ResolveFlashAlpha(WheelGamePhase.Bombed));
            Assert.AreEqual(motion.RewardFlashAlpha, motion.ResolveFlashAlpha(WheelGamePhase.Won));
        }
    }
}
