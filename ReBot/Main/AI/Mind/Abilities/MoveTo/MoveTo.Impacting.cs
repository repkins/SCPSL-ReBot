using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.MoveTo;

internal partial struct MoveTo<TTarget>
    where TTarget : ITarget
{
    internal struct Impacting(BotRef botRef, TTarget target) : IMoveToImpacting
    {
        Vector3 playerPosition = botRef.Value.PlayerState.Position;

        NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
        NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

        public bool Process(int targetLocationIdx, NativeList<ImpactingAction> impactingActions, float multiplier = 1f)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            var cost = ! obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef)
                ? Vector3.Distance(targetPosition, playerPosition)
                : Vector3.Distance(targetPosition, obstacleLocations[obstacleRef.Idx]);

            impactingActions.Add(cost, multiplier, ActionEnum.MovesTo | target.Enum, targetLocationIdx);

            return false;
        }
    }
}
