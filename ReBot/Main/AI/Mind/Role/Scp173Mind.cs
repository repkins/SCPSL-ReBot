using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp173Mind(BotRef botRef) : IMind
{
    SnappableEnemy snappableEnemy = new(botRef);
    Enemy enemy = new(botRef);
    Room room = new(botRef);
    InteractableObstacle interactableObstacle = new(botRef);
    ElevationTravelObstacle elevationTravelObstacle = new(botRef);
    ElevationCallObstacle elevationCallObstacle = new(botRef);

    public void ProcessGoals(NativeList<ImpactingBelief> impactingBeliefs)
    {
        impactingBeliefs.Add(0, BeliefEnum.EnemiesTerminated, default);
    }

    public bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return beliefToEnable.Type switch
        {
            BeliefEnum.EnemiesTerminated =>
                snappableEnemy.Terminates(impactingBeliefs),

            BeliefEnum.AttackedEnemy =>
                snappableEnemy.Attacks(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.MovedToEnemyLocation =>
                snappableEnemy.MovesTo(new(beliefToEnable.Idx)),

            BeliefEnum.SightedEnemies => 
                enemy.SearchesInRoom(impactingBeliefs),

            BeliefEnum.WithinRoom =>
                room.MovesToRoom(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToRoomEnterLocation => 
                room.MovesToEnterLocation(beliefToEnable.Idx),

            BeliefEnum.ObstacleToOvercome =>
                elevationTravelObstacle.TryGetElevationIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationIdx)
                    ? elevationTravelObstacle.Overcomes(elevationIdx, impactingBeliefs)
                    : elevationCallObstacle.TryGetElevationCallIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationCallIdx)
                        ? elevationCallObstacle.Overcomes(new(beliefToEnable.Idx), elevationCallIdx, impactingBeliefs)
                        : interactableObstacle.Overcomes(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.InteractingWithObstacle => 
                interactableObstacle.Interacts(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToObstacleInteractionLocation => 
                interactableObstacle.MovesTo(beliefToEnable.Idx),

            BeliefEnum.TravelingOnElevator => 
                elevationTravelObstacle.Travels(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToElevator => 
                elevationTravelObstacle.MovesToElevator(beliefToEnable.Idx),

            BeliefEnum.ElevatorCalled => 
                elevationCallObstacle.Calls(beliefToEnable.Idx, impactingBeliefs),

            _ => false
        };
    }

    public struct Factory : IMind.IFactory<Scp173Mind>
    {
        public readonly Scp173Mind Create(BotRef botRef)
        {
            return new Scp173Mind(botRef);
        }
    }
}
