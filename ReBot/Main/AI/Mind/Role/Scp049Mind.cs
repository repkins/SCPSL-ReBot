using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

public struct Scp049Mind(BotRef botRef) : IMind
{
    CardiacableEnemy cardiacableEnemy = new(botRef);
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
        var actionFound = false;

        switch (beliefToEnable.Type)
        {
            case BeliefEnum.EnemiesTerminated:
                cardiacableEnemy.Terminates(impactingBeliefs);
                break;

            case BeliefEnum.AttackEnemy:
                actionFound = cardiacableEnemy.Attacks(new(beliefToEnable.Idx), impactingBeliefs);
                break;

            case BeliefEnum.WithinEnemyLocation:
                actionFound = cardiacableEnemy.MovesTo(new(beliefToEnable.Idx));
                break;

            case BeliefEnum.SightedEnemies:
                enemy.Searches(impactingBeliefs);
                break;

            case BeliefEnum.WithinRoomEnterLocation:
                actionFound = room.MovesToEnterLocation(beliefToEnable.Idx);
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
                actionFound = interactableObstacle.MovesTo(beliefToEnable.Idx);
                break;

            case BeliefEnum.TravelingOnElevator:
                actionFound = elevationTravelObstacle.Travels(beliefToEnable.Idx, impactingBeliefs);
                break;

            case BeliefEnum.InElevator:
                actionFound = elevationTravelObstacle.MovesToElevator(beliefToEnable.Idx);
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
