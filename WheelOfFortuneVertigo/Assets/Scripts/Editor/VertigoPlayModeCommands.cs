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
    public static class VertigoPlayModeCommands
    {
        private static Coroutine _endWithRewardsRoutine;

        [MenuItem("Vertigo Case/Play Commands/Spin", true)]
        [MenuItem("Vertigo Case/Play Commands/Retry", true)]
        [MenuItem("Vertigo Case/Play Commands/Exit", true)]
        [MenuItem("Vertigo Case/Play Commands/Force Reward Popup", true)]
        [MenuItem("Vertigo Case/Play Commands/Force Bomb Fail", true)]
        [MenuItem("Vertigo Case/Play Commands/Add 15 Rewards", true)]
        [MenuItem("Vertigo Case/Play Commands/End With 15 Rewards", true)]
        [MenuItem("Vertigo Case/Play Commands/Capture Start Frame", true)]
        private static bool ValidatePlayCommand()
        {
            return Application.isPlaying;
        }

        [MenuItem("Vertigo Case/Play Commands/Spin")]
        private static void Spin()
        {
            Runtime().RequestSpin();
        }

        [MenuItem("Vertigo Case/Play Commands/Retry")]
        private static void Retry()
        {
            Runtime().RequestRestart();
        }

        [MenuItem("Vertigo Case/Play Commands/Exit")]
        private static void Exit()
        {
            Runtime().RequestLeave();
        }

        [MenuItem("Vertigo Case/Play Commands/Force Reward Popup")]
        private static void ForceRewardPopup()
        {
            WheelGameState state = WheelRuntimeLocator.State;
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            WheelGameSettings settings = WheelRuntimeLocator.Settings;
            WheelSpinner spinner = WheelRuntimeLocator.Spinner;
            if (state == null || publisher == null || settings == null || spinner == null)
            {
                return;
            }

            state.Restart();
            state.PrepareCurrentZone();
            publisher.PublishAll();

            int rewardIndex = FindRewardSliceIndex(state);
            if (rewardIndex < 0)
            {
                return;
            }

            spinner.SetSlices(state.Slices, state.SliceCount);
            spinner.SnapToSelectionForDebug(rewardIndex);
            var result = new WheelSpinResult(rewardIndex, state.Slices[rewardIndex]);
            state.Resolve(result);
            publisher.PublishZone();
            publisher.PublishOutcome(result, true);
            publisher.PublishHud();
            Directory.CreateDirectory("Temp/DebugScreenshots");
            publisher.StartCoroutine(CaptureScreenshotAfterLayout(
                "Temp/DebugScreenshots/reward_popup_runtime.png",
                true));
        }

        [MenuItem("Vertigo Case/Play Commands/Force Bomb Fail")]
        private static void ForceBombFail()
        {
            WheelGameState state = WheelRuntimeLocator.State;
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            WheelGameSettings settings = WheelRuntimeLocator.Settings;
            WheelSpinner spinner = WheelRuntimeLocator.Spinner;
            if (state == null || publisher == null || settings == null || spinner == null)
            {
                return;
            }

            state.Restart();
            AddDebugRewards(state, settings, 3);
            state.PrepareCurrentZone();
            publisher.PublishAll();

            int bombIndex = FindBombSliceIndex(state);
            if (bombIndex < 0)
            {
                return;
            }

            spinner.SetSlices(state.Slices, state.SliceCount);
            spinner.SnapToSelectionForDebug(bombIndex);
            var result = new WheelSpinResult(bombIndex, state.Slices[bombIndex]);
            state.Resolve(result);
            publisher.PublishZone();
            publisher.PublishOutcome(result, true);
            publisher.PublishHud();
            publisher.StartCoroutine(CaptureScreenshotAfterLayout(
                "/Users/bengisucay/ComputerUse/vertigo-case/screenshots/vertigo_bomb_fail_frame.png",
                false));
        }

        [MenuItem("Vertigo Case/Play Commands/Add 15 Rewards")]
        private static void AddFifteenRewards()
        {
            BuildFifteenRewardEndState(false);
        }

        [MenuItem("Vertigo Case/Play Commands/End With 15 Rewards")]
        private static void EndWithFifteenRewards()
        {
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            if (publisher == null)
            {
                return;
            }

            if (_endWithRewardsRoutine != null)
            {
                publisher.StopCoroutine(_endWithRewardsRoutine);
            }

            _endWithRewardsRoutine = publisher.StartCoroutine(BuildFifteenRewardEndStateAfterReset());
        }

        [MenuItem("Vertigo Case/Play Commands/Capture Start Frame")]
        private static void CaptureStartFrame()
        {
            WheelGameState state = WheelRuntimeLocator.State;
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            if (state == null || publisher == null)
            {
                return;
            }

            state.Restart();
            publisher.PublishAll();
            publisher.StartCoroutine(CaptureScreenshotAfterLayout(
                "/Users/bengisucay/ComputerUse/vertigo-case/screenshots/vertigo_first_play_frame.png",
                false));
        }

        private static IEnumerator BuildFifteenRewardEndStateAfterReset()
        {
            WheelGameState state = WheelRuntimeLocator.State;
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            if (state == null || publisher == null)
            {
                _endWithRewardsRoutine = null;
                yield break;
            }

            state.Restart();
            publisher.PublishAll();
            yield return null;
            yield return new WaitForEndOfFrame();

            BuildFifteenRewardEndState(true);
            _endWithRewardsRoutine = null;
        }

        private static void BuildFifteenRewardEndState(bool cashOut)
        {
            WheelGameState state = WheelRuntimeLocator.State;
            WheelStatePublisher publisher = WheelRuntimeLocator.Publisher;
            WheelGameSettings settings = WheelRuntimeLocator.Settings;
            if (state == null || publisher == null || settings == null)
            {
                return;
            }

            const int targetCount = 15;
            state.Inventory.Clear();
            int added = AddDebugRewards(state, settings, targetCount);
            if (cashOut)
            {
                state.CashOut();
                publisher.PublishOutcome(default(WheelSpinResult), false);
            }

            publisher.PublishHud();
            Directory.CreateDirectory("Temp/DebugScreenshots");
            string screenshotPath = cashOut
                ? "Temp/DebugScreenshots/reward_opening_15_runtime.png"
                : "Temp/DebugScreenshots/reward_panel_15_runtime.png";
            publisher.StartCoroutine(CaptureScreenshotAfterLayout(screenshotPath, cashOut));
            Debug.Log("Built " + added + " debug rewards for " + (cashOut ? "the cashed-out reward opening." : "the reward panel."));
        }

        private static IEnumerator CaptureScreenshotAfterLayout(string screenshotPath, bool captureRevealStep)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitForSecondsRealtime(0.55f);
            yield return new WaitForEndOfFrame();
            if (captureRevealStep)
            {
                string revealPath = Path.Combine(
                    Path.GetDirectoryName(screenshotPath),
                    Path.GetFileNameWithoutExtension(screenshotPath) + "_reveal.png");
                WriteScreenPng(revealPath);
                yield return new WaitForSecondsRealtime(2.45f);
                yield return new WaitForEndOfFrame();
            }

            WriteScreenPng(screenshotPath);
        }

        private static void WriteScreenPng(string screenshotPath)
        {
            var texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0f, 0f, Screen.width, Screen.height), 0, 0);
            texture.Apply();
            File.WriteAllBytes(screenshotPath, texture.EncodeToPNG());
            Object.Destroy(texture);
            Debug.Log("Wrote debug screenshot: " + screenshotPath);
        }

        private static int AddDebugRewards(WheelGameState state, WheelGameSettings settings, int targetCount)
        {
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
                    source.Id + "_debug_" + i,
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
                state.Inventory.Add(new WheelSpinResult(added, slice));
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
                if (IsReferenceLikeReward(state.Slices[i]))
                {
                    return i;
                }
            }

            for (int i = 0; i < state.SliceCount; i++)
            {
                if (!state.Slices[i].IsBomb)
                {
                    return i;
                }
            }

            return -1;
        }

        private static bool IsReferenceLikeReward(WheelSliceDefinition slice)
        {
            if (slice == null || slice.IsBomb || slice.Reward == null)
            {
                return false;
            }

            string displayName = slice.Reward.DisplayName;
            return !string.IsNullOrEmpty(displayName)
                && displayName.IndexOf("Points", System.StringComparison.OrdinalIgnoreCase) < 0
                && displayName.IndexOf("Chest", System.StringComparison.OrdinalIgnoreCase) < 0
                && displayName.IndexOf("Cash", System.StringComparison.OrdinalIgnoreCase) < 0;
        }

        private static WheelRuntimeCompositionRoot Runtime()
        {
            return Object.FindObjectOfType<WheelRuntimeCompositionRoot>();
        }
    }
}
#endif
