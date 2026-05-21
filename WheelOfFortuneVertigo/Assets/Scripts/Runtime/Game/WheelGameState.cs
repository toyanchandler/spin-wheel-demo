using System;
using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelGameState : MonoBehaviour
    {
        [SerializeField] private WheelGameSettings _settings;

        private readonly RewardInventory _inventory = new RewardInventory();
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
        public ZoneType ZoneType { get { return _settings.GetZoneType(_zone); } }

        public WheelPhaseGameplayProfile PhaseGameplay { get { return _settings.UiCopy.GetPhaseCopy(_phase).gameplay; } }

        public bool CanSpin { get { return _settings != null && PhaseGameplay.allowSpin; } }

        public bool CanLeave
        {
            get
            {
                return _settings != null
                    && _settings.GetZoneGameplay(ZoneType).allowLeave
                    && PhaseGameplay.allowLeave;
            }
        }

        public bool CanRestart { get { return _settings != null && PhaseGameplay.allowRestart; } }

        public void InitializeRuntime()
        {
            _settings.InitializeRuntime();
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
            WheelSpinResolveProfile profile = _settings.SpinResolveCatalog.GetProfile(result.IsBomb);
            ApplyResolveProfile(result, profile);
        }

        public void PrepareCurrentZone()
        {
            SliceRefreshActions[Convert.ToInt32(_slicesDirty)](this);
        }

        private void ApplyResolveProfile(WheelSpinResult result, WheelSpinResolveProfile profile)
        {
            _phase = profile.targetPhase;
            ResolveEffects.Apply(_inventory, this, result, profile);
        }

        private void AdvanceZone()
        {
            _zone++;
        }

        private void MarkSlicesDirtyAndPrepare()
        {
            _slicesDirty = true;
            PrepareCurrentZone();
        }

        private void RefreshSlicesForCurrentZone()
        {
            FillSlices();
            _slicesDirty = false;
        }

        private void FillSlices()
        {
            EnsureSliceBuffer();
            _sliceCount = _settings.FillSlicesForZone(_zone, _sliceBuffer);
        }

        private void EnsureSliceBuffer()
        {
            BufferEnsureActions[Convert.ToInt32(_sliceBuffer != null && _sliceBuffer.Length == _settings.SliceCount)](this);
        }

        private void AllocateSliceBuffer()
        {
            _sliceBuffer = new WheelSliceDefinition[_settings.SliceCount];
            for (int i = 0; i < _sliceBuffer.Length; i++)
            {
                _sliceBuffer[i] = new WheelSliceDefinition();
            }
        }

        private static readonly Action<WheelGameState>[] SliceRefreshActions =
        {
            state => { },
            state => state.RefreshSlicesForCurrentZone()
        };

        private static readonly Action<WheelGameState>[] BufferEnsureActions =
        {
            state => state.AllocateSliceBuffer(),
            state => { }
        };

        private static class ResolveEffects
        {
            private static readonly Action<RewardInventory>[] ClearInventoryActions =
            {
                inventory => { },
                inventory => inventory.Clear()
            };

            private static readonly Action<RewardInventory, WheelSpinResult>[] AddResultActions =
            {
                (inventory, result) => { },
                (inventory, result) => inventory.Add(result)
            };

            private static readonly Action<WheelGameState>[] AdvanceZoneActions =
            {
                state => { },
                state => state.AdvanceZone()
            };

            private static readonly Action<WheelGameState>[] RefreshSliceActions =
            {
                state => { },
                state => state.MarkSlicesDirtyAndPrepare()
            };

            public static void Apply(
                RewardInventory inventory,
                WheelGameState state,
                WheelSpinResult result,
                WheelSpinResolveProfile profile)
            {
                ClearInventoryActions[Convert.ToInt32(profile.clearInventory)](inventory);
                AddResultActions[Convert.ToInt32(profile.addResultToInventory)](inventory, result);
                AdvanceZoneActions[Convert.ToInt32(profile.advanceZone)](state);
                RefreshSliceActions[Convert.ToInt32(profile.markSlicesDirty)](state);
            }
        }
    }
}
