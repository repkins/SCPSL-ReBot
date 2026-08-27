namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToOvercome,
    WithinObstacleInteractionLocation,
    MovedToObstacleInteractionLocation,
    BlinkedToObstacleInteractionLocation,
    MovedToObstacleLocation,

    InteractingWithObstacle,
    PassingThroughObstacle,
    TravelingOnElevator,
    WithinElevator,
    MovedToElevator,
    BlinkedToElevator,
    ElevatorCalled,
    WithinElevatorCallLocation,
    MovedToElevatorCallLocation,
    BlinkedToElevatorCallLocation,

    WithinRoom,
    MovedToRoomEnterLocation,
    BlinkedToRoomEnterLocation,

    EnemiesTerminated,
    SightedEnemies,
    WithinEnemyLocation,
    MovedToEnemyLocation,
    AttackedEnemy,
    BlinkedToEnemy,
    Enraged,
    Calmed,
}
