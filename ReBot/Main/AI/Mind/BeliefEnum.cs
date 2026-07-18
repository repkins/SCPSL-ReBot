namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToOvercome,
    WithinObstacleLocation,

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
