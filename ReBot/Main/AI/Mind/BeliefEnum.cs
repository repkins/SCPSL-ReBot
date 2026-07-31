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

    EnemiesTerminated,
    SightedEnemies,
    WithinEnemyLocation,
    AttackEnemy,
}
