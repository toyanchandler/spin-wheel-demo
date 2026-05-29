namespace Vertigo.Wheel.Runtime
{
    /// <summary>Production randomness backed by <see cref="UnityEngine.Random"/>.</summary>
    public sealed class UnityRandomSource : IRandomSource
    {
        public static readonly UnityRandomSource Shared = new UnityRandomSource();

        public int Range(int minInclusive, int maxExclusive)
        {
            return UnityEngine.Random.Range(minInclusive, maxExclusive);
        }
    }
}
