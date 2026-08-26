using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.Blink;
using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Mind.Abilities.Obstacle;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp173Mind(BotRef botRef) : IMind
{
    SnappableEnemy snappableEnemy = new(botRef);
    Enemy enemy = new(botRef);

    Unobserved<MovableToRoomEnterLocation> unobservedMovableToRoom = new(botRef, new(botRef));
    ObservedBlinkableToRoom observedBlinkableToRoom = new(botRef);

    InteractableObstacle interactableObstacle = new(botRef);
    Unobserved<MovableToInteractableObstacleLocation> unobservedMovableToInteractableObstacle = new(botRef, new(botRef));
    ObservedBlinkableToInteractableObstacle observedBlinkableToInteractableObstacle = new(botRef);

    ElevationTravelObstacle elevationTravelObstacle = new(botRef);
    Unobserved<MovableToElevationTravelObstacleEnterLocation> unobservedMovableToElevationTravelObstacle = new(botRef, new(botRef));
    ObservedBlinkableToElevationTravelObstacle observedBlinkableToElevationTravelObstacle = new(botRef);

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
                unobservedMovableToRoom.GoesTo(beliefToEnable.Idx, impactingBeliefs)
                    || observedBlinkableToRoom.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToRoomEnterLocation =>
                unobservedMovableToRoom.MovesTo(beliefToEnable.Idx),

            BeliefEnum.BlinkedToRoomEnterLocation =>
                observedBlinkableToRoom.BlinksToEnterLocation(beliefToEnable.Idx),

            BeliefEnum.ObstacleToOvercome =>
                elevationTravelObstacle.TryGetElevationIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationIdx)
                    ? elevationTravelObstacle.Overcomes(elevationIdx, impactingBeliefs)
                    : elevationCallObstacle.TryGetElevationCallIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationCallIdx)
                        ? elevationCallObstacle.Overcomes(new(beliefToEnable.Idx), elevationCallIdx, impactingBeliefs)
                        : interactableObstacle.Overcomes(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.InteractingWithObstacle => 
                interactableObstacle.Interacts(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.WithinObstacleInteractionLocation =>
                unobservedMovableToInteractableObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs)
                    || observedBlinkableToInteractableObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToObstacleInteractionLocation => 
                unobservedMovableToInteractableObstacle.MovesTo(beliefToEnable.Idx),

            BeliefEnum.BlinkedToObstacleInteractionLocation =>
                observedBlinkableToInteractableObstacle.BlinksTo(beliefToEnable.Idx),

            BeliefEnum.TravelingOnElevator => 
                elevationTravelObstacle.Travels(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.WithinElevator =>
                unobservedMovableToElevationTravelObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs)
                    || observedBlinkableToElevationTravelObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedToElevator =>
                unobservedMovableToElevationTravelObstacle.MovesTo(beliefToEnable.Idx),

            BeliefEnum.BlinkedToElevator =>
                observedBlinkableToElevationTravelObstacle.BlinksTo(beliefToEnable.Idx),

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
