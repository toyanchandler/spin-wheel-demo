#if UNITY_EDITOR
namespace Vertigo.Wheel.EditorTools
{
    internal static class WheelZoneProgressHierarchy
    {
        public const string CellsGroup = "ZoneProgressCells";
        public const string RailChrome = "ZoneProgressRailChrome";
        public const string Title = "ZoneProgressTitle";
        public const string HeaderZoneValue = "HeaderZoneValueHidden";
        public const string HeaderZoneType = "HeaderZoneTypeValue";

        public const string StandardState = "Standard";
        public const string SafeState = "Safe";
        public const string SuperState = "Super";
        public const string CurrentState = "Current";
        public const string ValueLabel = "Value";

        public const string CellPrefabName = "ZoneProgressCell";
        public const string HeaderLabelsGroup = "ZoneHeaderLabels";
    }
}
#endif
