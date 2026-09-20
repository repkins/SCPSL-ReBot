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

        public bool Process(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs, float multiplier = 1f)
        {
            var targetPosition = target.Locations[targetLocationIdx];

            if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
            {
                var cost = Vector3.Distance(targetPosition, playerPosition);

                impactingBeliefs.Add(cost, multiplier, BeliefEnum.MovedTo | target.Enum, targetLocationIdx);
            }
            else
            {
                var cost = Vector3.Distance(targetPosition, obstacleLocations[obstacleRef.Idx]);

                impactingBeliefs.Add(cost, multiplier, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacleRef.Idx);
            }

            return false;
        }
    }
}
