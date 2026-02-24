using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Game.Core.Core.Market;
using Game.Core.Core.Economy;
using Game.Core.Simulation;
using Game.Core.Simulation.Phases;
using Game.Core.Simulation.State;

Console.WriteLine("=================================");
Console.WriteLine("  凯恩斯经济模拟引擎");
Console.WriteLine("  Keynes Economic Simulation Engine");
Console.WriteLine("=================================");
Console.WriteLine();

var rootPath = FindRepoRoot(AppContext.BaseDirectory);
var configPath = Path.Combine(rootPath, "src", "Game.Core", "Data", "global_market.json");
var marketData = GlobalMarketLoader.Load(configPath);
var economyConfigPath = Path.Combine(rootPath, "src", "Game.Core", "Data", "commodities.json");
var economyConfig = CommodityEconomyLoader.Load(economyConfigPath);
var state = new SimulationState(1, marketData.Catalog.Definitions.Count);
var buffers = new LocalBuffers(state.ProvinceCount, state.CommodityCount);
var aggregation = new MarketAggregation(state.CommodityCount);
var market = new MarketState(state.CommodityCount);
var kernel = new SimulationKernel(state, buffers, aggregation, market);

SeedState(state, marketData.Catalog.Mapping.Codes, economyConfig);
foreach (var phase in PhaseSchedule.CreateBaseline(marketData.Service))
{
    kernel.AddPhase(phase);
}

var outputPath = PrepareOutputPath();
var csv = new StringBuilder();
csv.AppendLine("Tick,Commodity,GlobalPrice,NationalPrice,Production,Demand,Inventory");

var ticks = 1000;
for (var i = 0; i < ticks; i++)
{
    kernel.TickOnce();
    WriteTickRow(csv, kernel.Tick, state, market, marketData.Catalog.Mapping.Codes);
}

File.WriteAllText(outputPath, csv.ToString());

Console.WriteLine("Engine demo completed!");
Console.WriteLine($"CSV written: {outputPath}");
Console.WriteLine();
Console.WriteLine("Determinism hashes:");
foreach (var snapshot in kernel.Snapshots)
{
    Console.WriteLine($"Tick {snapshot.Tick}: {snapshot.Hash}");
}

static void WriteTickRow(StringBuilder csv, int tick, SimulationState state, MarketState market, IReadOnlyList<string> codes)
{
    var commodityCount = state.CommodityCount;
    var provinceCount = state.ProvinceCount;
    for (var commodityId = 0; commodityId < commodityCount; commodityId++)
    {
        float production = 0f;
        float demand = 0f;
        float inventory = 0f;
        for (var provinceId = 0; provinceId < provinceCount; provinceId++)
        {
            var index = provinceId * commodityCount + commodityId;
            production += state.Production[index];
            demand += state.Demand[index];
            inventory += state.Inventory[index];
        }

        csv.Append(tick.ToString(CultureInfo.InvariantCulture));
        csv.Append(',');
        csv.Append(codes[commodityId]);
        csv.Append(',');
        csv.Append(state.GlobalMarketPrices[commodityId].ToString(CultureInfo.InvariantCulture));
        csv.Append(',');
        csv.Append(market.NationalPrices[commodityId].ToString(CultureInfo.InvariantCulture));
        csv.Append(',');
        csv.Append(production.ToString(CultureInfo.InvariantCulture));
        csv.Append(',');
        csv.Append(demand.ToString(CultureInfo.InvariantCulture));
        csv.Append(',');
        csv.Append(inventory.ToString(CultureInfo.InvariantCulture));
        csv.AppendLine();
    }
}

static string PrepareOutputPath()
{
    var root = FindRepoRoot(AppContext.BaseDirectory);
    var outputDir = Path.Combine(root, "out");
    Directory.CreateDirectory(outputDir);
    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
    return Path.Combine(outputDir, $"tick_log_{timestamp}.csv");
}

static void SeedState(SimulationState state, IReadOnlyList<string> codes, IReadOnlyDictionary<string, CommodityEconomyEntry> economyConfig)
{
    var total = state.ProvinceCount * state.CommodityCount;
    for (var index = 0; index < total; index++)
    {
        state.Capacity[index] = 100f;
        state.Efficiency[index] = 0.8f;
        state.Inventory[index] = 20f;
        state.LocalPrices[index] = 0f;
    }

    for (var commodityId = 0; commodityId < state.CommodityCount; commodityId++)
    {
        var code = codes[commodityId];
        if (!economyConfig.TryGetValue(code, out var entry))
        {
            throw new InvalidOperationException($"Missing economy config for {code}.");
        }

        state.DemandElasticity[commodityId] = entry.DemandElasticity;
        state.PriceSensitivity[commodityId] = entry.PriceSensitivity;
        state.PriceDominance[commodityId] = ParseDominance(entry.PriceDominance);
    }
}

static int ParseDominance(string value)
{
    if (string.Equals(value, "Supply", StringComparison.OrdinalIgnoreCase))
    {
        return -1;
    }

    return 1;
}

static string FindRepoRoot(string startPath)
{
    var directory = new DirectoryInfo(startPath);
    while (directory != null)
    {
        var readmePath = Path.Combine(directory.FullName, "README.md");
        if (File.Exists(readmePath))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new DirectoryNotFoundException("Repository root not found.");
}
