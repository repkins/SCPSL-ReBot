namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    ObstacleToRemove,
    WithinObstacleLocation,

    WithinRoomEnterLocation,
    InteractingWithObstacle,

    TargetsTerminated,
    SightedEnemies,
    WithinEnemyLocation,
    CardiacEnemy,
}
