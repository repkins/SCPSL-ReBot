using Interactables.Interobjects.DoorUtils;
using ReBot.Main.AI.Mind.Abilities.Move;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Obstacle;

internal struct InteractableObstacle(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<DoorPermissionFlags> doorObstaclePermissions = ObstacleState.DoorObstaclePermissions;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool Overcomes(ObstacleRef obstacleRef, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var obstaclePosition = obstacleLocations[obstacleRef.Idx];
        var obstacleInteractPosition = obstacleLocations[obstacleRef.Idx];

        var cost = Vector3.Distance(obstaclePosition, obstacleInteractPosition) + 1f;

        impactingBeliefs.Add(cost, BeliefEnum.InteractingWithObstacle, obstacleRef.Idx);

        return false;
    }

    public bool Interacts(int obstacleInteractIdx, NativeList<ImpactingBelief> impactingBeliefs)
    {
        var doorPermissions = doorObstaclePermissions[obstacleInteractIdx];
        if (doorPermissions == DoorPermissionFlags.None)
        {
            var obstacleInteractPosition = obstacleLocations[obstacleInteractIdx];

            if (Vector3.Distance(obstacleInteractPosition, playerPosition) < 2f)
            {
                if (!obstaclesByTarget.TryGetValue(obstacleInteractPosition, out var obstacle2Ref))
                {
                    interactPositionRef.Value = (true, obstacleInteractPosition);
                    lookAtPositionRef.Value = (true, obstacleInteractPosition);
                }
                else
                {
                    var cost = Vector3.Distance(obstacleInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                    impactingBeliefs.Add(cost, BeliefEnum.ObstacleToOvercome, obstacle2Ref.Idx);
                }

                return true;
            }
            else
            {
                var cost = 0f;

                impactingBeliefs.Add(cost, BeliefEnum.WithinObstacleInteractionLocation, obstacleInteractIdx);
            }
        }

        return false;
    }
}
