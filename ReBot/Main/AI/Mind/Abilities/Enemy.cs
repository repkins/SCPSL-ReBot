using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Enemy(BotRef botRef)
{
    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    public bool SearchesInRoom(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < 1; roomEnterLocationIdx++)
            {
                var cost = 0f;
                var multiplier = 10f; // multiplier is odds of seeing enemies

                impactingBeliefs.Add(cost, multiplier, BeliefEnum.WithinRoom, roomEnterLocationIdx);
            }
        }

        return false;
    }
}
