namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToOvercome,
    WithinObstacleInteractionLocation,
    WithinObstacleLocation,

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
