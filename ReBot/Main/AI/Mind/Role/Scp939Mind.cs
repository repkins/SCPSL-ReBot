using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.CallsElevator;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.Overcome;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.TravelOn;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp939Mind(BotRef botRef) : IMind
{
    TerminateEnemyEnabling terminateEnemyEnabling = new(botRef);
    MeleeEnemyEnabling meleeEnemyEnabling = new(botRef);
    MoveToEnabling<Enemy> moveToEnemyEnabling = new(botRef, new(botRef));
    SearchesEnemyEnabling searchesEnemyEnabling = new(botRef);

    MoveToEnabling<Room> moveToRoomEnabling = new(botRef, new(botRef));

    OvercomeEnablingObstacle overcomeObstacleEnabling = new(botRef);

    InteractEnablingObstacle interactObstacleEnabling = new(botRef);
    MoveToEnabling<Obstacle> moveToObstacleEnabling = new(botRef, new());

    TravelOnEnablingElevator travelOnElevatorEnabling = new(botRef);
    MoveToEnabling<Elevator> moveToElevatorEnabling = new(botRef, new());

    CallEnablingElevator callElevatorEnabling = new(botRef);
    MoveToEnabling<ElevatorCall> moveToElevatorCallEnabling = new(botRef, new());

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
