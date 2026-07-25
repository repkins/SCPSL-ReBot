namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToOvercome,
    WithinObstacleInteractionLocation,

    WithinRoomEnterLocation,
    InteractingWithObstacle,
    TravelingOnElevator,
    InElevator,
    ElevatorCalled,

    TargetsTerminated,
    SightedEnemies,
    WithinEnemyLocation,
    CardiacEnemy,
}
