using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.Terminate;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

public struct Scp049Mind(BotRef botRef) : IMind
{
    TerminateEnemies terminateEnemies = new(botRef);
    CardiacEnemy cardiacEnemy = new(botRef);
    MoveTo<Enemy> moveToEnemy = new(botRef, new(botRef));
    SearchesEnemy searchesEnemy = new(botRef);

    MoveTo<Room> moveToRoom = new(botRef, new(botRef));

    InteractObstacle interactObstacle = new(botRef);
    MoveTo<Obstacle> moveToObstacle = new(botRef, new());

    TravelOnElevator travelOnElevator = new(botRef);
    MoveTo<Elevator> moveToElevator = new(botRef, new());

    CallElevator callElevator = new(botRef);
    MoveTo<ElevatorCall> moveToElevatorCall = new(botRef, new());

    public void ProcessGoals(NativeList<BeliefRef> beliefsToEnable)
    {
        terminateEnemies.Enabling(beliefsToEnable);
    }

    public bool ProcessImpactingAction(ActionRef action, NativeList<BeliefRef> beliefsToEnable)
    {
        return action.Enum switch
        {
            ActionEnum.Attacks | ActionEnum.Enemy =>
                cardiacEnemy.Enabling(new(action.Idx), beliefsToEnable),

            ActionEnum.MovesTo | ActionEnum.Enemy =>
                moveToEnemy.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.Searches | ActionEnum.Room =>
                searchesEnemy.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.MovesTo | ActionEnum.Room =>
                moveToRoom.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.InteractsWith | ActionEnum.Obstacle =>
                interactObstacle.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.MovesTo | ActionEnum.Obstacle =>
                moveToObstacle.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.TravelsOn | ActionEnum.Elevator =>
                travelOnElevator.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.MovesTo | ActionEnum.Elevator =>
                moveToElevator.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.Calls | ActionEnum.Elevator =>
                callElevator.Enabling(action.Idx, beliefsToEnable),

            ActionEnum.MovesTo | ActionEnum.ElevatorCall =>
                moveToElevatorCall.Enabling(action.Idx, beliefsToEnable),

            _ => false
        };
    }

    public bool ProcessEnablingBelief(BeliefRef beliefRef, NativeList<ImpactingAction> impactingActions)
    {
        return beliefRef.Enum switch
        {
            BeliefEnum.TerminatedEnemy =>
                cardiacEnemy.Impacting(new(beliefRef.Idx), impactingActions),

            BeliefEnum.WithinEnemy => 
                moveToEnemy.Impacting(beliefRef.Idx, impactingActions),

            BeliefEnum.SightedEnemies =>
                searchesEnemy.Impacting(impactingActions),

            BeliefEnum.WithinRoom => 
                moveToRoom.Impacting(beliefRef.Idx, impactingActions),

            BeliefEnum.UnobstructingObstacle =>
                travelOnElevator.TryGetElevationIdx(new ObstacleRef(beliefRef.Idx), out var elevationIdx)
                    ? travelOnElevator.Impacting(elevationIdx, impactingActions)
                    : callElevator.TryGetElevationCallIdx(new ObstacleRef(beliefRef.Idx), out var elevationCallIdx)
                        ? callElevator.Impacting(elevationCallIdx, impactingActions)
                        : interactObstacle.Impacting(new(beliefRef.Idx), impactingActions),

            BeliefEnum.WithinObstacle => 
                moveToObstacle.Impacting(beliefRef.Idx, impactingActions),

            BeliefEnum.WithinElevator => 
                moveToElevator.Impacting(beliefRef.Idx, impactingActions),

            BeliefEnum.WithinElevatorCall => 
                moveToElevatorCall.Impacting(beliefRef.Idx, impactingActions),

            _ => false
        };
    }

    public struct Factory : IMind.IFactory<Scp049Mind>
    {
        public readonly Scp049Mind Create(BotRef botRef)
        {
            return new Scp049Mind(botRef);
        }
    }
}
