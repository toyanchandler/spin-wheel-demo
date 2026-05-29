using System;
using UnityEngine;
using Vertigo.Collections;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    [WheelBind]
    public sealed class WheelZoneProgressView : MonoBehaviour
    {
        [SerializeField] private Transform _cellRoot;

        [CollectChildren(nameof(_cellRoot))]
        [SerializeField] private WheelZoneProgressCellView[] _cells = Array.Empty<WheelZoneProgressCellView>();

        [WheelInject] private WheelEventBus _eventBus;

        private bool _hasSnapshot;
        private WheelHudSnapshot _lastSnapshot;

        [WheelAfterInject]
        private void Connect()
        {
            ValidateWiring();
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void RefreshResponsiveLayout()
        {
            if (!_hasSnapshot) return;
            ApplySnapshot(_lastSnapshot);
        }

        [WheelBeforeUnbind]
        private void Disconnect()
        {
            if (_eventBus == null) return;
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _hasSnapshot = false;
        }

        private void ValidateWiring()
        {
            if (_cells == null || _cells.Length == 0) throw new InvalidOperationException(
                    name + " has no zone progress cells. Collect children in the inspector.");
            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i] == null || !_cells[i].IsWired())
                {
                    throw new InvalidOperationException(
                        name + " cell at index " + i + " is incomplete. Collect children in the inspector.");
                }
            }
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            _lastSnapshot = snapshot;
            _hasSnapshot = true;
            ApplySnapshot(snapshot);
        }

        private void ApplySnapshot(WheelHudSnapshot snapshot)
        {
            WheelZoneProgressWindow window = new WheelZoneProgressWindow(snapshot, _cells.Length);

            for (int i = 0; i < _cells.Length; i++)
            {
                WheelZoneProgressCellPresentation presentation =
                    WheelZoneProgressPresentationBuilder.Build(snapshot, window.WindowStart, i);
                _cells[i].Apply(presentation);
            }
        }
    }
}
