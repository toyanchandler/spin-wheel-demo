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
            public Image Frame;
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
            public readonly CellColorResolver FrameColor;
            public readonly CellColorResolver GlowColor;

            public ZoneProgressCellStyle(CellColorResolver textColor, CellColorResolver imageColor, CellColorResolver frameColor, CellColorResolver glowColor)
            {
                TextColor = textColor;
                ImageColor = imageColor;
                FrameColor = frameColor;
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
                snapshot => new Color(0.82f, 0.88f, 0.95f, 0.76f),
                snapshot => Color.clear,
                snapshot => Color.clear,
                snapshot => Color.clear),
            new ZoneProgressCellStyle(
                snapshot => new Color(0.62f, 0.97f, 1f, 1f),
                snapshot => new Color(0.06f, 0.54f, 0.70f, 0.44f),
                snapshot => new Color(0.52f, 0.98f, 1f, 0.82f),
                snapshot => new Color(0.22f, 0.88f, 1f, 0.42f)),
            new ZoneProgressCellStyle(
                snapshot => new Color(1f, 0.86f, 0.42f, 0.98f),
                snapshot => new Color(0.58f, 0.38f, 0.06f, 0.34f),
                snapshot => new Color(1f, 0.78f, 0.32f, 0.78f),
                snapshot => new Color(1f, 0.72f, 0.22f, 0.34f)),
            new ZoneProgressCellStyle(
                snapshot => Color.white,
                snapshot => new Color(0.18f, 0.82f, 1f, 0.96f),
                snapshot => new Color(0.94f, 1f, 1f, 0.96f),
                snapshot => new Color(0.18f, 0.86f, 1f, 0.54f))
        };

        [SerializeField] private ZoneProgressCellBinding[] _cells = Array.Empty<ZoneProgressCellBinding>();
        [SerializeField] private Image[] _connectors = Array.Empty<Image>();
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
        private Vector2[] _connectorBasePositions = Array.Empty<Vector2>();
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
            CacheConnectorBasePositions();
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

            int previousWindowStart = GetWindowStart(_lastSnapshot);
            int nextWindowStart = GetWindowStart(snapshot);
            if (previousWindowStart == nextWindowStart)
            {
                AnimateWithinWindow(snapshot, nextWindowStart);
                return;
            }

            AnimateAdvance(snapshot, nextWindowStart);
        }

        private void AnimateWithinWindow(WheelHudSnapshot snapshot, int windowStart)
        {
            KillSlide();
            RenderWindow(snapshot, false);
            int previousIndex = _lastZone - windowStart;
            int currentIndex = snapshot.Zone - windowStart;
            _slideSequence = DOTween.Sequence().SetUpdate(true);
            AddMarkerScaleTweens(previousIndex, currentIndex);
            _slideSequence.OnComplete(() =>
            {
                RenderWindow(snapshot);
                _lastSnapshot = snapshot;
                _lastZone = snapshot.Zone;
                _slideSequence = null;
            });
        }

        private void AnimateAdvance(WheelHudSnapshot snapshot, int nextWindowStart)
        {
            KillSlide();
            RenderWindow(snapshot, false);
            OffsetCells(new Vector2(_cellSpacing, 0f));
            OffsetConnectors(new Vector2(_cellSpacing, 0f));
            _slideSequence = DOTween.Sequence().SetUpdate(true);
            for (int i = 0; i < _cells.Length; i++)
            {
                ZoneProgressCellBinding cell = _cells[i];
                _slideSequence.Join(cell.Root.DOAnchorPos(
                    _cellBasePositions[i],
                    _slideDuration).SetEase(Ease.OutCubic));
            }

            for (int i = 0; i < _connectors.Length; i++)
            {
                Image connector = _connectors[i];
                if (connector == null)
                {
                    continue;
                }

                _slideSequence.Join(connector.rectTransform.DOAnchorPos(
                    _connectorBasePositions[i],
                    _slideDuration).SetEase(Ease.OutCubic));
            }

            int previousIndex = _lastZone - nextWindowStart;
            int currentIndex = snapshot.Zone - nextWindowStart;
            AddMarkerScaleTweens(previousIndex, currentIndex);

            _slideSequence.OnComplete(() =>
            {
                RestoreBasePositions();
                RestoreConnectorBasePositions();
                RenderWindow(snapshot);
                _lastSnapshot = snapshot;
                _lastZone = snapshot.Zone;
                _slideSequence = null;
            });
        }

        private void RenderWindow(WheelHudSnapshot snapshot, bool pulseCurrent = true)
        {
            int windowStart = GetWindowStart(snapshot);
            for (int i = 0; i < _cells.Length; i++)
            {
                int zone = windowStart + i;
                RenderCell(snapshot, i, zone, zone == snapshot.Zone, pulseCurrent);
            }

            RenderConnectors(snapshot, windowStart);
        }

        private void RenderCell(WheelHudSnapshot snapshot, int index, int zone, bool isCurrent, bool pulseCurrent)
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
            cell.Image.enabled = kind != ZoneProgressCellKind.Standard;
            cell.Image.color = style.ImageColor(snapshot);
            if (cell.Frame != null)
            {
                cell.Frame.enabled = kind != ZoneProgressCellKind.Standard;
                cell.Frame.color = style.FrameColor(snapshot);
            }

            if (cell.Glow != null)
            {
                cell.Glow.enabled = kind != ZoneProgressCellKind.Standard;
                cell.Glow.color = style.GlowColor(snapshot);
            }

            float scale = isCurrent ? _currentCellScale : _regularCellScale;
            cell.Root.DOKill();
            cell.Root.localScale = new Vector3(scale, scale, 1f);
            if (isCurrent && pulseCurrent)
            {
                cell.Root.DOScale(new Vector3(scale * 1.018f, scale * 1.018f, 1f), 0.64f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetTarget(cell.Root)
                    .SetUpdate(true);
            }
        }

        private void RenderConnectors(WheelHudSnapshot snapshot, int windowStart)
        {
            for (int i = 0; i < _connectors.Length; i++)
            {
                Image connector = _connectors[i];
                if (connector == null)
                {
                    continue;
                }

                int leftZone = windowStart + i;
                int rightZone = leftZone + 1;
                bool isVisible = leftZone > 0 && rightZone > 0;
                connector.gameObject.SetActive(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                bool isCompleted = rightZone <= snapshot.Zone;
                connector.color = isCompleted
                    ? new Color(0.28f, 0.86f, 1f, 0.46f)
                    : new Color(0.76f, 0.88f, 1f, 0.16f);
            }
        }

        private void ApplyLabelStyle(TextMeshProUGUI label, ZoneProgressCellKind kind, bool isCurrent)
        {
            label.enableAutoSizing = true;
            label.fontSizeMax = isCurrent ? 31f : 23f;
            label.fontSizeMin = 18f;
            label.fontStyle = FontStyles.Normal;
            label.characterSpacing = 0f;
            label.outlineWidth = isCurrent ? 0.14f : 0.08f;
            label.outlineColor = new Color(0f, 0f, 0f, kind == ZoneProgressCellKind.Standard ? 0.76f : 0.92f);
        }

        private int GetWindowStart(WheelHudSnapshot snapshot)
        {
            int centerIndex = _cells.Length / 2;
            return snapshot.Zone - centerIndex;
        }

        private void AddMarkerScaleTweens(int previousIndex, int currentIndex)
        {
            if (IsCellIndex(previousIndex))
            {
                RectTransform previous = _cells[previousIndex].Root;
                previous.localScale = new Vector3(_currentCellScale, _currentCellScale, 1f);
                _slideSequence.Join(previous.DOScale(
                    new Vector3(_regularCellScale, _regularCellScale, 1f),
                    _slideDuration * 0.86f).SetEase(Ease.OutCubic));
            }

            if (IsCellIndex(currentIndex))
            {
                RectTransform current = _cells[currentIndex].Root;
                current.localScale = new Vector3(_regularCellScale, _regularCellScale, 1f);
                _slideSequence.Join(current.DOScale(
                    new Vector3(_currentCellScale, _currentCellScale, 1f),
                    _slideDuration).SetEase(Ease.OutBack, 1.65f));
            }
        }

        private bool IsCellIndex(int index)
        {
            return index >= 0 && index < _cells.Length && _cells[index].Root != null;
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

        private void CacheConnectorBasePositions()
        {
            _connectorBasePositions = new Vector2[_connectors.Length];
            for (int i = 0; i < _connectors.Length; i++)
            {
                if (_connectors[i] != null)
                {
                    _connectorBasePositions[i] = _connectors[i].rectTransform.anchoredPosition;
                }
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

        private void RestoreConnectorBasePositions()
        {
            for (int i = 0; i < _connectors.Length; i++)
            {
                if (_connectors[i] == null)
                {
                    continue;
                }

                _connectors[i].rectTransform.DOKill();
                _connectors[i].rectTransform.anchoredPosition = _connectorBasePositions[i];
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

        private void OffsetConnectors(Vector2 offset)
        {
            for (int i = 0; i < _connectors.Length; i++)
            {
                if (_connectors[i] == null)
                {
                    continue;
                }

                _connectors[i].rectTransform.DOKill();
                _connectors[i].rectTransform.anchoredPosition = _connectorBasePositions[i] + offset;
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

            for (int i = 0; i < _connectors.Length; i++)
            {
                if (_connectors[i] != null)
                {
                    _connectors[i].rectTransform.DOKill();
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
