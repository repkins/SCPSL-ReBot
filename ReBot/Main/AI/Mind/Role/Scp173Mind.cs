using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.Blink;
using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp173Mind(BotRef botRef) : IMind
{
    ObservablePlayer observablePlayer = new(botRef);

    SnappableEnemy snappableEnemy = new(botRef);
    Enemy enemy = new(botRef);

    MovableToRoom movableToRoom = new(botRef);
    BlinkableToRoom blinkableToRoom = new(botRef);

    InteractableObstacle interactableObstacle = new(botRef);
    MovableToInteractableObstacle movableToInteractableObstacle = new(botRef);
    BlinkableToInteractableObstacle blinkableToInteractableObstacle = new(botRef);

    ElevationTravelObstacle elevationTravelObstacle = new(botRef);
    MovableToElevationTravelObstacle movableToElevationTravelObstacle = new(botRef);
    BlinkableToElevationTravelObstacle blinkableToElevationTravelObstacle = new(botRef);

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

            BeliefEnum.BlinkedToEnemy =>
                snappableEnemy.BlinksTo(new(beliefToEnable.Idx)),

            BeliefEnum.SightedEnemies => 
                enemy.SearchesInRoom(impactingBeliefs),

            BeliefEnum.WithinRoom =>
                (!observablePlayer.isObserved 
                    && movableToRoom.GoesTo(beliefToEnable.Idx, impactingBeliefs))
                || (observablePlayer.isObserved 
                    && blinkableToRoom.GoesTo(beliefToEnable.Idx, impactingBeliefs)),

            BeliefEnum.MovedToRoomEnterLocation =>
                movableToRoom.MovesToEnterLocation(beliefToEnable.Idx),

            BeliefEnum.BlinkedToRoomEnterLocation =>
                blinkableToRoom.BlinksToEnterLocation(beliefToEnable.Idx),

            BeliefEnum.ObstacleToOvercome =>
                elevationTravelObstacle.TryGetElevationIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationIdx)
                    ? elevationTravelObstacle.Overcomes(elevationIdx, impactingBeliefs)
                    : elevationCallObstacle.TryGetElevationCallIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationCallIdx)
                        ? elevationCallObstacle.Overcomes(new(beliefToEnable.Idx), elevationCallIdx, impactingBeliefs)
                        : interactableObstacle.Overcomes(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.InteractingWithObstacle => 
                interactableObstacle.Interacts(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.WithinObstacleInteractionLocation =>
                (!observablePlayer.isObserved
                    && movableToInteractableObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs))
                || (observablePlayer.isObserved
                    && blinkableToInteractableObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs)),

            BeliefEnum.MovedToObstacleInteractionLocation => 
                movableToInteractableObstacle.MovesTo(beliefToEnable.Idx),

            BeliefEnum.BlinkedToObstacleInteractionLocation =>
                blinkableToInteractableObstacle.BlinksTo(beliefToEnable.Idx),

            BeliefEnum.TravelingOnElevator => 
                elevationTravelObstacle.Travels(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.WithinElevator =>
                (!observablePlayer.isObserved
                    && movableToElevationTravelObstacle.GoesToElevator(beliefToEnable.Idx, impactingBeliefs))
                || (observablePlayer.isObserved 
                    && blinkableToElevationTravelObstacle.GoesToElevator(beliefToEnable.Idx, impactingBeliefs)),

            BeliefEnum.MovedToElevator =>
                movableToElevationTravelObstacle.MovesToElevator(beliefToEnable.Idx),

            BeliefEnum.BlinkedToElevator =>
                blinkableToElevationTravelObstacle.BlinksToElevator(beliefToEnable.Idx),

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
