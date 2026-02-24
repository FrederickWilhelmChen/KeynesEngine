using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Core.Core.Ids
{
    public sealed class CommodityCatalog
    {
        public IdMapping Mapping { get; }
        public IReadOnlyList<CommodityDefinition> Definitions { get; }

        private CommodityCatalog(IdMapping mapping, List<CommodityDefinition> definitions)
        {
            Mapping = mapping;
            Definitions = definitions;
        }

        public static CommodityCatalog Create(IEnumerable<CommodityDefinition> definitions)
        {
            var list = definitions.ToList();
            if (list.Count == 0)
            {
                throw new InvalidOperationException("No commodity definitions provided.");
            }

            var mapping = IdMapping.Create(list.Select(definition => definition.Code));

            var ordered = new CommodityDefinition[list.Count];
            foreach (var definition in list)
            {
                var id = mapping.CodeToId[definition.Code];
                ordered[id] = definition;
            }

            return new CommodityCatalog(mapping, ordered.ToList());
        }
    }

    public sealed class CommodityDefinition
    {
        public string Code { get; }
        public float BasePrice { get; }
        public float ShockAmplitude { get; }
        public int ShockPeriod { get; }
        public float NoiseAmplitude { get; }

        public CommodityDefinition(string code, float basePrice, float shockAmplitude, int shockPeriod, float noiseAmplitude)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Code is required.", nameof(code));
            }

            if (basePrice <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(basePrice));
            }

            if (shockPeriod <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shockPeriod));
            }

            if (shockAmplitude < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(shockAmplitude));
            }

            if (noiseAmplitude < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(noiseAmplitude));
            }

            Code = code.Trim();
            BasePrice = basePrice;
            ShockAmplitude = shockAmplitude;
            ShockPeriod = shockPeriod;
            NoiseAmplitude = noiseAmplitude;
        }
    }
}
