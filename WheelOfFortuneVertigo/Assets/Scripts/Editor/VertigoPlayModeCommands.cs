#if UNITY_EDITOR
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
        [MenuItem("Vertigo Case/Play Commands/Spin", true)]
        [MenuItem("Vertigo Case/Play Commands/Retry", true)]
        [MenuItem("Vertigo Case/Play Commands/Exit", true)]
        [MenuItem("Vertigo Case/Play Commands/Add 15 Rewards", true)]
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

        [MenuItem("Vertigo Case/Play Commands/Add 15 Rewards")]
        private static void AddFifteenRewards()
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
            int added = 0;
            var usedRewardIds = new HashSet<string>();
            added = AddRewardsFromPool(state, settings.StandardRewards, usedRewardIds, added, targetCount);
            added = AddRewardsFromPool(state, settings.SafeRewards, usedRewardIds, added, targetCount);
            added = AddRewardsFromPool(state, settings.SuperRewards, usedRewardIds, added, targetCount);
            publisher.PublishHud();
            Directory.CreateDirectory("Temp/DebugScreenshots");
            ScreenCapture.CaptureScreenshot("Temp/DebugScreenshots/reward_panel_15_runtime.png");
            Debug.Log("Added " + added + " debug rewards to the reward panel.");
        }

        private static int AddRewardsFromPool(
            WheelGameState state,
            List<RewardDefinition> rewards,
            HashSet<string> usedRewardIds,
            int added,
            int targetCount)
        {
            if (rewards == null)
            {
                return added;
            }

            for (int i = 0; i < rewards.Count && added < targetCount; i++)
            {
                RewardDefinition reward = rewards[i];
                if (reward == null || string.IsNullOrEmpty(reward.Id) || usedRewardIds.Contains(reward.Id))
                {
                    continue;
                }

                usedRewardIds.Add(reward.Id);
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

        private static WheelRuntimeCompositionRoot Runtime()
        {
            return Object.FindObjectOfType<WheelRuntimeCompositionRoot>();
        }
    }
}
#endif
