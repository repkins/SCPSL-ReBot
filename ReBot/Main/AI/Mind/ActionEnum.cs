namespace ReBot.Main.AI.Mind;

public enum ActionEnum : byte
{
    Default = 0x00,

    MovesTo = 0x01,
    BlinksTo = 0x02,

    Overcomes = 0x03,

    InteractsWith = 0x04,
    PassesThrough = 0x05,
    TravelsOn = 0x06,
    Calls = 0x07,

    Terminates = 0x08,
    Attacks = 0x09,
    Finds = 0x0A,
    Enrages = 0x0B,
    Calms = 0x0C,

    Searches = 0x0D,

    Room = 0x10,
    Obstacle = 0x20,
    Elevator = 0x30,
    ElevatorCall = 0x40,
    Enemy = 0x50,
}
