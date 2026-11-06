using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.State;

public struct WithinFovJob(float fovDegrees = 90f) : IJobFor
{
    private readonly float cosineOfFov = Mathf.Cos(fovDegrees * Mathf.Deg2Rad / 2f);

    [ReadOnly]
    public required LayerMask raycastMask;
    [ReadOnly]
    public required float sightDistance;

    [ReadOnly]
    public required Vector3 fromPosition;
    [ReadOnly]
    public required Vector3 facingDirection;
    [ReadOnly]
    public required NativeArray<Vector3> targetPositions;

    [WriteOnly]
    public required NativeList<int> outWithinFovTargets;
    [WriteOnly]
    public required NativeArray<RaycastCommand> outRaycastCommands;

    public void Execute(int index)
    {
        var targetPosition = targetPositions[index];

        var relTargetPosition = targetPosition - fromPosition;
        var directionToTarget = Vector3.Normalize(relTargetPosition);

        if (Vector3.Dot(facingDirection, directionToTarget) < 0)
        {
            return;
        }

        if (Vector3.Dot(facingDirection, directionToTarget) < cosineOfFov)
        {
            return;
        }

        outWithinFovTargets.Add(index);
        outRaycastCommands[index] = new(fromPosition, directionToTarget, new(raycastMask), relTargetPosition.magnitude);
    }
}
