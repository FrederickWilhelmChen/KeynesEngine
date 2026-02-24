using System.Collections.Generic;
using Game.Core.Core.Market;

namespace Game.Core.Simulation.Phases
{
    public static class PhaseSchedule
    {
        public static IEnumerable<IPhase> CreateBaseline(GlobalMarketService market)
        {
            return new IPhase[]
            {
                new LocalUpdatePhase(),
                new NoOpPhase(PhaseId.Phase2Logistics),
                new AggregationPhase(),
                new GlobalMarketPhase(market),
                new MarketSolvePhase(),
                new NoOpPhase(PhaseId.Phase5IncomeDistribution),
                new NoOpPhase(PhaseId.Phase6PoliticalUpdate),
                new NoOpPhase(PhaseId.Phase7Government),
                new NoOpPhase(PhaseId.Phase8AIDecision),
                new NoOpPhase(PhaseId.Phase9DecisionApply)
            };
        }
    }
}
