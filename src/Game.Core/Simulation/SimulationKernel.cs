using System.Collections.Generic;

namespace Game.Core.Simulation
{
    public sealed class SimulationKernel
    {
        private readonly List<IPhase> _phases = new List<IPhase>();
        private readonly State.SimulationState _state;
        private readonly State.LocalBuffers _buffers;
        private readonly State.MarketAggregation _aggregation;
        private readonly State.MarketState _market;
        private readonly List<Determinism.StateSnapshot> _snapshots = new List<Determinism.StateSnapshot>();
        private int _tick;

        public int Tick => _tick;
        public IReadOnlyList<Determinism.StateSnapshot> Snapshots => _snapshots;

        public SimulationKernel(State.SimulationState state, State.LocalBuffers buffers, State.MarketAggregation aggregation, State.MarketState market)
        {
            _state = state;
            _buffers = buffers;
            _aggregation = aggregation;
            _market = market;
        }

        public void AddPhase(IPhase phase)
        {
            _phases.Add(phase);
        }

        public void TickOnce()
        {
            _buffers.Clear();
            _aggregation.Clear();
            _market.Clear();

            var context = new PhaseContext(_tick, _state, _buffers, _aggregation, _market);
            foreach (var phase in _phases)
            {
                phase.Execute(context);
            }

            CaptureSnapshot();

            _tick++;
        }

        private void CaptureSnapshot()
        {
            var productionHash = Determinism.DeterminismHasher.Hash(_state.Production);
            var demandHash = Determinism.DeterminismHasher.Hash(_state.Demand);
            var pricesHash = Determinism.DeterminismHasher.Hash(_state.GlobalMarketPrices);
            var capacityHash = Determinism.DeterminismHasher.Hash(_state.Capacity);
            var efficiencyHash = Determinism.DeterminismHasher.Hash(_state.Efficiency);
            var inventoryHash = Determinism.DeterminismHasher.Hash(_state.Inventory);
            var localPricesHash = Determinism.DeterminismHasher.Hash(_state.LocalPrices);
            var elasticityHash = Determinism.DeterminismHasher.Hash(_state.DemandElasticity);
            var priceSensitivityHash = Determinism.DeterminismHasher.Hash(_state.PriceSensitivity);
            var priceDominanceHash = Determinism.DeterminismHasher.Hash(_state.PriceDominance);

            var combined = Determinism.DeterminismHasher.Combine(productionHash, demandHash);
            combined = Determinism.DeterminismHasher.Combine(combined, pricesHash);
            combined = Determinism.DeterminismHasher.Combine(combined, capacityHash);
            combined = Determinism.DeterminismHasher.Combine(combined, efficiencyHash);
            combined = Determinism.DeterminismHasher.Combine(combined, inventoryHash);
            combined = Determinism.DeterminismHasher.Combine(combined, localPricesHash);
            combined = Determinism.DeterminismHasher.Combine(combined, elasticityHash);
            combined = Determinism.DeterminismHasher.Combine(combined, priceSensitivityHash);
            combined = Determinism.DeterminismHasher.Combine(combined, priceDominanceHash);

            _snapshots.Add(new Determinism.StateSnapshot(_tick, combined));
        }
    }
}
