using System;
using Game.Core.Core.Ids;
using Game.Core.Core.Random;

namespace Game.Core.Core.Market
{
    public sealed class GlobalMarketService
    {
        private readonly CommodityCatalog _catalog;
        private readonly DeterministicRandom _random;
        private readonly float[] _prices;

        public GlobalMarketService(CommodityCatalog catalog, int seed)
        {
            _catalog = catalog;
            _random = new DeterministicRandom((uint)seed);
            _prices = new float[catalog.Definitions.Count];
        }

        public ReadOnlySpan<float> Prices => _prices;

        public void UpdatePrices(int tick)
        {
            var count = _catalog.Definitions.Count;
            for (var i = 0; i < count; i++)
            {
                var definition = _catalog.Definitions[i];
                var periodic = definition.ShockAmplitude * (float)Math.Sin(2.0 * Math.PI * tick / definition.ShockPeriod);
                var noise = _random.NextNormal(0f, definition.NoiseAmplitude);
                if (noise > 0.02f) noise = 0.02f;
                if (noise < -0.02f) noise = -0.02f;

                var price = definition.BasePrice * (1f + periodic + noise);
                var minPrice = definition.BasePrice * 0.5f;
                var maxPrice = definition.BasePrice * 2.0f;
                if (price < minPrice) price = minPrice;
                if (price > maxPrice) price = maxPrice;

                _prices[i] = price;
            }
        }
    }
}
