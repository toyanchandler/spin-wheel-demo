namespace Vertigo.Wheel.Data
{
    /// <summary>
    /// High-level play session moment. Drives CanSpin/CanLeave/CanRestart via UiCopy phase profiles.
    /// Transitions are documented in ARCHITECTURE_AND_LOGIC.md (phase model and post-spin resolve).
    /// </summary>
    public enum WheelGamePhase
    {
        Ready,
        Spinning,
        Won,
        Bombed,
        CashedOut
    }
}
