using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SearchRoom(BotRef botRef)
{
    NativeHashMap<int, float> roomLastVisitTime = botRef.Value.RoomState.RoomLastVisitTime;

    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;
    NativeList<int> roomEnterInstanceIds = botRef.Value.RoomState.RoomEnterInstanceIds;

    public void Impacting(NativeList<ImpactingAction> impactingActions)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomEnterIdx = 0; roomEnterIdx < roomEnterLocations.Length; roomEnterIdx++)
            {
                var cost = 0f;
                var multiplier = 10f; // cost multiplier is odds of seeing enemies
                multiplier += roomLastVisitTime.TryGetValue(roomEnterInstanceIds[roomEnterIdx], out var time)
                    ? 1f / ((Time.time - time) / 300f) // 300 secs = 5 mins
                    : 0f;

                impactingActions.Add(cost, multiplier, ActionEnum.Searches | ActionEnum.Room, roomEnterIdx);
            }
        }
    }

    public void Enabling(int roomEnterIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        beliefsToEnable.Add(new(BeliefEnum.WithinRoom, roomEnterIdx));
    }
}
