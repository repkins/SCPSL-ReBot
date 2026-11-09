using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities;

internal struct SearchRoom(BotRef botRef)
{
    NativeHashMap<int, float> roomLastVisitTime = botRef.Value.RoomStore.RoomLastVisitTime;

    NativeList<int> roomEnterInstanceIds = botRef.Value.RoomStore.RoomEnterInstanceIds;
    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomStore.RoomEnterLocations;
    NativeList<Vector3> roomExitLocations = botRef.Value.RoomStore.RoomExitLocations;
    NativeList<bool> roomExitIsWithins = botRef.Value.RoomStore.RoomExitIsWithins;
    NativeList<bool> roomIsElevations = botRef.Value.RoomStore.RoomIsElevations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleStore.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementStore.MoveToPositionRef;

    public void Impacting(NativeList<ImpactingAction> impactingActions)
    {
        for (var roomEnterIdx = 0; roomEnterIdx < roomEnterInstanceIds.Length; roomEnterIdx++)
        {
            var roomEnterPosition = roomEnterLocations[roomEnterIdx];

            var cost = 0f;
            if (!roomIsElevations[roomEnterIdx])
            {
                cost += !obstaclesByTarget.TryGetValue(roomEnterPosition, out var obstacleRef)
                    ? Vector3.Distance(roomEnterPosition, roomExitLocations[roomEnterIdx])
                    : Vector3.Distance(roomEnterPosition, obstacleLocations[obstacleRef.Idx]);
            }
            else
            {
                cost += !obstaclesByTarget.TryGetValue(roomEnterPosition, out var obstacleRef)
                    ? 5f
                    : 0f;
            }

            var multiplier = 10f; // cost multiplier is odds of seeing enemies
            multiplier += roomLastVisitTime.TryGetValue(roomEnterInstanceIds[roomEnterIdx], out var time)
                ? 1f / ((Time.time - time) / 300f) // 300 secs = 5 mins
                : 0f;

            impactingActions.Add(cost, multiplier, ActionEnum.Searches | ActionEnum.Room, roomEnterIdx);
        }
    }

    public void Enabling(int roomEnterIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        if (roomExitIsWithins[roomEnterIdx])
        {
            var roomEnterPosition = roomEnterLocations[roomEnterIdx];
            if (!obstaclesByTarget.TryGetValue(roomEnterPosition, out var obstacleRef))
            {
                moveToPositionRef.Value = (true, roomEnterPosition);
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacleRef.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinRoom, roomEnterIdx));
        }
    }
}
