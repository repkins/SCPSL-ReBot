using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Mind.Abilities.Move.Location;
using ReBot.Main.AI.Mind.Abilities.Obstacle;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp096Mind(BotRef botRef) : IMind
{
    Enemy enemy = new(botRef);
    Scp096MeleeableEnemy meleeableEnemy = new(botRef);

    MovableTo<MoveRoomEnterLocation> movableToRoom = new(botRef, new(botRef));

    InteractableObstacle interactableObstacle = new(botRef);
    MovableTo<MoveInteractableObstacleLocation> movableToInteractableObstacle = new(botRef, new());

    PassthroughObstacle passthroughObstacle = new(botRef);

    ElevationTravelObstacle elevationTravelObstacle = new(botRef);
    MovableTo<MoveElevationTravelObstacleEnterLocation> movableToElevationTravelObstacle = new(botRef, new());

    ElevationCallObstacle elevationCallObstacle = new(botRef);
    MovableTo<MoveElevationCallObstacleLocation> movableToElevationCallObstacle = new(botRef, new());

    public void ProcessGoals(NativeList<ImpactingBelief> impactingBeliefs)
    {
        impactingBeliefs.Add(0, BeliefEnum.EnemiesTerminated, default);
    }

    public bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return beliefToEnable.Type switch
        {
            BeliefEnum.EnemiesTerminated =>
                meleeableEnemy.Terminates(impactingBeliefs),

            BeliefEnum.AttackedEnemy =>
                meleeableEnemy.Attacks(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.MovedToEnemyLocation =>
                meleeableEnemy.MovesTo(new(beliefToEnable.Idx)),

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

            BeliefEnum.WithinElevatorCallLocation =>
                movableToElevationCallObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToElevatorCallLocation =>
                movableToElevationCallObstacle.MovesTo(beliefToEnable.Idx),

            _ => false
        };
    }

    public struct Factory : IMind.IFactory<Scp096Mind>
    {
        public readonly Scp096Mind Create(BotRef botRef)
        {
            return new Scp096Mind(botRef);
        }
    }
}
