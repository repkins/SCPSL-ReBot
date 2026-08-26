using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Blink;

internal struct ObservedBlinkableToRoom(BotRef botRef)
{
    bool isObserved = botRef.Value.BlinkState.IsObserved;
    float remainingSecondsToBlink = botRef.Value.BlinkState.RemainingSecondsToBlink;

    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

    public bool GoesTo(int roomEnterLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (isObserved)
        {
            var roomEnterLocationPosition = roomEnterLocations[roomEnterLocationIdx];

            if (!obstaclesByTarget.TryGetValue(roomEnterLocationPosition, out var obstacleRef))
            {
                var cost = remainingSecondsToBlink;

                impactingBeliefs.Add(cost, BeliefEnum.BlinkedToRoomEnterLocation, roomEnterLocationIdx);
            }
            else
            {
                var cost = Vector3.Distance(roomEnterLocationPosition, obstacleLocations[obstacleRef.Idx]);

                impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
            }
        }        

        return false;
    }

    public bool BlinksToEnterLocation(int roomEnterLocationIdx)
    {
        var targetPosition = roomEnterLocations[roomEnterLocationIdx];
        blinkToPositionRef.Value = (true, targetPosition);

        return true;
    }
}
