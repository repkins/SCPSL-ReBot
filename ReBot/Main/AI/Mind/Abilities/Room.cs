using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Room(BotRef botRef)
{
    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;
    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool MovesToEnterLocation(int roomEnterLocationIdx)
    {
        var targetPosition = roomEnterLocations[roomEnterLocationIdx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}
