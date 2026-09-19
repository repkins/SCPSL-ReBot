using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Passthrough;

internal struct PassthroughObstacleEnabling(BotRef botRef)
{
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;
    NativeReference<(bool, Vector3)> lookAtPositionRef = botRef.Value.MovementState.LookAtPositionRef;

    public bool Process(int obstacleIdx)
    {
        var obstaclePosition = obstacleLocations[obstacleIdx];
        moveToPositionRef.Value = (true, obstaclePosition);
        lookAtPositionRef.Value = (true, obstaclePosition);

        return true;
    }
}
