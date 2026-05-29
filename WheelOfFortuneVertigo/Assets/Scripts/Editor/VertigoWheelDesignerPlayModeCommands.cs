using UnityEngine;
using Vertigo.Wheel.Data;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.EditorTools
{
    /// <summary>Play-mode debug commands invoked from <see cref="VertigoWheelDesignerWindow"/>.</summary>
    internal static class VertigoWheelDesignerPlayModeCommands
    {
        public static bool CanRun => WheelRuntimeEditorSession.IsReady;

        public static void AddFifteenLootToRewardPanel()
        {
            if (!TryGetSession(out WheelGameplaySession session))
            {
                return;
            }

            if (RejectWhileSpinning(session.Spinner))
            {
                return;
            }

            session.State.Restart();
            int added = WheelEditorDebugRewards.FillInventory(session.State, session.Settings, 15);
            session.Publisher.PublishAll();
            Debug.Log("Added " + added + " debug loot entries to the reward panel.");
        }

        public static void OpenFifteenRewardCards()
        {
            if (!TryGetSession(out WheelGameplaySession session))
            {
                return;
            }

            if (RejectWhileSpinning(session.Spinner))
            {
                return;
            }

            session.State.Restart();
            int added = WheelEditorDebugRewards.FillInventory(session.State, session.Settings, 15);
            session.State.CashOut();
            session.Publisher.PublishHud();
            session.Publisher.PublishOutcome(default(WheelSpinResult), false);
            Debug.Log("Opened reward-card reveal with " + added + " debug loot entries.");
        }

        public static void ForceBombOutcome()
        {
            ForceOutcome(bomb: true);
        }

        public static void ForceRandomProductOutcome()
        {
            ForceOutcome(bomb: false);
        }

        private static void ForceOutcome(bool bomb)
        {
            if (!TryGetSession(out WheelGameplaySession session))
            {
                return;
            }

            if (!TryGetSpinner(session, out WheelSpinner spinner) || RejectWhileSpinning(session.Spinner))
            {
                return;
            }

            PrepareDebugZone(session);
            if (!TryResolveSliceIndex(session.State, bomb, out int sliceIndex))
            {
                return;
            }

            CompleteForcedSpin(session, spinner, sliceIndex);
            Debug.Log("Forced " + (bomb ? "bomb" : "random product") + " outcome on slice " + sliceIndex + ".");
        }

        private static bool TryGetSession(out WheelGameplaySession session)
        {
            if (WheelRuntimeEditorSession.TryGet(out session))
            {
                return true;
            }

            LogRuntimeNotReady();
            return false;
        }

        private static bool TryGetSpinner(WheelGameplaySession session, out WheelSpinner spinner)
        {
            spinner = session.Spinner as WheelSpinner;
            if (spinner != null)
            {
                return true;
            }

            LogRuntimeNotReady();
            return false;
        }

        private static bool RejectWhileSpinning(IWheelSpinDriver spinner)
        {
            if (!spinner.IsSpinning)
            {
                return false;
            }

            Debug.LogWarning("Cannot run debug command while the wheel is spinning.");
            return true;
        }

        private static void PrepareDebugZone(WheelGameplaySession session)
        {
            session.State.Restart();
            session.State.PrepareCurrentZone();
            session.Publisher.PublishAll();
        }

        private static bool TryResolveSliceIndex(WheelGameState state, bool bomb, out int sliceIndex)
        {
            sliceIndex = bomb
                ? state.FindFirstSliceIndex(true)
                : state.SelectRandomRewardSliceIndex();
            if (sliceIndex >= 0)
            {
                return true;
            }

            Debug.LogWarning(
                bomb
                    ? "No bomb slice is available in the current debug zone."
                    : "No reward slice is available in the current debug zone.");
            return false;
        }

        private static void CompleteForcedSpin(
            WheelGameplaySession session,
            WheelSpinner spinner,
            int sliceIndex)
        {
            WheelSliceDefinition[] slices = session.State.CreateSliceSnapshot();
            spinner.AcceptSlices(slices, slices.Length);
            spinner.SnapToSelectionForDebug(sliceIndex);
            WheelSpinResult result = session.State.CreateSpinResult(sliceIndex);
            session.Flow.ForceResolveOutcome(result);
            session.Publisher.PublishZone();
        }

        private static void LogRuntimeNotReady()
        {
            Debug.LogWarning("Wheel runtime is not ready for play-mode commands.");
        }
    }
}
