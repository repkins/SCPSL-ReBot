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
    Elevator = 0x30,
    ElevatorCall = 0x40,
    Enemy = 0x50,
}
