using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Enemy(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    public bool Searches(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < roomEnterLocations.Length; roomEnterLocationIdx++)
            {
                var roomEnterLocationPosition = roomEnterLocations[roomEnterLocationIdx];

                var multiplier = 10f; // multiplier is odds of seeing enemies

                if (!obstaclesByTarget.TryGetValue(roomEnterLocationPosition, out var obstacleRef))
                {
                    var cost = Vector3.Distance(roomEnterLocationPosition, playerPosition);

                    impactingBeliefs.Add(cost, multiplier, BeliefEnum.WithinRoomEnterLocation, roomEnterLocationIdx);
                }
                else
                {
                    var cost = Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                    impactingBeliefs.Add(cost, multiplier, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
                }
            }
        }

        return false;
    }
}
