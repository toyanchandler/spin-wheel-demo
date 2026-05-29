#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.EditorTools;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Tests
{
    public sealed class WheelPlayModeSmokeTests
    {
        private const float SpinTimeoutSeconds = 20f;

        [UnityTest]
        public IEnumerator MainScene_RuntimeBoots_AndCompletesOneSpin()
        {
            EditorSceneManager.OpenScene(VertigoWheelPaths.ScenePath, OpenSceneMode.Single);

            yield return new EnterPlayMode();

            float waitForReady = 5f;
            while (!WheelRuntimeLocator.IsReady && waitForReady > 0f)
            {
                waitForReady -= Time.deltaTime;
                yield return null;
            }

            Assert.IsTrue(WheelRuntimeLocator.IsReady, "Wheel runtime did not become ready in MainScene.");
            Assert.IsNotNull(WheelRuntimeLocator.EventBus);
            Assert.IsNotNull(WheelRuntimeLocator.Flow);
            Assert.IsNotNull(WheelRuntimeLocator.Spinner);

            WheelGamePhase phaseBeforeSpin = WheelRuntimeLocator.State.Phase;
            Assert.AreEqual(WheelGamePhase.Ready, phaseBeforeSpin);

            WheelRuntimeLocator.EventBus.RequestSpin();

            float timeout = SpinTimeoutSeconds;
            while (WheelRuntimeLocator.Spinner.IsSpinning && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.IsFalse(WheelRuntimeLocator.Spinner.IsSpinning, "Spin did not complete before timeout.");
            Assert.AreNotEqual(WheelGamePhase.Ready, WheelRuntimeLocator.State.Phase, "Spin should resolve gameplay phase.");

            yield return new ExitPlayMode();
        }
    }
}
#endif
