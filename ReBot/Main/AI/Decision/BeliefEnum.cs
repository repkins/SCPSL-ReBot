namespace ReBot.Main.AI.Decision;

internal enum BeliefEnum : byte
{
    TerminatedEnemy,
    WithinEnemy,
    SightedEnemies,

    UnobstructingObstacle,

    WithinRoom,
    WithinObstacle,
    WithinEmergencyRelease,
    WithinElevator,
    WithinElevatorCall,

    Calmed,
    Enraged,
}