namespace ReBot.Main.AI.Mind;

public enum BeliefEnum : byte
{
    Default = 0x00,

    MovedTo,
    BlinkedTo,

    Overcame,

    InteractingWith,
    PassingThrough,
    TravellingOn,
    Called,

    Terminated,
    Attacked,
    Sighted,
    Enraged,
    Calmed,

    Room = 0x10,
    Obstacle = 0x20,
    ObstacleInteractionLocation = 0x30,
    Elevator = 0x40,
    ElevatorCall = 0x50,
    Enemy = 0x60,
}
