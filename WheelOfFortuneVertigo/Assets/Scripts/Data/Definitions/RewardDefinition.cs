using System;
using UnityEngine;

namespace Vertigo.Wheel.Data
{
    public enum RewardTier
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public sealed class RewardDefinition
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Sprite _wheelIcon;
        [SerializeField] private int _amount = 1;
        [SerializeField] private RewardTier _tier;
        [SerializeField] private Color _accentColor = Color.white;

        [NonSerialized] private string _label;
        [NonSerialized] private string _winLabel;

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Sprite WheelIcon => _wheelIcon != null ? _wheelIcon : _icon;
        public int Amount => _amount;
        public RewardTier Tier => _tier;
        public Color AccentColor => _accentColor;

        public string Label
        {
            get
            {
                RequireCachedText();
                return _label;
            }
        }

        public string WinLabel
        {
            get
            {
                RequireCachedText();
                return _winLabel;
            }
        }

        public static RewardDefinition Create(
            string id,
            string displayName,
            Sprite icon,
            int amount,
            RewardTier tier,
            Color accentColor,
            Sprite wheelIcon = null)
        {
            return new RewardDefinition
            {
                _id = id,
                _displayName = displayName,
                _icon = icon,
                _wheelIcon = wheelIcon,
                _amount = amount,
                _tier = tier,
                _accentColor = accentColor
            };
        }

        private void RequireCachedText()
        {
            if (_label == null || _winLabel == null) throw new InvalidOperationException("RewardDefinition text was not cached. Call CacheRuntimeText first.");
        }

        public void CacheRuntimeText(string winLabelFormat)
        {
            _label = _amount > 1 ? _displayName + " x" + _amount : _displayName;
            _winLabel = string.Format(winLabelFormat, _label);
        }
    }
}
