using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move.Location;

internal struct RoomEnterLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.RoomState.RoomEnterLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToRoomEnterLocation;
}

