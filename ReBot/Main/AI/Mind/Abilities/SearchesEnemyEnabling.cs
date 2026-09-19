using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SearchesEnemyEnabling(BotRef botRef)
{
    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    GoTo<Room>.Impacting goToImpactingRoom = new(botRef, new(botRef));

    public bool Process(NativeList<ImpactingBelief> impactingBeliefs)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < 1; roomEnterLocationIdx++)
            {
                var costMultiplier = 10f; // cost multiplier is odds of seeing enemies
                goToImpactingRoom.Process(roomEnterLocationIdx, impactingBeliefs, costMultiplier);
            }
        }

        return false;
    }
}
