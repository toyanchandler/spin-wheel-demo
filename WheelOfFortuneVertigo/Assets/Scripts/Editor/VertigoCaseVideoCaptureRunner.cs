#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
        private static MethodInfo _playSpinMethod;

        [MenuItem("Vertigo Case/Video Capture/Record All Case Videos", true)]
        private static bool ValidateRecordAll()
        {
            return Application.isPlaying;
        }

        [MenuItem("Vertigo Case/Video Capture/Record All Case Videos")]
        private static void RecordAll()
        {
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            if (publisher == null)
            {
                Debug.LogWarning("Video capture requires Play Mode with WheelRuntimeLocator ready.");
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
            WheelGameState state;
            WheelStatePublisher publisher;
            WheelGameSettings settings;
            WheelSpinner spinner;
            if (!TryGetRuntimeParts(out state, out publisher, out settings, out spinner))
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
            WheelGameState state;
            WheelStatePublisher publisher;
            WheelGameSettings settings;
            WheelSpinner spinner;
            if (!TryGetRuntimeParts(out state, out publisher, out settings, out spinner))
            {
                yield break;
            }

            state.Restart();
            AddDebugRewards(state, settings, 15);
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
            spinner.SetSlices(state.Slices, state.SliceCount);

            int sliceIndex = bomb ? FindBombSliceIndex(state) : FindRewardSliceIndex(state);
            if (sliceIndex < 0)
            {
                sliceIndex = 0;
            }

            PlaySpin(spinner, sliceIndex);
        }

        private static void PlaySpin(WheelSpinner spinner, int sliceIndex)
        {
            MethodInfo method = _playSpinMethod;
            if (method == null)
            {
                method = typeof(WheelSpinner).GetMethod("PlaySpin", BindingFlags.Instance | BindingFlags.NonPublic);
                _playSpinMethod = method;
            }

            if (method == null)
            {
                Debug.LogWarning("Could not resolve WheelSpinner.PlaySpin for directed video capture.");
                return;
            }

            method.Invoke(spinner, new object[] { sliceIndex });
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

        private static bool TryGetRuntimeParts(
            out WheelGameState state,
            out WheelStatePublisher publisher,
            out WheelGameSettings settings,
            out WheelSpinner spinner)
        {
            state = WheelRuntimeLocator.State;
            publisher = WheelRuntimeLocator.Publisher;
            settings = WheelRuntimeLocator.Settings;
            spinner = WheelRuntimeLocator.Spinner;
            if (state != null && publisher != null && settings != null && spinner != null)
            {
                return true;
            }

            Debug.LogWarning("Wheel runtime is not ready for video capture.");
            return false;
        }

        private static int FindBombSliceIndex(WheelGameState state)
        {
            for (int i = 0; i < state.SliceCount; i++)
            {
                if (state.Slices[i].IsBomb)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int FindRewardSliceIndex(WheelGameState state)
        {
            for (int i = 0; i < state.SliceCount; i++)
            {
                if (!state.Slices[i].IsBomb)
                {
                    return i;
                }
            }

            return -1;
        }

        private static int AddDebugRewards(WheelGameState state, WheelGameSettings settings, int targetCount)
        {
            state.Inventory.Clear();

            var rewardPool = new List<RewardDefinition>();
            AddRewardsToPool(rewardPool, settings.StandardRewards);
            AddRewardsToPool(rewardPool, settings.SafeRewards);
            AddRewardsToPool(rewardPool, settings.SuperRewards);
            if (rewardPool.Count == 0)
            {
                return 0;
            }

            int added = 0;
            for (int i = 0; i < targetCount; i++)
            {
                RewardDefinition source = rewardPool[i % rewardPool.Count];
                if (source == null || string.IsNullOrEmpty(source.Id))
                {
                    continue;
                }

                RewardDefinition reward = RewardDefinition.Create(
                    source.Id + "_video_" + i,
                    source.DisplayName,
                    source.Icon,
                    source.Amount,
                    source.Tier,
                    source.AccentColor,
                    source.WheelIcon);
                reward.CacheRuntimeText(settings.UiCopy.WinLabelFormat);

                var slice = new WheelSliceDefinition();
                slice.ApplySlot(
                    false,
                    reward,
                    reward.WheelIcon,
                    reward.Amount,
                    reward.AccentColor,
                    reward.Label,
                    true);

                state.Inventory.Add(new WheelSpinResult(added, slice), settings.UiCopy.InventoryFallbackRewardName);
                added++;
            }

            return added;
        }

        private static void AddRewardsToPool(List<RewardDefinition> target, List<RewardDefinition> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    target.Add(source[i]);
                }
            }
        }

        private static void ResetDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

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
