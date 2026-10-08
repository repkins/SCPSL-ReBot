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
    TerminateEnemy terminateEnemy = new(botRef);
    SnapEnemy snapEnemy = new(botRef);
    SearchesEnemy<Within<Room>> searchesEnemy = new(botRef, new(botRef, new(botRef)));

    MoveTo<Enemy> moveToEnemy = new(botRef, new(botRef));
    BlinkTo<Enemy> blinkToEnemy = new(botRef, new(botRef));

    MoveTo<Room> moveToRoom = new(botRef, new(botRef));
    BlinkTo<Room> blinkToRoom = new(botRef, new(botRef));

    OvercomeObstacle overcomeObstacle = new();

    InteractObstacle<Within<Obstacle>> interactObstacle = new(botRef, new(botRef, new()));
    MoveTo<Obstacle> moveToObstacle = new(botRef, new());
    BlinkTo<Obstacle> blinkToObstacle = new(botRef, new());

    TravelOnElevator<Within<Elevator>> travelOnElevator = new(botRef, new(botRef, new()));
    MoveTo<Elevator> moveToElevator = new(botRef, new());
    BlinkTo<Elevator> blinkToElevator = new(botRef, new());

    CallElevator<Within<ElevatorCall>> callElevator = new(botRef, new(botRef, new()));
    MoveTo<ElevatorCall> moveToElevatorCall = new(botRef, new());
    BlinkTo<ElevatorCall> blinkToElevatorCall = new(botRef, new());

    public void ProcessGoals(NativeList<ImpactingAction> impactingActions)
    {
        impactingActions.Add(0, ActionEnum.Terminates | ActionEnum.Enemy, default);
    }

    public bool ProcessActionToEnable(ActionRef actionToEnable, NativeList<ImpactingAction> impactingActions)
    {
        return actionToEnable.Enum switch
        {
            ActionEnum.Terminates | ActionEnum.Enemy =>
                terminateEnemy.Process(impactingActions),

            ActionEnum.Attacks | ActionEnum.Enemy =>
                snapEnemy.Process(new(actionToEnable.Idx), impactingActions),

            ActionEnum.MovesTo | ActionEnum.Enemy =>
                moveToEnemy.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Enemy =>
                blinkToEnemy.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Finds | ActionEnum.Enemy =>
                searchesEnemy.Process(impactingActions),

            ActionEnum.MovesTo | ActionEnum.Room =>
                moveToRoom.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Room =>
                blinkToRoom.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Overcomes | ActionEnum.Obstacle =>
                overcomeObstacle.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.InteractsWith | ActionEnum.Obstacle =>
                interactObstacle.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.Obstacle =>
                moveToObstacle.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Obstacle =>
                blinkToObstacle.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.TravelsOn | ActionEnum.Elevator =>
                travelOnElevator.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.Elevator =>
                moveToElevator.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.Elevator =>
                blinkToElevator.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.Calls | ActionEnum.Elevator =>
                callElevator.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.MovesTo | ActionEnum.ElevatorCall =>
                moveToElevatorCall.Process(actionToEnable.Idx, impactingActions),

            ActionEnum.BlinksTo | ActionEnum.ElevatorCall =>
                blinkToElevatorCall.Process(actionToEnable.Idx, impactingActions),

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
