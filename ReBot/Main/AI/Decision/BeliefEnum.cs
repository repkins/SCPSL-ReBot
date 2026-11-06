namespace ReBot.Main.AI.Decision;

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