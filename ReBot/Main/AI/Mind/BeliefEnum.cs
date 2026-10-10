namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    TerminatedEnemy,
    WithinEnemy,
    SightedEnemies,

    UnobstructingObstacle,

    WithinRoom,
    WithinObstacle,
    WithinElevator,
    WithinElevatorCall,

    Calmed,
    Enraged,
}