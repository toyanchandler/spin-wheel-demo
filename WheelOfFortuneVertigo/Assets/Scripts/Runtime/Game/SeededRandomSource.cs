namespace Vertigo.Wheel.Runtime
{
    /// <summary>Deterministic <see cref="IRandomSource"/> for edit-mode and play-mode tests.</summary>
    public sealed class SeededRandomSource : IRandomSource
    {
        private int _state;

        public SeededRandomSource(int seed)
        {
            _state = seed;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                return minInclusive;
            }

            int range = maxExclusive - minInclusive;
            _state = (_state * 1664525 + 1013904223);
            if (_state < 0)
            {
                _state = -_state;
            }

            return minInclusive + (_state % range);
        }
    }
}
