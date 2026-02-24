using System;

namespace Game.Core.Core.Random
{
    public sealed class DeterministicRandom
    {
        private uint _state;

        public DeterministicRandom(uint seed)
        {
            _state = seed == 0 ? 1u : seed;
        }

        public uint NextUInt()
        {
            var x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public float NextFloat01()
        {
            return (NextUInt() & 0x00FFFFFF) / (float)0x01000000;
        }

        public float NextNormal(float mean, float stdDev)
        {
            if (stdDev <= 0f)
            {
                return mean;
            }

            var u1 = Math.Max(NextFloat01(), 1e-7f);
            var u2 = NextFloat01();
            var radius = Math.Sqrt(-2.0 * Math.Log(u1));
            var theta = 2.0 * Math.PI * u2;
            var z0 = radius * Math.Cos(theta);
            return (float)(mean + z0 * stdDev);
        }
    }
}
