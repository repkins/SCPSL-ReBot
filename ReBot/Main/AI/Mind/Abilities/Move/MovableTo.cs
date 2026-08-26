using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Move;

internal struct MovableTo<TTarget>(BotRef botRef, TTarget target) : IMovableToTarget
    where TTarget : ITarget
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;

    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool GoesTo(int targetLocationIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var targetPosition = target.Locations[targetLocationIdx];

        if (!obstaclesByTarget.TryGetValue(targetPosition, out var obstacleRef))
        {
            var cost = Vector3.Distance(targetPosition, playerPosition);

            impactingBeliefs.Add(cost, target.MovedToEnum, targetLocationIdx);
        }
        else
        {
            var cost = Vector3.Distance(targetPosition, obstacleLocations[obstacleRef.Idx]);

            impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacleRef.Idx);
        }

        return false;
    }

    public bool MovesTo(int targetLocationIdx)
    {
        var targetPosition = target.Locations[targetLocationIdx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}

internal interface ITarget
{
    NativeList<Vector3> Locations { get; }
    BeliefEnum MovedToEnum { get; }
}
