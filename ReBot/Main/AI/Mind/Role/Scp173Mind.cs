using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.BlinkTo;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.GoTo.Scp173;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.OvercomeObstacle;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.Terminate.Scp173;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp173Mind(BotRef botRef) : IMind
{
    TerminateEnemyEnabling terminateEnemyEnabling = new(botRef);
    SnapEnemyEnabling snapEnemyEnabling = new(botRef);
    SearchesEnemyEnabling<GoTo<Room>.Impacting> searchesEnemyEnabling = new(botRef, new(botRef, new(botRef)));

    MoveTo<Enemy>.Enabling moveToEnemyEnabling = new(botRef, new(botRef));
    BlinkTo<Enemy>.Enabling blinkToEnemyEnabling = new(botRef, new(botRef));

    MoveTo<Room>.Enabling moveToRoomEnabling = new(botRef, new(botRef));
    BlinkTo<Room>.Enabling blinkToRoomEnabling = new(botRef, new(botRef));

    OvercomeObstacleEnabling overcomeObstacleEnabling = new();

    InteractObstacleEnabling<GoTo<Obstacle>.Impacting> interactObstacleEnabling = new(botRef, new(botRef, new()));
    MoveTo<Obstacle>.Enabling moveToObstacleEnabling = new(botRef, new());
    BlinkTo<Obstacle>.Enabling blinkToObstacleEnabling = new(botRef, new());

    TravelOnElevatorEnabling<GoTo<Elevator>.Impacting> travelOnElevatorEnabling = new(botRef, new(botRef, new()));
    MoveTo<Elevator>.Enabling moveToElevatorEnabling = new(botRef, new());
    BlinkTo<Elevator>.Enabling blinkToElevatorEnabling = new(botRef, new());

    CallElevatorEnabling<GoTo<ElevatorCall>.Impacting> callElevatorEnabling = new(botRef, new(botRef, new()));
    MoveTo<ElevatorCall>.Enabling moveToElevatorCallEnabling = new(botRef, new());
    BlinkTo<ElevatorCall>.Enabling blinkToElevatorCallEnabling = new(botRef, new());

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
                snapEnemyEnabling.Process(new(actionToEnable.Idx), impactingActions),

            ActionEnum.MovesTo | ActionEnum.Enemy =>
                moveToEnemyEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Enemy =>
                blinkToEnemyEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Finds | ActionEnum.Enemy =>
                searchesEnemyEnabling.Process(impactingActions),

            ActionEnum.MovesTo | ActionEnum.Room =>
                moveToRoomEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Room =>
                blinkToRoomEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Overcomes | ActionEnum.Obstacle =>
                overcomeObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.InteractsWith | ActionEnum.Obstacle =>
                interactObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.Obstacle =>
                moveToObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Obstacle =>
                blinkToObstacleEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.TravelsOn | ActionEnum.Elevator =>
                travelOnElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.Elevator =>
                moveToElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Elevator =>
                blinkToElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Calls | ActionEnum.Elevator =>
                callElevatorEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.ElevatorCall =>
                moveToElevatorCallEnabling.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.ElevatorCall =>
                blinkToElevatorCallEnabling.Process(actionToEnable.Idx, impactingActions),

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
