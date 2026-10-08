using ReBot.Main.AI.Mind.Abilities.GoTo;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct SearchesEnemyEnabling<TGoToImpacting>(BotRef botRef, TGoToImpacting goToRoomImpacting)
    where TGoToImpacting : IGoToImpacting
{
    NativeList<Vector3> roomEnterLocations = botRef.Value.RoomState.RoomEnterLocations;

    public bool Process(NativeList<ImpactingAction> impactingActions)
    {
        if (roomEnterLocations.Length > 0)
        {
            for (var roomEnterLocationIdx = 0; roomEnterLocationIdx < 1; roomEnterLocationIdx++)
            {
                var costMultiplier = 10f; // cost multiplier is odds of seeing enemies
                goToRoomImpacting.Process(roomEnterLocationIdx, impactingActions, costMultiplier);
            }
        }

        return false;
    }
}
