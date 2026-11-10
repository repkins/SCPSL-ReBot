using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Break;

internal struct BreakObstacle(BotRef botRef)
{
    bool isEnraged = botRef.Value.RageStore.IsEnraged;

    Vector3 playerPosition = botRef.Value.PlayerStore.Position;

    NativeList<Vector3> obstacleLocations = ObstacleStore.ObstacleLocations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleStore.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementStore.LookAtPositionRef;
    NativeReference<(bool, Vector3)> meleeAttackPositionRef = botRef.Value.MeleeStore.MeleeAttackPositionRef;

    public void Impacting(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        if (isEnraged)
        {
            var cost = 0f;

            impactingActions.Add(cost, ActionEnum.Attacks | ActionEnum.Obstacle, obstacleRef.Idx);
        }
    }

    public void Enabling(int obstacleIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var obstaclePosition = obstacleLocations[obstacleIdx];

        if (Vector3.Distance(obstaclePosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(obstaclePosition, out var obstacle2Ref))
            {
                meleeAttackPositionRef.Value = (true, obstaclePosition);
                lookAtPositionRef.Value = (true, obstaclePosition);
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacle2Ref.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinObstacle, obstacleIdx));
        }
    }
}
