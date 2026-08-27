using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink.Location;

internal struct BlinkRoomEnterLocation(BotRef botRef) : IBlinkTarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.RoomState.RoomEnterLocations;

    public readonly BeliefEnum BlinkedToEnum => BeliefEnum.BlinkedToRoomEnterLocation;
}

