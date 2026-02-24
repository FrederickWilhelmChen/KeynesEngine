namespace Game.Core.Simulation
{
    public sealed class PhaseContext
    {
        public int Tick { get; }
        public State.SimulationState State { get; }
        public State.LocalBuffers Buffers { get; }
        public State.MarketAggregation Aggregation { get; }
        public State.MarketState Market { get; }

        public PhaseContext(int tick, State.SimulationState state, State.LocalBuffers buffers, State.MarketAggregation aggregation, State.MarketState market)
        {
            Tick = tick;
            State = state;
            Buffers = buffers;
            Aggregation = aggregation;
            Market = market;
        }
    }
}
