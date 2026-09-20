using Interactables.Interobjects.DoorUtils;
using ReBot.Main.AI.Mind.Abilities.GoTo;
using ReBot.Main.AI.Mind.Abilities.Target;
using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Interact;

internal struct InteractObstacleEnabling(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerState.Position;

    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeList<DoorPermissionFlags> doorObstaclePermissions = ObstacleState.DoorObstaclePermissions;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleState.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionState.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    GoTo<Obstacle>.Impacting goToObstacleImpacting = new(botRef, new());

    public bool Process(int obstacleInteractIdx, NativeList<ImpactingBelief> impactingBeliefs)
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

                    return true;
                }
                else
                {
                    var cost = Vector3.Distance(obstacleInteractPosition, obstacleLocations[obstacle2Ref.Idx]);

                    impactingBeliefs.Add(cost, BeliefEnum.Overcame | BeliefEnum.Obstacle, obstacle2Ref.Idx);
                }
            }
            else
            {
                goToObstacleImpacting.Process(obstacleInteractIdx, impactingBeliefs);
            }
        }

        return false;
    }
}
