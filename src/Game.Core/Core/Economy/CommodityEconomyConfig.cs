using System.Collections.Generic;

namespace Game.Core.Core.Economy
{
    public sealed class CommodityEconomyConfig
    {
        public List<CommodityEconomyEntry> Commodities { get; set; } = new List<CommodityEconomyEntry>();
    }

    public sealed class CommodityEconomyEntry
    {
        public string Code { get; set; } = string.Empty;
        public float DemandElasticity { get; set; }
        public float PriceSensitivity { get; set; }
        public string PriceDominance { get; set; } = "Demand";
    }
}
