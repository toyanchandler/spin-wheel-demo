using System;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelGameState : MonoBehaviour
    {
        private readonly RewardInventory _inventory = new RewardInventory();
        private WheelGameSettings _settings;
        private WheelSliceDefinition[] _sliceBuffer;
        private WheelSpinResult _lastResult;
        private bool _hasLastResult;
        private int _sliceCount;
        private int _zone = 1;
        private WheelGamePhase _phase = WheelGamePhase.Ready;
        private bool _slicesDirty = true;

        public WheelGameSettings Settings { get { return _settings; } }
        public RewardInventory Inventory { get { return _inventory; } }
        public WheelSpinResult LastResult { get { return _lastResult; } }
        public bool HasLastResult { get { return _hasLastResult; } }
        public int SliceCount { get { return _sliceCount; } }
        public int Zone { get { return _zone; } }
        public WheelGamePhase Phase { get { return _phase; } }
        public ZoneType ZoneType { get { return Settings.GetZoneType(_zone); } }

        public WheelPhaseGameplayProfile PhaseGameplay { get { return Settings.UiCopy.GetPhaseCopy(_phase).Gameplay; } }

        public bool CanSpin { get { return Settings != null && PhaseGameplay.AllowSpin; } }

        public bool CanLeave
        {
            get
            {
                return Settings != null
                    && (Settings.GetZoneGameplay(ZoneType).AllowLeave || _inventory.Count > 0)
                    && PhaseGameplay.AllowLeave;
            }
        }

        public bool CanRestart { get { return Settings != null && PhaseGameplay.AllowRestart; } }

        public void InitializeRuntime(WheelGameSettings settings)
        {
            if (settings == null)
            {
                throw new InvalidOperationException("WheelGameState requires WheelGameSettings to initialize runtime.");
            }

            _settings = settings;
            Settings.InitializeRuntime();
            EnsureSliceBuffer();
        }

        public void Restart()
        {
            _zone = 1;
            _phase = WheelGamePhase.Ready;
            _lastResult = default(WheelSpinResult);
            _hasLastResult = false;
            _inventory.Clear();
            _slicesDirty = true;
            PrepareCurrentZone();
        }

        public void BeginSpin()
        {
            _phase = WheelGamePhase.Spinning;
        }

        public void CashOut()
        {
            _phase = WheelGamePhase.CashedOut;
        }

        public int SelectSpinSliceIndex()
        {
            PrepareCurrentZone();
            return WheelSpinOutcomeSelector.SelectSliceIndex(_sliceCount);
        }

        public WheelSpinResult CreateSpinResult(int sliceIndex)
        {
            PrepareCurrentZone();
            if (sliceIndex < 0 || sliceIndex >= _sliceCount)
            {
                throw new ArgumentOutOfRangeException(nameof(sliceIndex), "Slice index is outside the current wheel slice count.");
            }

            return new WheelSpinResult(sliceIndex, _sliceBuffer[sliceIndex]);
        }

        public int FindFirstSliceIndex(bool isBomb)
        {
            PrepareCurrentZone();
            return WheelSliceQueries.FindFirst(_sliceBuffer, _sliceCount, isBomb);
        }

        public int SelectRandomRewardSliceIndex()
        {
            PrepareCurrentZone();

            int rewardCount = 0;
            for (int i = 0; i < _sliceCount; i++)
            {
                if (!_sliceBuffer[i].IsBomb)
                {
                    rewardCount++;
                }
            }

            if (rewardCount <= 0)
            {
                return -1;
            }

            int selectedReward = UnityEngine.Random.Range(0, rewardCount);
            for (int i = 0; i < _sliceCount; i++)
            {
                if (_sliceBuffer[i].IsBomb)
                {
                    continue;
                }

                if (selectedReward == 0)
                {
                    return i;
                }

                selectedReward--;
            }

            return -1;
        }

        public int CopySliceDefinitions(WheelSliceDefinition[] destination)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            PrepareCurrentZone();

            int count = Mathf.Min(_sliceCount, destination.Length);
            for (int i = 0; i < count; i++)
            {
                WheelSliceCopyUtility.CopyInto(_sliceBuffer[i], destination[i]);
            }

            return count;
        }

        public WheelSliceDefinition[] CreateSliceSnapshot()
        {
            PrepareCurrentZone();

            var copies = new WheelSliceDefinition[_sliceCount];
            for (int i = 0; i < _sliceCount; i++)
            {
                copies[i] = WheelSliceCopyUtility.CreateCopy(_sliceBuffer[i]);
            }

            return copies;
        }

        public void Resolve(WheelSpinResult result)
        {
            _lastResult = result;
            _hasLastResult = true;
            WheelSpinResolveProfile profile = Settings.SpinResolveCatalog.GetProfile(result.IsBomb);
            ApplyResolveProfile(result, profile);
        }

        public void PrepareCurrentZone()
        {
            if (!_slicesDirty)
            {
                return;
            }

            RefreshSlicesForCurrentZone();
        }

        private void ApplyResolveProfile(WheelSpinResult result, WheelSpinResolveProfile profile)
        {
            _phase = profile.TargetPhase;

            if (profile.ClearInventory)
            {
                _inventory.Clear();
            }

            if (profile.AddResultToInventory)
            {
                _inventory.Add(result, Settings.UiCopy.InventoryFallbackRewardName);
            }

            if (profile.AdvanceZone)
            {
                _zone++;
            }

            if (profile.MarkSlicesDirty)
            {
                _slicesDirty = true;
                PrepareCurrentZone();
            }
        }

        private void RefreshSlicesForCurrentZone()
        {
            FillSlices();
            _slicesDirty = false;
        }

        private void FillSlices()
        {
            EnsureSliceBuffer();
            _sliceCount = Settings.FillSlicesForZone(_zone, _sliceBuffer);
        }

        private void EnsureSliceBuffer()
        {
            if (_sliceBuffer != null && _sliceBuffer.Length == Settings.SliceCount)
            {
                return;
            }

            AllocateSliceBuffer();
        }

        private void AllocateSliceBuffer()
        {
            _sliceBuffer = new WheelSliceDefinition[Settings.SliceCount];
            for (int i = 0; i < _sliceBuffer.Length; i++)
            {
                _sliceBuffer[i] = new WheelSliceDefinition();
            }
        }
    }
}
