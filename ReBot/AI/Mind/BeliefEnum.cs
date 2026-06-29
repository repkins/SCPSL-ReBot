namespace ReBot.AI.Mind;

public enum BeliefEnum : byte
{
    Default,

    WithinTarget,
    InteractingWithTarget,

    ObstacleTargetToRemove,
    SightedEnemies,

    TargetsTerminated,
    EnemyWithinDistance,
    CardiacTarget,
}
