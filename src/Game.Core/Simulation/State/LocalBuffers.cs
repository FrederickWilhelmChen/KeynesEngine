using System;

namespace Game.Core.Simulation.State
{
    public sealed class LocalBuffers
    {
        public float[] Production { get; }
        public float[] Demand { get; }
        public float[] LocalPrices { get; }
        public float[] Inventory { get; }

        public LocalBuffers(int provinceCount, int commodityCount)
        {
            if (provinceCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(provinceCount));
            }

            if (commodityCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(commodityCount));
            }

            var total = provinceCount * commodityCount;
            Production = new float[total];
            Demand = new float[total];
            LocalPrices = new float[total];
            Inventory = new float[total];
        }

        public void Clear()
        {
            Array.Clear(Production, 0, Production.Length);
            Array.Clear(Demand, 0, Demand.Length);
            Array.Clear(LocalPrices, 0, LocalPrices.Length);
            Array.Clear(Inventory, 0, Inventory.Length);
        }
    }
}
