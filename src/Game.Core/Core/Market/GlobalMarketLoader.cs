using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Game.Core.Core.Ids;

namespace Game.Core.Core.Market
{
    public static class GlobalMarketLoader
    {
        public static GlobalMarketData Load(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Global market config not found: {jsonPath}");
            }

            var json = File.ReadAllText(jsonPath);
            var config = JsonSerializer.Deserialize<GlobalMarketConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null)
            {
                throw new InvalidOperationException("Global market config could not be parsed.");
            }

            if (config.Commodities.Count == 0)
            {
                throw new InvalidOperationException("Global market config contains no commodities.");
            }

            var definitions = config.Commodities.Select(item =>
                new CommodityDefinition(item.Code, item.BasePrice, item.ShockAmplitude, item.ShockPeriod, item.NoiseAmplitude));

            var catalog = CommodityCatalog.Create(definitions);
            var service = new GlobalMarketService(catalog, config.Seed);

            return new GlobalMarketData(catalog, service);
        }
    }

    public sealed class GlobalMarketData
    {
        public CommodityCatalog Catalog { get; }
        public GlobalMarketService Service { get; }

        public GlobalMarketData(CommodityCatalog catalog, GlobalMarketService service)
        {
            Catalog = catalog;
            Service = service;
        }
    }
}
