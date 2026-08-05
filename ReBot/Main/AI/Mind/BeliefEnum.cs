namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToOvercome,
    WithinObstacleInteractionLocation,
    WithinObstacleLocation,

    WithinRoom,
    WithinRoomEnterLocation,
    InteractingWithObstacle,
    PassingThroughObstacle,
    TravelingOnElevator,
    InElevator,
    ElevatorCalled,

    EnemiesTerminated,
    SightedEnemies,
    WithinEnemyLocation,
    AttackEnemy,
}
