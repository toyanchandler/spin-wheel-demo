using System;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    /// <summary>
    /// Mutable play-session model: zone, phase, inventory, and wheel slice buffer.
    /// Does not publish UI events — <see cref="WheelGameFlowController"/> and
    /// <see cref="WheelStatePublisher"/> read this after changes.
    /// </summary>
    public sealed class WheelGameState : MonoBehaviour
    {
        private readonly RewardInventory _inventory = new RewardInventory();
        private WheelGameSettings _settings;
        private IRandomSource _randomSource = UnityRandomSource.Shared;
        private WheelSliceDefinition[] _sliceBuffer;
        private WheelSpinResult _lastResult;
        private bool _hasLastResult;
        private int _sliceCount;
        private int _zone = 1;
        private WheelGamePhase _phase = WheelGamePhase.Ready;
        private bool _slicesDirty = true;

        public WheelGameSettings Settings => _settings;
        public RewardInventory Inventory => _inventory;
        public WheelSpinResult LastResult => _lastResult;
        public bool HasLastResult => _hasLastResult;
        public int SliceCount => _sliceCount;
        public int Zone => _zone;
        public WheelGamePhase Phase => _phase;

        public ZoneType ZoneType
        {
            get
            {
                if (_settings == null)
                {
                    throw new InvalidOperationException(
                        "WheelGameState.ZoneType requires InitializeRuntime before access.");
                }

                return _settings.GetZoneType(_zone);
            }
        }

        public WheelPhaseGameplayProfile PhaseGameplay => Settings.UiCopy.GetPhaseCopy(_phase).Gameplay;

        public bool CanSpin => Settings != null && PhaseGameplay.AllowSpin;

        public bool CanLeave =>
            Settings != null
            && (Settings.GetZoneGameplay(ZoneType).AllowLeave || _inventory.Count > 0)
            && PhaseGameplay.AllowLeave;

        public bool CanRestart => Settings != null && PhaseGameplay.AllowRestart;

        public void InitializeRuntime(WheelGameSettings settings)
        {
            InitializeRuntime(settings, UnityRandomSource.Shared);
        }

        public void InitializeRuntime(WheelGameSettings settings, IRandomSource randomSource)
        {
            if (settings == null)
            {
                throw new InvalidOperationException("WheelGameState requires WheelGameSettings to initialize runtime.");
            }

            if (randomSource == null)
            {
                throw new ArgumentNullException(nameof(randomSource));
            }

            _settings = settings;
            _randomSource = randomSource;
            Settings.InitializeRuntime();
            EnsureSliceBuffer();
        }

        public void Restart()
        {
            _zone = 1;
            _phase = WheelGamePhase.Ready;
            _lastResult = default;
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
            return WheelSpinOutcomeSelector.SelectSliceIndex(_sliceCount, _randomSource);
        }

        public WheelSpinResult CreateSpinResult(int sliceIndex)
        {
            PrepareCurrentZone();
            if (sliceIndex < 0 || sliceIndex >= _sliceCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sliceIndex),
                    "Slice index is outside the current wheel slice count.");
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
            int rewardCount = WheelSliceQueries.CountNonBomb(_sliceBuffer, _sliceCount);
            if (rewardCount <= 0)
            {
                return -1;
            }

            int pick = _randomSource.Range(0, rewardCount);
            return WheelSliceQueries.SelectRandomRewardIndex(_sliceBuffer, _sliceCount, pick);
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

        /// <summary>Applies win/bomb rules from <see cref="WheelSpinResolveCatalog"/> via <see cref="WheelSpinResolvePipeline"/>.</summary>
        public void Resolve(WheelSpinResult result)
        {
            WheelSpinResolvePipeline.Apply(this, result);
        }

        internal void RecordSpinResult(WheelSpinResult result)
        {
            _lastResult = result;
            _hasLastResult = true;
        }

        internal void ApplyResolveProfile(WheelSpinResult result, WheelSpinResolveProfile profile)
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

        public void PrepareCurrentZone()
        {
            if (!_slicesDirty)
            {
                return;
            }

            RefreshSlicesForCurrentZone();
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
