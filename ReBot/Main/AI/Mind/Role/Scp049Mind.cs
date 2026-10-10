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
    SearchRoom searchRoom = new(botRef);

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

    public void ProcessImpactingAction(ActionRef action, NativeList<BeliefRef> beliefsToEnable)
    {
        switch (action.Enum)
        {
            case ActionEnum.Attacks | ActionEnum.Enemy:
                cardiacEnemy.Enabling(new(action.Idx), beliefsToEnable);
                
                break;
            case ActionEnum.MovesTo | ActionEnum.Enemy:
                moveToEnemy.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.Searches | ActionEnum.Room:
                searchRoom.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.MovesTo | ActionEnum.Room:
                moveToRoom.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.InteractsWith | ActionEnum.Obstacle:
                interactObstacle.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.MovesTo | ActionEnum.Obstacle:
                moveToObstacle.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.TravelsOn | ActionEnum.Elevator:
                travelOnElevator.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.MovesTo | ActionEnum.Elevator:
                moveToElevator.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.Calls | ActionEnum.Elevator:
                callElevator.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.MovesTo | ActionEnum.ElevatorCall:
                moveToElevatorCall.Enabling(action.Idx, beliefsToEnable);
                
                break;                
        };
    }

    public void ProcessEnablingBelief(BeliefRef beliefRef, NativeList<ImpactingAction> impactingActions)
    {
        switch (beliefRef.Enum)
        {
            case BeliefEnum.TerminatedEnemy:
                cardiacEnemy.Impacting(new(beliefRef.Idx), impactingActions);

                break;
            case BeliefEnum.WithinEnemy: 
                moveToEnemy.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.SightedEnemies:
                searchRoom.Impacting(impactingActions);

                break;
            case BeliefEnum.WithinRoom: 
                moveToRoom.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.UnobstructingObstacle:
                if (travelOnElevator.TryGetElevationIdx(new ObstacleRef(beliefRef.Idx), out var elevationIdx))
                {
                    travelOnElevator.Impacting(elevationIdx, impactingActions);
                }
                else if (callElevator.TryGetElevationCallIdx(new ObstacleRef(beliefRef.Idx), out var elevationCallIdx))
                {
                    callElevator.Impacting(elevationCallIdx, impactingActions);
                }
                else
                {
                    interactObstacle.Impacting(new(beliefRef.Idx), impactingActions);
                }

                break;
            case BeliefEnum.WithinObstacle: 
                moveToObstacle.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.WithinElevator: 
                moveToElevator.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.WithinElevatorCall: 
                moveToElevatorCall.Impacting(beliefRef.Idx, impactingActions);

                break;
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
