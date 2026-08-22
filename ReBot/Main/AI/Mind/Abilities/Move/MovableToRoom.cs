using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableToRoom(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool GoesTo(int roomEnterLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var roomEnterLocationPosition = roomEnterLocations[roomEnterLocationIdx];

        if (!obstaclesByTarget.TryGetValue(roomEnterLocationPosition, out var obstacleRef))
        {
            var cost = Vector3.Distance(roomEnterLocationPosition, playerPosition);

            impactingBeliefs.Add(cost, BeliefEnum.MovedToRoomEnterLocation, roomEnterLocationIdx);
        }
        else
        {
            var cost = Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
        }

        return false;
    }

    public bool MovesToEnterLocation(int roomEnterLocationIdx)
    {
        var targetPosition = roomEnterLocations[roomEnterLocationIdx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}
