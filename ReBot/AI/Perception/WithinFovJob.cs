using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Perception;

public struct WithinFovJob : IJobFor
{
    [ReadOnly]
    public required LayerMask raycastMask;
    [ReadOnly]
    public required float raycastDistance;

    [ReadOnly]
    public required Vector3 fromPosition;
    [ReadOnly]
    public required Vector3 direction;
    [ReadOnly]
    public required NativeArray<Vector3> targetPositions;

    [WriteOnly]
    public required NativeList<int> outWithinFovTargets;
    [WriteOnly]
    public required NativeArray<RaycastCommand> outRaycastCommands;

    public void Execute(int index)
    {
        var targetPosition = targetPositions[index];

        var facingDir = direction;
        var diff = Vector3.Normalize(targetPosition - fromPosition);

        if (Vector3.Dot(facingDir, diff) < 0)
        {
            return;
        }

        var fovDegrees = 90f;
        var fovRadians = fovDegrees * Mathf.Deg2Rad;
        var cosineOfFov = Mathf.Cos(fovRadians / 2f);

        if (Vector3.Dot(facingDir, diff) < cosineOfFov)
        {
            return;
        }

        outWithinFovTargets.Add(index);
        outRaycastCommands[index] = new(fromPosition, direction, new(raycastMask), raycastDistance);
    }
}
