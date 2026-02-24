using System;

namespace Game.Core.Simulation.Phases
{
    public sealed class MarketSolvePhase : IPhase
    {
        public PhaseId Id => PhaseId.Phase4MarketSolve;

        public void Execute(PhaseContext context)
        {
            var aggregation = context.Aggregation;
            var market = context.Market;
            var globalPrices = context.State.GlobalMarketPrices;
            var commodityCount = globalPrices.Length;
            var localPrices = context.State.LocalPrices;
            var provinceCount = context.State.ProvinceCount;

            for (var i = 0; i < commodityCount; i++)
            {
                var supply = aggregation.NationalSupply[i];
                var demand = aggregation.NationalDemand[i];
                var sensitivity = context.State.PriceSensitivity[i];
                var dominance = context.State.PriceDominance[i];
                var ratio = demand > 0f && supply > 0f
                    ? demand / supply
                    : 1f;

                if (dominance < 0)
                {
                    ratio = 1f / Math.Max(ratio, 0.01f);
                }

                var price = globalPrices[i] * PriceFactor(ratio, sensitivity);
                var minPrice = globalPrices[i] * 0.5f;
                var maxPrice = globalPrices[i] * 2.0f;
                if (price < minPrice) price = minPrice;
                if (price > maxPrice) price = maxPrice;

                market.NationalPrices[i] = price;
            }

            Array.Copy(globalPrices, market.GlobalPrices, commodityCount);

            for (var provinceId = 0; provinceId < provinceCount; provinceId++)
            {
                for (var commodityId = 0; commodityId < commodityCount; commodityId++)
                {
                    var index = provinceId * commodityCount + commodityId;
                    localPrices[index] = market.NationalPrices[commodityId];
                }
            }
        }

        private static float PriceFactor(float ratio, float sensitivity)
        {
            if (ratio <= 0f)
            {
                return 1f;
            }

            var clamped = Math.Max(0.25f, Math.Min(4.0f, ratio));
            var baseFactor = 0.5f + clamped / (1f + clamped);
            return MathF.Pow(baseFactor, Math.Max(0.1f, sensitivity));
        }
    }
}
