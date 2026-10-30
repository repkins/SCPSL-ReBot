using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Break;

internal struct BreakObstacle(BotRef botRef)
{
    bool isEnraged = botRef.Value.RageState.IsEnraged;

    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;
    NativeReference<(bool, Vector3)> meleeAttackPositionRef = botRef.Value.MeleeState.MeleeAttackPositionRef;

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
