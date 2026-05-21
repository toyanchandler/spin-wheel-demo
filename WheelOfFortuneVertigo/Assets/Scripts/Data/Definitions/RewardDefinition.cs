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
        public string id;
        public string displayName;
        public Sprite icon;
        public int amount = 1;
        public RewardTier tier;
        public Color accentColor = Color.white;

        [NonSerialized] private string _label;
        [NonSerialized] private string _winLabel;

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

        private void RequireCachedText()
        {
            if (_label == null || _winLabel == null)
            {
                throw new InvalidOperationException("RewardDefinition text was not cached. Call CacheRuntimeText first.");
            }
        }

        public void CacheRuntimeText(string winLabelFormat)
        {
            _label = amount > 1 ? displayName + " x" + amount : displayName;
            _winLabel = string.Format(winLabelFormat, _label);
        }
    }
}
