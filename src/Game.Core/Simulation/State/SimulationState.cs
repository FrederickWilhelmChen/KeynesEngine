using System;

namespace Game.Core.Simulation.State
{
    public sealed class SimulationState
    {
        public int ProvinceCount { get; }
        public int CommodityCount { get; }

        public float[] Production { get; }
        public float[] Demand { get; }
        public float[] GlobalMarketPrices { get; }
        public float[] LocalPrices { get; }
        public float[] Inventory { get; }
        public float[] Capacity { get; }
        public float[] Efficiency { get; }
        public float[] DemandElasticity { get; }
        public float[] PriceSensitivity { get; }
        public int[] PriceDominance { get; }

        public SimulationState(int provinceCount, int commodityCount)
        {
            if (provinceCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(provinceCount));
            }

            if (commodityCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(commodityCount));
            }

            ProvinceCount = provinceCount;
            CommodityCount = commodityCount;

            var total = provinceCount * commodityCount;
            Production = new float[total];
            Demand = new float[total];
            GlobalMarketPrices = new float[commodityCount];
            LocalPrices = new float[total];
            Inventory = new float[total];
            Capacity = new float[total];
            Efficiency = new float[total];
            DemandElasticity = new float[commodityCount];
            PriceSensitivity = new float[commodityCount];
            PriceDominance = new int[commodityCount];
        }

        public int Index(int provinceId, int commodityId)
        {
            return provinceId * CommodityCount + commodityId;
        }
    }
}
