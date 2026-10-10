using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.BlinkTo;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Observing;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.Terminate.Scp173;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using ReBot.Main.AI.Spacial;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp173Mind(BotRef botRef) : IMind
{
    TerminateEnemies terminateEnemies = new(botRef);
    SnapEnemy snapEnemy = new(botRef);
    SearchRoom searchRoom = new(botRef);

    Unobserved<MoveTo<Enemy>> unobservedMoveToEnemy = new(botRef, new(botRef, new(botRef)));
    ObservedBlinkTo<Enemy> observedBlinkToEnemy = new(botRef, new(botRef));

    Unobserved<MoveTo<Room>> unobservedMoveToRoom = new(botRef, new(botRef, new(botRef)));
    ObservedBlinkTo<Room> observedBlinkToRoom = new(botRef, new(botRef));

    InteractObstacle interactObstacle = new(botRef);
    Unobserved<MoveTo<Obstacle>> unobservedMoveToObstacle = new(botRef, new(botRef, new()));
    ObservedBlinkTo<Obstacle> observedBlinkToObstacle = new(botRef, new());

    TravelOnElevator travelOnElevator = new(botRef);
    Unobserved<MoveTo<Elevator>> unobservedMoveToElevator = new(botRef, new(botRef, new()));
    ObservedBlinkTo<Elevator> observedBlinkToElevator = new(botRef, new());

    CallElevator callElevator = new(botRef);
    Unobserved<MoveTo<ElevatorCall>> unobservedMoveToElevatorCall = new(botRef, new(botRef, new()));
    ObservedBlinkTo<ElevatorCall> observedBlinkToElevatorCall = new(botRef, new());

    public void ProcessGoals(NativeList<BeliefRef> beliefsToEnable)
    {
        terminateEnemies.Enabling(beliefsToEnable);
    }

    public void ProcessImpactingAction(ActionRef action, NativeList<BeliefRef> beliefsToEnable)
    {
        switch (action.Enum)
        {
            case ActionEnum.Attacks | ActionEnum.Enemy:
                snapEnemy.Enabling(new(action.Idx), beliefsToEnable);

                break;
            case ActionEnum.MovesTo | ActionEnum.Enemy:
                unobservedMoveToEnemy.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.BlinksTo | ActionEnum.Enemy:
                observedBlinkToEnemy.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.Searches | ActionEnum.Room:
                searchRoom.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.MovesTo | ActionEnum.Room:
                unobservedMoveToRoom.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.BlinksTo | ActionEnum.Room:
                observedBlinkToRoom.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.InteractsWith | ActionEnum.Obstacle:
                interactObstacle.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.MovesTo | ActionEnum.Obstacle:
                unobservedMoveToObstacle.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.BlinksTo | ActionEnum.Obstacle:
                observedBlinkToObstacle.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.TravelsOn | ActionEnum.Elevator:
                travelOnElevator.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.MovesTo | ActionEnum.Elevator:
                unobservedMoveToElevator.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.BlinksTo | ActionEnum.Elevator:
                observedBlinkToElevator.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.Calls | ActionEnum.Elevator:
                callElevator.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.MovesTo | ActionEnum.ElevatorCall:
                unobservedMoveToElevatorCall.Enabling(action.Idx, beliefsToEnable);

                break;
            case ActionEnum.BlinksTo | ActionEnum.ElevatorCall:
                observedBlinkToElevatorCall.Enabling(action.Idx, beliefsToEnable);

                break;
        };
    }

    public void ProcessEnablingBelief(BeliefRef beliefRef, NativeList<ImpactingAction> impactingActions)
    {
        switch (beliefRef.Enum)
        {
            case BeliefEnum.TerminatedEnemy:
                snapEnemy.Impacting(new(beliefRef.Idx), impactingActions);

                break;
            case BeliefEnum.WithinEnemy:
                unobservedMoveToEnemy.Impacting(beliefRef.Idx, impactingActions);
                observedBlinkToEnemy.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.SightedEnemies:
                searchRoom.Impacting(impactingActions);

                break;
            case BeliefEnum.WithinRoom:
                unobservedMoveToRoom.Impacting(beliefRef.Idx, impactingActions);
                observedBlinkToRoom.Impacting(beliefRef.Idx, impactingActions);

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
                unobservedMoveToObstacle.Impacting(beliefRef.Idx, impactingActions);
                observedBlinkToObstacle.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.WithinElevator:
                unobservedMoveToElevator.Impacting(beliefRef.Idx, impactingActions);
                observedBlinkToElevator.Impacting(beliefRef.Idx, impactingActions);

                break;
            case BeliefEnum.WithinElevatorCall:
                unobservedMoveToElevatorCall.Impacting(beliefRef.Idx, impactingActions);
                observedBlinkToElevatorCall.Impacting(beliefRef.Idx, impactingActions);

                break;
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
