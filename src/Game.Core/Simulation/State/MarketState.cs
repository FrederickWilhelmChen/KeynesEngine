using System;

namespace Game.Core.Simulation.State
{
    public sealed class MarketState
    {
        public float[] NationalPrices { get; }
        public float[] GlobalPrices { get; }

        public MarketState(int commodityCount)
        {
            if (commodityCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(commodityCount));
            }

            NationalPrices = new float[commodityCount];
            GlobalPrices = new float[commodityCount];
        }

        public void Clear()
        {
            Array.Clear(NationalPrices, 0, NationalPrices.Length);
            Array.Clear(GlobalPrices, 0, GlobalPrices.Length);
        }
    }
}
