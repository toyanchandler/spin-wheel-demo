namespace Vertigo.Wheel.Runtime
{
    /// <summary>Gameplay randomness contract for deterministic tests and production parity.</summary>
    public interface IRandomSource
    {
        int Range(int minInclusive, int maxExclusive);
    }
}
