using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Interact;

internal struct InteractEmergencyRelease(BotRef botRef)
{
    Vector3 playerPosition = botRef.Value.PlayerStore.Position;

    NativeList<bool> doorObstacleEmergencyReleaseHasPositions = ObstacleStore.DoorObstacleEmergencyReleaseHasPositions;
    NativeList<Vector3> doorObstacleEmergencyReleasePositions = ObstacleStore.DoorObstacleEmergencyReleasePositions;

    NativeHashMap<Vector3, ObstacleRef> obstaclesByTarget = botRef.Value.ObstacleStore.ObstaclesByTarget;

    NativeReference<(bool, Vector3)> interactPositionRef = botRef.Value.InteractionStore.InteractWithPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementStore.LookAtPositionRef;

    public void Impacting(ObstacleRef obstacleRef, NativeList<ImpactingAction> impactingActions)
    {
        if (doorObstacleEmergencyReleaseHasPositions[obstacleRef.Idx])
        {
            var cost = 1f;

            impactingActions.Add(cost, ActionEnum.InteractsWith | ActionEnum.EmergencyRelease, obstacleRef.Idx);
        }
    }

    public void Enabling(int obstacleIdx, NativeList<BeliefRef> beliefsToEnable)
    {
        var emergencyReleasePosition = doorObstacleEmergencyReleasePositions[obstacleIdx];

        if (Vector3.Distance(emergencyReleasePosition, playerPosition) < 2f)
        {
            if (!obstaclesByTarget.TryGetValue(emergencyReleasePosition, out var obstacle2Ref))
            {
                interactPositionRef.Value = (true, emergencyReleasePosition);
                lookAtPositionRef.Value = (true, emergencyReleasePosition);
            }
            else
            {
                beliefsToEnable.Add(new(BeliefEnum.UnobstructingObstacle, obstacle2Ref.Idx));
            }
        }
        else
        {
            beliefsToEnable.Add(new(BeliefEnum.WithinEmergencyRelease, obstacleIdx));
        }
    }
}
