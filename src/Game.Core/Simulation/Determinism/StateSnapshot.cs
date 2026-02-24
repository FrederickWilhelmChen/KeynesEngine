namespace Game.Core.Simulation.Determinism
{
    public sealed class StateSnapshot
    {
        public int Tick { get; }
        public ulong Hash { get; }

        public StateSnapshot(int tick, ulong hash)
        {
            Tick = tick;
            Hash = hash;
        }
    }
}
