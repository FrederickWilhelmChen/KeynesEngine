using Game.Core.Simulation.State;

namespace Game.Core.Simulation.Phases
{
    public sealed class LocalUpdatePhase : IPhase
    {
        public PhaseId Id => PhaseId.Phase1LocalUpdate;

        public void Execute(PhaseContext context)
        {
            var state = context.State;
            var buffers = context.Buffers;
            var total = state.ProvinceCount * state.CommodityCount;

            var commodityCount = state.CommodityCount;
            for (var index = 0; index < total; index++)
            {
                var commodityId = index % commodityCount;
                var capacity = state.Capacity[index];
                var efficiency = state.Efficiency[index];
                var baseProduction = capacity * efficiency;
                var price = state.LocalPrices[index] <= 0f
                    ? state.GlobalMarketPrices[commodityId]
                    : state.LocalPrices[index];
                var elasticity = state.DemandElasticity[commodityId];
                var demandScale = 1f - elasticity * ((price / state.GlobalMarketPrices[commodityId]) - 1f);
                if (demandScale < 0.5f) demandScale = 0.5f;
                if (demandScale > 1.5f) demandScale = 1.5f;

                var production = baseProduction;
                var demand = baseProduction * 0.95f * demandScale;

                buffers.Production[index] = production;
                buffers.Demand[index] = demand;
                buffers.Inventory[index] = state.Inventory[index] + production - demand;
            }

            MergeBuffers(state, buffers);
        }

        private static void MergeBuffers(SimulationState state, LocalBuffers buffers)
        {
            var total = state.ProvinceCount * state.CommodityCount;
            for (var index = 0; index < total; index++)
            {
                state.Production[index] = buffers.Production[index];
                state.Demand[index] = buffers.Demand[index];
                state.Inventory[index] = buffers.Inventory[index];
            }
        }
    }
}
