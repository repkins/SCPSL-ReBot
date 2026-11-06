using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SearchRoom(BotRef botRef)
{
    NativeHashMap<int, float> roomLastVisitTime = botRef.Value.RoomState.RoomLastVisitTime;

    NativeList<int> roomEnterInstanceIds = botRef.Value.RoomState.RoomEnterInstanceIds;
    NativeList<bool> roomExitIsWithins = botRef.Value.RoomState.RoomExitIsWithins;
    NativeList<float> roomEnterExitDistances = botRef.Value.RoomState.RoomEnterExitDistances;

    public void Impacting(NativeList<ImpactingAction> impactingActions)
    {
        if (roomEnterInstanceIds.Length > 0)
        {
            for (var roomEnterIdx = 0; roomEnterIdx < roomEnterInstanceIds.Length; roomEnterIdx++)
            {
                var cost = 0f;
                var multiplier = 10f; // cost multiplier is odds of seeing enemies
                multiplier += roomLastVisitTime.TryGetValue(roomEnterInstanceIds[roomEnterIdx], out var time)
                    ? 1f / ((Time.time - time) / 300f) // 300 secs = 5 mins
                    : 0f;

                if (!roomExitIsWithins[roomEnterIdx])
                {
                    cost += roomEnterExitDistances[roomEnterIdx];
                }

                impactingActions.Add(cost, multiplier, ActionEnum.Searches | ActionEnum.Room, roomEnterIdx);
            }
        }
    }

    public void Enabling(int roomEnterIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        beliefsToEnable.Add(new(BeliefEnum.WithinRoom, roomEnterIdx));
    }
}
