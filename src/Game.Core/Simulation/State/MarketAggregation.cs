using System;

namespace Game.Core.Simulation.State
{
    public sealed class MarketAggregation
    {
        public float[] NationalSupply { get; }
        public float[] NationalDemand { get; }
        public float[] GlobalSupply { get; }
        public float[] GlobalDemand { get; }

        public MarketAggregation(int commodityCount)
        {
            if (commodityCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(commodityCount));
            }

            NationalSupply = new float[commodityCount];
            NationalDemand = new float[commodityCount];
            GlobalSupply = new float[commodityCount];
            GlobalDemand = new float[commodityCount];
        }

        public void Clear()
        {
            Array.Clear(NationalSupply, 0, NationalSupply.Length);
            Array.Clear(NationalDemand, 0, NationalDemand.Length);
            Array.Clear(GlobalSupply, 0, GlobalSupply.Length);
            Array.Clear(GlobalDemand, 0, GlobalDemand.Length);
        }
    }
}
