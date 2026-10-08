using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.GoTo;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.OvercomeObstacle.Scp106;
using ReBot.Main.AI.Mind.Abilities.Passthrough;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.Terminate;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp106Mind(BotRef botRef) : IMind
{
    TerminateEnemyEnabling terminateEnemyEnabling = new(botRef);
    CaptureEnemyEnabling captureEnemyEnabling = new(botRef);
    MoveTo<Enemy>.Enabling moveToEnemyEnabling = new(botRef, new(botRef));
    SearchesEnemyEnabling<GoTo<Enemy>.Impacting> searchesEnemyEnabling = new(botRef, new(botRef, new(botRef)));

    MoveTo<Room>.Enabling moveToRoomEnabling = new(botRef, new(botRef));

    OvercomeObstacleEnabling overcomeObstacleEnabling = new(botRef);

    InteractObstacleEnabling<GoTo<Obstacle>.Impacting> interactObstacleEnabling = new(botRef, new(botRef, new()));
    MoveTo<Obstacle>.Enabling moveToObstacleEnabling = new(botRef, new());

    PassthroughObstacleEnabling passthroughObstacleEnabling = new(botRef);

    TravelOnElevatorEnabling<GoTo<Elevator>.Impacting> travelOnElevatorEnabling = new(botRef, new(botRef, new()));
    MoveTo<Elevator>.Enabling moveToElevatorEnabling = new(botRef, new());

    CallElevatorEnabling<GoTo<ElevatorCall>.Impacting> callElevatorEnabling = new(botRef, new(botRef, new()));
    MoveTo<ElevatorCall>.Enabling moveToElevatorCallEnabling = new(botRef, new());

    public void ProcessGoals(NativeList<ImpactingAction> impactingActions)
    {
        impactingActions.Add(0, ActionEnum.Terminates | ActionEnum.Enemy, default);
    }

    public bool ProcessActionToEnable(ActionRef actionToEnable, NativeList<ImpactingAction> impactingActions)
    {
        return actionToEnable.Enum switch
        {
            ActionEnum.Terminates | ActionEnum.Enemy =>
                terminateEnemyEnabling.Process(impactingActions),

            ActionEnum.Attacks | ActionEnum.Enemy =>
                captureEnemyEnabling.Process(new(actionToEnable.Idx), impactingActions),

            ActionEnum.MovesTo | ActionEnum.Enemy =>
                moveToEnemyEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Finds | ActionEnum.Enemy =>
                searchesEnemyEnabling.Process(impactingActions),

            ActionEnum.MovesTo | ActionEnum.Room =>
                moveToRoomEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Overcomes | ActionEnum.Obstacle =>
                overcomeObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.InteractsWith | ActionEnum.Obstacle =>
                interactObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.Obstacle =>
                moveToObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.PassesThrough | ActionEnum.Obstacle =>
                passthroughObstacleEnabling.Process(actionToEnable.Idx),

            ActionEnum.TravelsOn | ActionEnum.Elevator =>
                travelOnElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.Elevator =>
                moveToElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Calls | ActionEnum.Elevator =>
                callElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.ElevatorCall =>
                moveToElevatorCallEnabling.Process(actionToEnable.Idx, impactingActions),

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
