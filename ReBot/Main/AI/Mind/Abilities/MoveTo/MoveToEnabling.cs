using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal struct MoveToEnabling<TTarget>(BotRef botRef, TTarget target)
    where TTarget : ITarget
{
    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool Process(int targetLocationIdx)
    {
        var targetPosition = target.Locations[targetLocationIdx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}
