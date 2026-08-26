using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

public struct Scp049Mind(BotRef botRef) : IMind
{
    CardiacableEnemy cardiacableEnemy = new(botRef);
    Enemy enemy = new(botRef);
    MovableToRoom room = new(botRef);

    InteractableObstacle interactableObstacle = new(botRef);
    MovableToInteractableObstacle movableToInteractableObstacle = new(botRef);

    ElevationTravelObstacle elevationTravelObstacle = new(botRef);
    MovableToElevationTravelObstacle movableToElevationTravelObstacle = new(botRef);

    ElevationCallObstacle elevationCallObstacle = new(botRef);

    public void ProcessGoals(NativeList<ImpactingBelief> impactingBeliefs)
    {
        impactingBeliefs.Add(0, BeliefEnum.EnemiesTerminated, default);
    }

    public bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var actionFound = false;

        switch (beliefToEnable.Type)
        {
            case BeliefEnum.EnemiesTerminated:
                cardiacableEnemy.Terminates(impactingBeliefs);
                break;

            case BeliefEnum.AttackedEnemy:
                actionFound = cardiacableEnemy.Attacks(new(beliefToEnable.Idx), impactingBeliefs);
                break;

            case BeliefEnum.MovedToEnemyLocation:
                actionFound = cardiacableEnemy.MovesTo(new(beliefToEnable.Idx));
                break;

            case BeliefEnum.SightedEnemies:
                enemy.SearchesInRoom(impactingBeliefs);
                break;

            case BeliefEnum.WithinRoom:
                actionFound = room.GoesTo(beliefToEnable.Idx, impactingBeliefs);
                break;

            case BeliefEnum.MovedToRoomEnterLocation:
                actionFound = room.MovesTo(beliefToEnable.Idx);
                break;

            case BeliefEnum.ObstacleToOvercome:
                if (elevationTravelObstacle.TryGetElevationIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationIdx))
                {
                    elevationTravelObstacle.Overcomes(elevationIdx, impactingBeliefs);
                }
                else if (elevationCallObstacle.TryGetElevationCallIdx(new ObstacleRef(beliefToEnable.Idx), out var elevationCallIdx))
                {
                    elevationCallObstacle.Overcomes(new(beliefToEnable.Idx), elevationCallIdx, impactingBeliefs);
                }
                else
                {
                    interactableObstacle.Overcomes(new(beliefToEnable.Idx), impactingBeliefs);
                }
                break;

            case BeliefEnum.InteractingWithObstacle:
                actionFound = interactableObstacle.Interacts(beliefToEnable.Idx, impactingBeliefs);
                break;

            case BeliefEnum.WithinObstacleInteractionLocation:
                actionFound = movableToInteractableObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs);
                break;

            case BeliefEnum.MovedToObstacleInteractionLocation:
                actionFound = movableToInteractableObstacle.MovesTo(beliefToEnable.Idx);
                break;

            case BeliefEnum.TravelingOnElevator:
                actionFound = elevationTravelObstacle.Travels(beliefToEnable.Idx, impactingBeliefs);
                break;

            case BeliefEnum.WithinElevator:
                actionFound = movableToElevationTravelObstacle.GoesTo(beliefToEnable.Idx, impactingBeliefs);
                break;

            case BeliefEnum.MovedToElevator:
                actionFound = movableToElevationTravelObstacle.MovesTo(beliefToEnable.Idx);
                break;

            case BeliefEnum.ElevatorCalled:
                actionFound = elevationCallObstacle.Calls(beliefToEnable.Idx, impactingBeliefs);
                break;
        }

        return actionFound;
    }

    public struct Factory : IMind.IFactory<Scp049Mind>
    {
        public readonly Scp049Mind Create(BotRef botRef)
        {
            return new Scp049Mind(botRef);
        }
    }
}
