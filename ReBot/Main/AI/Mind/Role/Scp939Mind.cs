using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.OvercomeObstacle;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp939Mind(BotRef botRef) : IMind
{
    TerminateEnemyEnabling terminateEnemyEnabling = new(botRef);
    MeleeEnemyEnabling meleeEnemyEnabling = new(botRef);
    MoveTo<Enemy>.Enabling moveToEnemyEnabling = new(botRef, new(botRef));
    SearchesEnemyEnabling searchesEnemyEnabling = new(botRef);

    MoveTo<Room>.Enabling moveToRoomEnabling = new(botRef, new(botRef));

    OvercomeObstacleEnabling overcomeObstacleEnabling = new();

    InteractObstacleEnabling interactObstacleEnabling = new(botRef);
    MoveTo<Obstacle>.Enabling moveToObstacleEnabling = new(botRef, new());

    TravelOnElevatorEnabling travelOnElevatorEnabling = new(botRef);
    MoveTo<Elevator>.Enabling moveToElevatorEnabling = new(botRef, new());

    CallElevatorEnabling callElevatorEnabling = new(botRef);
    MoveTo<ElevatorCall>.Enabling moveToElevatorCallEnabling = new(botRef, new());

    public void ProcessGoals(NativeList<ImpactingBelief> impactingBeliefs)
    {
        impactingBeliefs.Add(0, BeliefEnum.Terminated | BeliefEnum.Enemy, default);
    }

    public bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return beliefToEnable.Type switch
        {
            BeliefEnum.Terminated | BeliefEnum.Enemy =>
                terminateEnemyEnabling.Process(impactingBeliefs),

            BeliefEnum.Attacked | BeliefEnum.Enemy =>
                meleeEnemyEnabling.Process(new(beliefToEnable.Idx), impactingBeliefs),

            BeliefEnum.MovedTo | BeliefEnum.Enemy  =>
                moveToEnemyEnabling.Process(beliefToEnable.Idx),

            BeliefEnum.Sighted | BeliefEnum.Enemy =>
                searchesEnemyEnabling.Process(impactingBeliefs),

            BeliefEnum.MovedTo | BeliefEnum.Room =>
                moveToRoomEnabling.Process(beliefToEnable.Idx),

            BeliefEnum.Overcame | BeliefEnum.Obstacle =>
                overcomeObstacleEnabling.Process(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.InteractingWith | BeliefEnum.Obstacle =>
                interactObstacleEnabling.Process(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedTo | BeliefEnum.Obstacle =>
                moveToObstacleEnabling.Process(beliefToEnable.Idx),

            BeliefEnum.TravellingOn | BeliefEnum.Elevator =>
                travelOnElevatorEnabling.Process(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedTo | BeliefEnum.Elevator =>
                moveToElevatorEnabling.Process(beliefToEnable.Idx),

            BeliefEnum.Called | BeliefEnum.Elevator =>
                callElevatorEnabling.Process(beliefToEnable.Idx, impactingBeliefs),

            BeliefEnum.MovedTo | BeliefEnum.ElevatorCall =>
                moveToElevatorCallEnabling.Process(beliefToEnable.Idx),

            _ => false
        };
    }

    public struct Factory : IMind.IFactory<Scp939Mind>
    {
        public readonly Scp939Mind Create(BotRef botRef)
        {
            return new(botRef);
        }
    }
}
