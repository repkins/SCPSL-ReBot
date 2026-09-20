using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.BlinkTo;

internal partial struct BlinkTo<TTarget>
    where TTarget : ITarget
{
    internal struct Enabling(BotRef botRef, TTarget target)
    {
        NativeReference<(bool, Vector3)> blinkToPositionRef = botRef.Value.BlinkState.BlinkToPositionRef;

        public bool Process(int targetLocationIdx)
        {
            var targetPosition = target.Locations[targetLocationIdx];
            blinkToPositionRef.Value = (true, targetPosition);

            return true;
        }
    }
}
