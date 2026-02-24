using System.Collections.Generic;

namespace Game.Core.Core.Market
{
    public sealed class GlobalMarketConfig
    {
        public int Seed { get; set; }
        public List<GlobalCommodityConfig> Commodities { get; set; } = new List<GlobalCommodityConfig>();
    }

    public sealed class GlobalCommodityConfig
    {
        public string Code { get; set; } = string.Empty;
        public float BasePrice { get; set; }
        public float ShockAmplitude { get; set; }
        public int ShockPeriod { get; set; }
        public float NoiseAmplitude { get; set; }
    }
}
