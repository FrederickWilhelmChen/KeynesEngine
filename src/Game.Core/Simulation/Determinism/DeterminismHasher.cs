using System;

namespace Game.Core.Simulation.Determinism
{
    public static class DeterminismHasher
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        public static ulong Hash(ReadOnlySpan<float> values)
        {
            var hash = OffsetBasis;
            for (var i = 0; i < values.Length; i++)
            {
                var bits = (uint)BitConverter.SingleToInt32Bits(values[i]);
                hash ^= bits;
                hash *= Prime;
            }

            return hash;
        }

        public static ulong Hash(ReadOnlySpan<int> values)
        {
            var hash = OffsetBasis;
            for (var i = 0; i < values.Length; i++)
            {
                hash ^= (uint)values[i];
                hash *= Prime;
            }

            return hash;
        }

        public static ulong Combine(ulong left, ulong right)
        {
            var hash = OffsetBasis;
            hash ^= left;
            hash *= Prime;
            hash ^= right;
            hash *= Prime;
            return hash;
        }
    }
}
