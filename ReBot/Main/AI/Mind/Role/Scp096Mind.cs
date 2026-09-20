using ReBot.Main.AI.Mind.Abilities;
using ReBot.Main.AI.Mind.Abilities.CallElevator;
using ReBot.Main.AI.Mind.Abilities.GoTo;
using ReBot.Main.AI.Mind.Abilities.Interact;
using ReBot.Main.AI.Mind.Abilities.MoveTo;
using ReBot.Main.AI.Mind.Abilities.OvercomeObstacle;
using ReBot.Main.AI.Mind.Abilities.Rage;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Mind.Abilities.Terminate.Scp096;
using ReBot.Main.AI.Mind.Abilities.TravelOnElevator;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp096Mind(BotRef botRef) : IMind
{
    EnrageEnabling enrageEnabling = new(botRef);
    CalmEnabling calmEnabling = new(botRef);


    TerminateEnemyEnabling terminateEnemyEnabling = new(botRef);
    MeleeEnemyEnabling meleeEnemyEnabling = new(botRef);
    MoveTo<Enemy>.Enabling moveToEnemyEnabling = new(botRef, new(botRef));
    SearchesEnemyEnabling<GoTo<Enemy>.Impacting> searchesEnemyEnabling = new(botRef, new(botRef, new(botRef)));

    MoveTo<Room>.Enabling moveToRoomEnabling = new(botRef, new(botRef));

    OvercomeObstacleEnabling overcomeObstacleEnabling = new();

    InteractObstacleEnabling<GoTo<Obstacle>.Impacting> interactObstacleEnabling = new(botRef, new(botRef, new()));
    MoveTo<Obstacle>.Enabling moveToObstacleEnabling = new(botRef, new());

    TravelOnElevatorEnabling<GoTo<Elevator>.Impacting> travelOnElevatorEnabling = new(botRef, new(botRef, new()));
    MoveTo<Elevator>.Enabling moveToElevatorEnabling = new(botRef, new());

    CallElevatorEnabling<GoTo<ElevatorCall>.Impacting> callElevatorEnabling = new(botRef, new(botRef, new()));
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

            BeliefEnum.MovedTo | BeliefEnum.Enemy =>
                moveToEnemyEnabling.Process(beliefToEnable.Idx),

            BeliefEnum.Enraged =>
                enrageEnabling.Process(),

            BeliefEnum.Calmed =>
                calmEnabling.Process(),

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

    public struct Factory : IMind.IFactory<Scp096Mind>
    {
        public readonly Scp096Mind Create(BotRef botRef)
        {
            return new Scp096Mind(botRef);
        }
    }
}
