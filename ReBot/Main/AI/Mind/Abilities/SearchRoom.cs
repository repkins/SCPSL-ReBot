using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SearchRoom(BotRef botRef)
{
    NativeHashMap<int, float> roomLastVisitTime = botRef.Value.RoomState.RoomLastVisitTime;

    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;
    NativeList<int> roomInstanceIds = botRef.Value.RoomState.RoomInstanceIds;

    public void Impacting(NativeList<ImpactingAction> impactingActions)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomIdx = 0; roomIdx < roomEnterLocations.Length; roomIdx++)
            {
                var cost = 0f;
                var multiplier = 10f; // cost multiplier is odds of seeing enemies
                multiplier += roomLastVisitTime.TryGetValue(roomInstanceIds[roomIdx], out var time)
                    ? 1f / ((Time.time - time) / 300f) // 300 secs = 5 mins
                    : 0f;

                impactingActions.Add(cost, multiplier, ActionEnum.Searches | ActionEnum.Room, roomIdx);
            }
        }
    }

    public void Enabling(int roomEnterLocationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        beliefsToEnable.Add(new(BeliefEnum.WithinRoom, roomEnterLocationIdx));
    }
}
