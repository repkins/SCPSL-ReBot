using ReBot.Main.AI.Mind.Abilities.Target;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal partial struct MoveTo<TTarget>
    where TTarget : ITarget
{
    internal struct Enabling(BotRef botRef, TTarget target)
    {
        NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

        public bool Process(int targetLocationIdx)
        {
            var targetPosition = target.Locations[targetLocationIdx];
            moveToPositionRef.Value = (true, targetPosition);

            return true;
        }
    }
}
