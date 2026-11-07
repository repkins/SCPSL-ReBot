using System;

namespace ReBot.Main.AI.Decision;

[Flags]
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

    Attacks = 0x08,
    Searches = 0x09,
    Enrages = 0x0A,
    Calms = 0x0B,

    Room = 0x10,
    Obstacle = 0x20,
    EmergencyRelease = 0x30,
    Elevator = 0x40,
    ElevatorCall = 0x50,
    Enemy = 0x60,
}
