using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Mind.Abilities.Obstacle;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp106Mind(BotRef botRef) : IMind
{
    CapturableEnemy capturableEnemy = new(botRef);
    Enemy enemy = new(botRef);

    MovableTo<Room.EnterLocation> movableToRoom = new(botRef, new(botRef));

    InteractableObstacle interactableObstacle = new(botRef);
    MovableTo<InteractableObstacle.Location> movableToInteractableObstacle = new(botRef, new(botRef));

    PassthroughObstacle passthroughObstacle = new(botRef);

    ElevationTravelObstacle elevationTravelObstacle = new(botRef);
    MovableTo<ElevationTravelObstacle.EnterLocation> movableToElevationTravelObstacle = new(botRef, new(botRef));

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
                capturableEnemy.Terminates(impactingBeliefs),

            BeliefEnum.AttackedEnemy => 
                capturableEnemy.Attacks(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.MovedToEnemyLocation => 
                capturableEnemy.MovesTo(new(beliefToEnable.Idx)),

            BeliefEnum.SightedEnemies => 
                enemy.SearchesInRoom(impactingBeliefs),

            BeliefEnum.WithinRoom =>
                movableToRoom.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToRoomEnterLocation => 
                movableToRoom.MovesTo(beliefToEnable.Idx),

            BeliefEnum.ObstacleToOvercome =>
                elevationTravelObstacle.TryGetElevationIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationIdx)
                    ? elevationTravelObstacle.Overcomes(elevationIdx, impactingBeliefs)
                    : elevationCallObstacle.TryGetElevationCallIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationCallIdx)
                        ? elevationCallObstacle.Overcomes(new(beliefToEnable.Idx), elevationCallIdx, impactingBeliefs)
                        : interactableObstacle.Overcomes(new(beliefToEnable.Idx), impactingBeliefs)
                            || passthroughObstacle.Overcomes(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.PassingThroughObstacle => 
                passthroughObstacle.PassesThrough(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToObstacleLocation =>
                passthroughObstacle.MovesLooksTo(beliefToEnable.Idx),

            BeliefEnum.InteractingWithObstacle => 
                interactableObstacle.Interacts(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.WithinObstacleInteractionLocation =>
                movableToInteractableObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToObstacleInteractionLocation =>
                movableToInteractableObstacle.MovesTo(beliefToEnable.Idx),

            BeliefEnum.TravelingOnElevator => 
                elevationTravelObstacle.Travels(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.WithinElevator =>
                movableToElevationTravelObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToElevator =>
                movableToElevationTravelObstacle.MovesTo(beliefToEnable.Idx),

            BeliefEnum.ElevatorCalled => 
                elevationCallObstacle.Calls(beliefToEnable.Idx, impactingBeliefs),

            _ => false
        };
    }

    public struct Factory : IMind.IFactory<Scp106Mind>
    {
        public readonly Scp106Mind Create(BotRef botRef)
        {
            return new Scp106Mind(botRef);
        }
    }
}
