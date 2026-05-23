using System;
using DG.Tweening;
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
            public RectTransform Root;
            public Image Image;
            public Image Glow;
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
            public readonly CellColorResolver GlowColor;

            public ZoneProgressCellStyle(CellColorResolver textColor, CellColorResolver imageColor, CellColorResolver glowColor)
            {
                TextColor = textColor;
                ImageColor = imageColor;
                GlowColor = glowColor;
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
                snapshot => new Color(0.72f, 0.74f, 0.78f, 0.7f),
                snapshot => new Color(0.12f, 0.13f, 0.15f, 0.58f),
                snapshot => Color.clear),
            new ZoneProgressCellStyle(
                snapshot => new Color(0.42f, 0.88f, 1f, 0.96f),
                snapshot => new Color(0.16f, 0.74f, 0.98f, 0.82f),
                snapshot => new Color(0.28f, 0.9f, 1f, 0.46f)),
            new ZoneProgressCellStyle(
                snapshot => new Color(0.58f, 1f, 0.2f, 0.96f),
                snapshot => new Color(0.25f, 0.76f, 0.04f, 0.86f),
                snapshot => new Color(0.55f, 1f, 0.16f, 0.5f)),
            new ZoneProgressCellStyle(
                snapshot => Color.white,
                snapshot => Color.white,
                snapshot => new Color(1f, 1f, 1f, 0.78f))
        };

        [SerializeField] private ZoneProgressCellBinding[] _cells = Array.Empty<ZoneProgressCellBinding>();
        [SerializeField] private Sprite _standardSprite;
        [SerializeField] private Sprite _currentSprite;
        [SerializeField] private Sprite _safeSprite;
        [SerializeField] private Sprite _superSprite;
        [SerializeField] private float _cellSpacing = 86f;
        [SerializeField] private float _slideDuration = 0.34f;
        [SerializeField] private float _regularCellScale = 0.86f;
        [SerializeField] private float _currentCellScale = 1.16f;

        private Sprite[] _spritesByKind = Array.Empty<Sprite>();
        private Vector2[] _cellBasePositions = Array.Empty<Vector2>();
        private WheelEventBus _eventBus;
        private WheelHudSnapshot _lastSnapshot;
        private int _lastZone;
        private Sequence _slideSequence;

        private void OnValidate()
        {
            SyncSpriteTable();
        }

        public void Bind(WheelEventBus eventBus)
        {
            RequireCells();
            SyncSpriteTable();
            CacheBasePositions();
            _eventBus = eventBus;
            _eventBus.HudStateChanged += OnHudStateChanged;
        }

        public void Unbind()
        {
            _eventBus.HudStateChanged -= OnHudStateChanged;
            _eventBus = null;
            KillSlide();
            _lastZone = 0;
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
            int direction = Math.Sign(snapshot.Zone - _lastZone);
            if (_lastZone == 0 || direction <= 0 || Mathf.Abs(snapshot.Zone - _lastZone) > 1)
            {
                RenderWindow(snapshot);
                _lastSnapshot = snapshot;
                _lastZone = snapshot.Zone;
                return;
            }

            AnimateAdvance(snapshot);
        }

        private void AnimateAdvance(WheelHudSnapshot snapshot)
        {
            KillSlide();
            RenderWindow(snapshot);
            OffsetCells(new Vector2(_cellSpacing, 0f));
            _slideSequence = DOTween.Sequence().SetUpdate(true);
            for (int i = 0; i < _cells.Length; i++)
            {
                ZoneProgressCellBinding cell = _cells[i];
                _slideSequence.Join(cell.Root.DOAnchorPos(
                    _cellBasePositions[i],
                    _slideDuration).SetEase(Ease.OutCubic));
            }

            _slideSequence.OnComplete(() =>
            {
                RestoreBasePositions();
                RenderWindow(snapshot);
                _lastSnapshot = snapshot;
                _lastZone = snapshot.Zone;
                _slideSequence = null;
            });
        }

        private void RenderWindow(WheelHudSnapshot snapshot)
        {
            int centerIndex = _cells.Length / 2;
            for (int i = 0; i < _cells.Length; i++)
            {
                int zone = snapshot.Zone + i - centerIndex;
                RenderCell(snapshot, i, zone, zone == snapshot.Zone);
            }
        }

        private void RenderCell(WheelHudSnapshot snapshot, int index, int zone, bool isCurrent)
        {
            ZoneProgressCellBinding cell = _cells[index];
            int isValid = Convert.ToInt32(zone > 0);
            cell.Root.gameObject.SetActive(isValid == 1);
            if (isValid == 0)
            {
                return;
            }

            ZoneProgressCellKind kind = ResolveCellKind(snapshot, zone, isCurrent);
            ZoneProgressCellStyle style = StylesByKind[(int)kind];
            cell.Label.SetText("{0}", zone);
            cell.Label.color = style.TextColor(snapshot);
            ApplyLabelStyle(cell.Label, kind, isCurrent);
            cell.Image.sprite = _spritesByKind[(int)kind];
            cell.Image.color = style.ImageColor(snapshot);
            if (cell.Glow != null)
            {
                cell.Glow.color = style.GlowColor(snapshot);
            }

            float scale = isCurrent ? _currentCellScale : _regularCellScale;
            cell.Root.localScale = new Vector3(scale, scale, 1f);
        }

        private void ApplyLabelStyle(TextMeshProUGUI label, ZoneProgressCellKind kind, bool isCurrent)
        {
            label.enableAutoSizing = true;
            label.fontSizeMax = isCurrent ? 34f : 28f;
            label.fontSizeMin = 18f;
            label.fontStyle = FontStyles.Normal;
            label.characterSpacing = 0f;
            label.outlineWidth = isCurrent ? 0.12f : 0.08f;
            label.outlineColor = new Color(0f, 0f, 0f, kind == ZoneProgressCellKind.Standard ? 0.76f : 0.92f);
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

        private void CacheBasePositions()
        {
            _cellBasePositions = new Vector2[_cells.Length];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cellBasePositions[i] = _cells[i].Root.anchoredPosition;
            }
        }

        private void RestoreBasePositions()
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].Root.DOKill();
                _cells[i].Root.anchoredPosition = _cellBasePositions[i];
            }
        }

        private void OffsetCells(Vector2 offset)
        {
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i].Root.DOKill();
                _cells[i].Root.anchoredPosition = _cellBasePositions[i] + offset;
            }
        }

        private void KillSlide()
        {
            if (_slideSequence != null)
            {
                _slideSequence.Kill();
                _slideSequence = null;
            }

            for (int i = 0; i < _cells.Length; i++)
            {
                if (_cells[i].Root != null)
                {
                    _cells[i].Root.DOKill();
                }
            }
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
