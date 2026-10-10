using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SearchRoom(BotRef botRef)
{
    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    public bool Impacting(NativeList<ImpactingAction> impactingActions)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < 1; roomEnterLocationIdx++)
            {
                var cost = 0f;
                var multiplier = 10f; // cost multiplier is odds of seeing enemies

                impactingActions.Add(cost, multiplier, ActionEnum.Searches | ActionEnum.Room, roomEnterLocationIdx);
            }
        }

        return false;
    }

    public bool Enabling(int roomEnterLocationIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        beliefsToEnable.Add(new(BeliefEnum.WithinRoom, roomEnterLocationIdx));

        return false;
    }
}
