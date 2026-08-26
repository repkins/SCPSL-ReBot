using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableToRoomEnterLocation(BotRef botRef) : IMovableToTarget
{
    MovableTo<RoomEnterLocation> movableToRoomEnterLocation = new(botRef, new(botRef));

    public bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        return movableToRoomEnterLocation.GoesTo(targetLocationIdx, impactingBeliefs);
    }

    public bool MovesTo(int targetLocationIdx)
    {
        return movableToRoomEnterLocation.MovesTo(targetLocationIdx);
    }
}

internal struct RoomEnterLocation(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.RoomState.RoomEnterLocations;

    public readonly BeliefEnum MovedToEnum => BeliefEnum.MovedToRoomEnterLocation;
}
