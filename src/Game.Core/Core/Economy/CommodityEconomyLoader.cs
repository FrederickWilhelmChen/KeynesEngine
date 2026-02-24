using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Game.Core.Core.Economy
{
    public static class CommodityEconomyLoader
    {
        public static IReadOnlyDictionary<string, CommodityEconomyEntry> Load(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                throw new FileNotFoundException($"Commodity economy config not found: {jsonPath}");
            }

            var json = File.ReadAllText(jsonPath);
            var config = JsonSerializer.Deserialize<CommodityEconomyConfig>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (config == null || config.Commodities.Count == 0)
            {
                throw new InvalidOperationException("Commodity economy config contains no commodities.");
            }

            var dictionary = new Dictionary<string, CommodityEconomyEntry>(StringComparer.Ordinal);
            foreach (var entry in config.Commodities)
            {
                var code = entry.Code?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(code))
                {
                    throw new InvalidOperationException("Commodity economy config entry has empty code.");
                }

                if (dictionary.ContainsKey(code))
                {
                    throw new InvalidOperationException($"Duplicate commodity economy code: {code}");
                }

                dictionary[code] = entry;
            }

            return dictionary;
        }
    }
}
