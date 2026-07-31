using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities;

public struct MovesToObstacleInteractLocation(BotRef botRef)
{
    NativeList<Vector3> obstacleLocations = ObstacleState.ObstacleLocations;
    NativeReference<(bool, Vector3)> moveToPositionRef = botRef.Value.MovementState.MoveToPositionRef;

    public bool Process(int obstacleInteractIdx)
    {
        var targetPosition = obstacleLocations[obstacleInteractIdx];
        moveToPositionRef.Value = (true, targetPosition);

        return true;
    }
}
