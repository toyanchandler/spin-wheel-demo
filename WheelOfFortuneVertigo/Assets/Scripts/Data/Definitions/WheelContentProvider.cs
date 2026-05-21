namespace Vertigo.Wheel.Data
{
    public enum ZoneType
    {
        Standard,
        Safe,
        Super
    }

    public interface IWheelContentProvider
    {
        WheelLayoutSettings Layout { get; }
        WheelThemeSettings Theme { get; }
        ZoneType GetZoneType(int zone);
        int FillSlicesForZone(int zone, WheelSliceDefinition[] buffer);
    }
}
