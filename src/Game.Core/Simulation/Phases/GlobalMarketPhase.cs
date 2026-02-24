using Game.Core.Core.Market;

namespace Game.Core.Simulation.Phases
{
    public sealed class GlobalMarketPhase : IPhase
    {
        private readonly GlobalMarketService _market;

        public GlobalMarketPhase(GlobalMarketService market)
        {
            _market = market;
        }

        public PhaseId Id => PhaseId.Phase4MarketSolve;

        public void Execute(PhaseContext context)
        {
            _market.UpdatePrices(context.Tick);
            _market.Prices.CopyTo(context.State.GlobalMarketPrices);
            _market.Prices.CopyTo(context.Market.GlobalPrices);
        }
    }
}
