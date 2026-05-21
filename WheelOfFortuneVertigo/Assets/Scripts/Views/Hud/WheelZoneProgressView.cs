using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.Wheel.Runtime;

namespace Vertigo.Wheel.Views
{
    public sealed class WheelZoneProgressView : MonoBehaviour
    {
        [Serializable]
        private struct ZoneProgressCellBinding
        {
            public Image Image;
            public TextMeshProUGUI Label;
        }

        private enum ZoneProgressCellKind
        {
            Standard,
            Safe,
            Super,
            Current
        }

        private delegate bool CellRuleMatcher(WheelHudSnapshot snapshot, int zone, bool isCurrent);
        private delegate Color CellColorResolver(WheelHudSnapshot snapshot);

        private readonly struct ZoneProgressCellStyle
        {
            public readonly CellColorResolver TextColor;
            public readonly CellColorResolver ImageColor;

            public ZoneProgressCellStyle(CellColorResolver textColor, CellColorResolver imageColor)
            {
                TextColor = textColor;
                ImageColor = imageColor;
            }
        }

        private readonly struct ZoneProgressCellRule
        {
            public readonly ZoneProgressCellKind Kind;
            public readonly CellRuleMatcher Matches;

            public ZoneProgressCellRule(ZoneProgressCellKind kind, CellRuleMatcher matches)
            {
                Kind = kind;
                Matches = matches;
            }
        }

        private static readonly ZoneProgressCellRule[] CellRules =
        {
            new ZoneProgressCellRule(ZoneProgressCellKind.Current, (snapshot, zone, isCurrent) => isCurrent),
            new ZoneProgressCellRule(ZoneProgressCellKind.Super, (snapshot, zone, isCurrent) => IsIntervalZone(zone, snapshot.SuperZoneInterval)),
            new ZoneProgressCellRule(ZoneProgressCellKind.Safe, (snapshot, zone, isCurrent) => IsIntervalZone(zone, snapshot.SafeZoneInterval)),
            new ZoneProgressCellRule(ZoneProgressCellKind.Standard, (snapshot, zone, isCurrent) => true)
        };

        private static readonly ZoneProgressCellStyle[] StylesByKind =
        {
            new ZoneProgressCellStyle(
                snapshot => new Color(0.8f, 0.82f, 0.86f, 0.82f),
                snapshot => new Color(1f, 1f, 1f, 0.42f)),
            new ZoneProgressCellStyle(
                snapshot => new Color(0.45f, 0.9f, 1f, 1f),
                snapshot => new Color(0.3f, 0.82f, 1f, 0.68f)),
            new ZoneProgressCellStyle(
                snapshot => new Color(0.58f, 1f, 0.28f, 1f),
                snapshot => new Color(0.65f, 1f, 0.25f, 0.72f)),
            new ZoneProgressCellStyle(
                snapshot => snapshot.ZoneNumberColor,
                snapshot => Color.white)
        };

        [SerializeField] private ZoneProgressCellBinding[] _cells = Array.Empty<ZoneProgressCellBinding>();
        [SerializeField] private Sprite _standardSprite;
        [SerializeField] private Sprite _currentSprite;
        [SerializeField] private Sprite _safeSprite;
        [SerializeField] private Sprite _superSprite;

        private Sprite[] _spritesByKind = Array.Empty<Sprite>();
        private WheelEventBus _eventBus;

        private void OnValidate()
        {
            SyncSpriteTable();
        }

        public void Bind(WheelEventBus eventBus)
        {
            RequireCells();
            SyncSpriteTable();
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
        }

        private void RequireCells()
        {
            if (_cells == null || _cells.Length == 0)
            {
                throw new InvalidOperationException(
                    name + " has no collected cells. Use Collect Zone Cells in the inspector or rebuild the scene.");
            }
        }

        private void OnHudStateChanged(WheelHudSnapshot snapshot)
        {
            int halfCount = _cells.Length / 2;
            int firstZone = Mathf.Max(1, snapshot.Zone - halfCount);

            for (int i = 0; i < _cells.Length; i++)
            {
                int zone = firstZone + i;
                bool isCurrent = zone == snapshot.Zone;
                ZoneProgressCellKind kind = ResolveCellKind(snapshot, zone, isCurrent);
                ZoneProgressCellStyle style = StylesByKind[(int)kind];
                _cells[i].Label.SetText("{0}", zone);
                _cells[i].Label.color = style.TextColor(snapshot);
                _cells[i].Image.sprite = _spritesByKind[(int)kind];
                _cells[i].Image.color = style.ImageColor(snapshot);
            }
        }

        private void SyncSpriteTable()
        {
            _spritesByKind = new[]
            {
                _standardSprite,
                _safeSprite,
                _superSprite,
                _currentSprite
            };
        }

        private static ZoneProgressCellKind ResolveCellKind(WheelHudSnapshot snapshot, int zone, bool isCurrent)
        {
            for (int i = 0; i < CellRules.Length; i++)
            {
                ZoneProgressCellRule rule = CellRules[i];
                if (rule.Matches(snapshot, zone, isCurrent))
                {
                    return rule.Kind;
                }
            }

            return ZoneProgressCellKind.Standard;
        }

        private static bool IsIntervalZone(int zone, int interval)
        {
            return interval > 0 && zone % interval == 0;
        }
    }
}
