namespace Game.Core.Simulation.Phases
{
    public sealed class NoOpPhase : IPhase
    {
        public PhaseId Id { get; }

        public NoOpPhase(PhaseId id)
        {
            Id = id;
        }

        public void Execute(PhaseContext context)
        {
        }
    }
}
