namespace Game.Core.Simulation.Phases
{
    public sealed class AggregationPhase : IPhase
    {
        public PhaseId Id => PhaseId.Phase3Aggregation;

        public void Execute(PhaseContext context)
        {
            var state = context.State;
            var aggregation = context.Aggregation;
            var commodityCount = state.CommodityCount;
            var total = state.ProvinceCount * commodityCount;

            for (var index = 0; index < total; index++)
            {
                var commodityId = index % commodityCount;
                aggregation.NationalSupply[commodityId] += state.Production[index];
                aggregation.NationalDemand[commodityId] += state.Demand[index];
                aggregation.GlobalSupply[commodityId] += state.Production[index];
                aggregation.GlobalDemand[commodityId] += state.Demand[index];
            }
        }
    }
}
