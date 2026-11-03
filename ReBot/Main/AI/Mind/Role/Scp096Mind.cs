using Interactables.Interobjects.DoorUtils;
using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.Break;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Rage;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.Terminate.Scp096;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp096Mind(BotRef botRef) : IMind
{
    Enrage enrage = new(botRef);
    Calm calm = new(botRef);

    TerminateEnemies terminateEnemies = new(botRef);
    MeleeEnemy meleeEnemy = new(botRef);
    MoveTo<Enemy> moveToEnemy = new(botRef, new(botRef));
    SearchRoom searchRoom = new(botRef);

    MoveTo<Room> moveToRoom = new(botRef, new(botRef));

    InteractObstacle interactObstacle = new(botRef, DoorPermissionFlags.ScpOverride);
    BreakObstacle breakObstacle = new(botRef);
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
                meleeEnemy.Enabling(new(action.Idx), beliefsToEnable);
                
                break;
            case ActionEnum.MovesTo | ActionEnum.Enemy:
                moveToEnemy.Enabling(action.Idx, beliefsToEnable);
                
                break;
            case ActionEnum.Enrages:
                enrage.Enabling();
                
                break;
            case ActionEnum.Calms:
                calm.Enabling();
                
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

            case ActionEnum.Attacks | ActionEnum.Obstacle:
                breakObstacle.Enabling(action.Idx, beliefsToEnable);

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
            case BeliefEnum.Enraged:
                enrage.Impacting(impactingActions);

                break;
            case BeliefEnum.Calmed:
                calm.Impacting(impactingActions);

                break;
            case BeliefEnum.TerminatedEnemy:
                meleeEnemy.Impacting(new(beliefRef.Idx), impactingActions);

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
                    breakObstacle.Impacting(new(beliefRef.Idx), impactingActions);
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

    public struct Factory : IMind.IFactory<Scp096Mind>
    {
        public readonly Scp096Mind Create(BotRef botRef)
        {
            return new Scp096Mind(botRef);
        }
    }
}
