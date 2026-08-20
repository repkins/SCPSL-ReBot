namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToOvercome,
    MovedToObstacleInteractionLocation,
    MovedToObstacleLocation,

    WithinRoom,
    MovedToRoomEnterLocation,
    InteractingWithObstacle,
    PassingThroughObstacle,
    TravelingOnElevator,
    MovedToElevator,
    ElevatorCalled,

    EnemiesTerminated,
    SightedEnemies,
    MovedToEnemyLocation,
    AttackedEnemy,
}
