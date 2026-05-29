#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    public static class VertigoCaseVideoCaptureRunner
    {
        private const int CaptureFrameRate = 30;
        private const string FrameRoot = "Temp/CaseVideoFrames";
        private const string DemoFrames = FrameRoot + "/SpinBomb";
        private const string RewardFrames = FrameRoot + "/RewardCards";
        private const string DoneFile = FrameRoot + "/capture_done.txt";

        private static Coroutine _activeRoutine;

        [MenuItem("Vertigo Case/Video Capture/Record All Case Videos", true)]
        private static bool ValidateRecordAll() => Application.isPlaying;

        [MenuItem("Vertigo Case/Video Capture/Record All Case Videos")]
        private static void RecordAll()
        {
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            if (!WheelRuntimeLocator.IsReady || publisher == null)
            {
                Debug.LogWarning("Video capture requires Play Mode with wheel runtime ready.");
                return;
            }

            if (_activeRoutine != null)
            {
                publisher.StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }

            _activeRoutine = publisher.StartCoroutine(RecordAllRoutine());
        }

        private static IEnumerator RecordAllRoutine()
        {
            File.Delete(DoneFile);
            ResetDirectory(DemoFrames);
            ResetDirectory(RewardFrames);

            int previousCaptureFrameRate = Time.captureFramerate;
            Time.captureFramerate = CaptureFrameRate;

            try
            {
                yield return RecordSpinBombDemo();
                yield return RecordRewardCards();
                Directory.CreateDirectory(FrameRoot);
                File.WriteAllText(DoneFile, "done");
                Debug.Log("Case video frame capture completed. Frames: " + FrameRoot);
            }
            finally
            {
                Time.captureFramerate = previousCaptureFrameRate;
                _activeRoutine = null;
            }
        }

        private static IEnumerator RecordSpinBombDemo()
        {
            if (!WheelRuntimeEditorSession.TryGet(out WheelGameplaySession session))
            {
                yield break;
            }

            WheelGameState state = session.State;
            WheelStatePublisher publisher = session.Publisher;
            WheelSpinner spinner = session.Spinner as WheelSpinner;
            if (spinner == null)
            {
                yield break;
            }

            var capture = new FrameCapture(DemoFrames);
            state.Restart();
            publisher.PublishAll();
            yield return CaptureSeconds(capture, 0.75f);

            for (int i = 0; i < 3; i++)
            {
                BeginDirectedSpin(state, publisher, spinner, false);
                yield return WaitForSpinToResolve(capture, state, spinner);
                yield return CaptureSeconds(capture, 0.7f);
            }

            BeginDirectedSpin(state, publisher, spinner, true);
            yield return WaitForSpinToResolve(capture, state, spinner);
            yield return CaptureSeconds(capture, 2.2f);
        }

        private static IEnumerator RecordRewardCards()
        {
            if (!WheelRuntimeEditorSession.TryGet(out WheelGameplaySession session))
            {
                yield break;
            }

            WheelGameState state = session.State;
            WheelStatePublisher publisher = session.Publisher;
            state.Restart();
            WheelEditorDebugRewards.FillInventory(state, session.Settings, 15, "video");
            state.CashOut();
            publisher.PublishHud();
            publisher.PublishOutcome(default(WheelSpinResult), false);

            yield return CaptureSeconds(new FrameCapture(RewardFrames), 6.0f);
        }

        private static void BeginDirectedSpin(
            WheelGameState state,
            WheelStatePublisher publisher,
            WheelSpinner spinner,
            bool bomb)
        {
            state.PrepareCurrentZone();
            publisher.PublishZone();
            state.BeginSpin();
            publisher.PublishHud();
            WheelSliceDefinition[] slices = state.CreateSliceSnapshot();
            spinner.AcceptSlices(slices, slices.Length);

            int sliceIndex = bomb ? state.FindFirstSliceIndex(true) : state.FindFirstSliceIndex(false);
            if (sliceIndex < 0) sliceIndex = 0;

            spinner.Spin(sliceIndex);
        }

        private static IEnumerator WaitForSpinToResolve(FrameCapture capture, WheelGameState state, WheelSpinner spinner)
        {
            yield return null;
            while (spinner != null && spinner.IsSpinning)
            {
                yield return CaptureFrame(capture);
            }

            float timeoutAt = Time.unscaledTime + 1.25f;
            while (state != null && state.Phase == WheelGamePhase.Spinning && Time.unscaledTime < timeoutAt)
            {
                yield return CaptureFrame(capture);
            }

            yield return CaptureSeconds(capture, 0.45f);
        }

        private static IEnumerator CaptureSeconds(FrameCapture capture, float seconds)
        {
            int framesToCapture = Mathf.CeilToInt(seconds * CaptureFrameRate);
            for (int i = 0; i < framesToCapture; i++)
            {
                yield return CaptureFrame(capture);
            }
        }

        private static IEnumerator CaptureFrame(FrameCapture capture)
        {
            yield return new WaitForEndOfFrame();
            string path = Path.Combine(capture.Directory, "frame_" + capture.Frame.ToString("0000") + ".png");
            capture.Frame++;
            ScreenCapture.CaptureScreenshot(path);
        }

        private static void ResetDirectory(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);

            Directory.CreateDirectory(path);
        }

        private sealed class FrameCapture
        {
            public readonly string Directory;
            public int Frame;

            public FrameCapture(string directory)
            {
                Directory = directory;
                Frame = 0;
            }
        }
    }
}
#endif
