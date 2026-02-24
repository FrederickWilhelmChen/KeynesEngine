namespace Game.Core.Simulation
{
    public interface IPhase
    {
        PhaseId Id { get; }
        void Execute(PhaseContext context);
    }
}
