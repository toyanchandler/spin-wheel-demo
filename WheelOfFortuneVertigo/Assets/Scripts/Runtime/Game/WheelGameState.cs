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
        public WheelSliceDefinition[] Slices { get { return _sliceBuffer; } }
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

        public void InitializeRuntime()
        {
            _settings = WheelRuntimeLocator.Settings;
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
