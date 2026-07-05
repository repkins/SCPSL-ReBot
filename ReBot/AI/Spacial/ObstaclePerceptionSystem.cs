using ReBot.AI.Movement.Pathfinding;
using ReBot.Navigation.Mesh;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal class ObstaclePerceptionSystem
{
    private static readonly LayerMask collisionLayerMask = LayerMask.GetMask("Default", "Door", "InteractableNoPlayerCollision", "Glass");

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var cellsPath = PathfindingStore.CellsPath[botIdx];
            var obstacles = ObstacleStore.ObstaclesByTarget[botIdx];

            var goalPos = PathfindingStore.GoalPositions[botIdx];

            var cameraPosition = PlayerStore.PlayerCameraPositions[botIdx];
            var cameraForward = PlayerStore.PlayerCameraForwards[botIdx];

            foreach (var cellInPath in cellsPath)
            {
                if (!ObstacleStore.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    continue;
                }

                var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];

                if (!IsWithinFov(cameraPosition, cameraForward, obstaclePosition))
                {
                    continue;
                }

                if (!IsPositionObstructed(cameraPosition, obstaclePosition, out RaycastHit outObstructionHit))
                {
                    if (obstacles.ContainsKey(goalPos))
                    {
                        obstacles.Remove(goalPos);
                    }
                    continue;
                }

                var obstacleTransform = NavigationMesh.CellsWithObstacles[cellInPath];
                if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(outObstructionHit.collider))
                {
                    continue;
                }

                obstacles[goalPos] = obstacleRef;
                break;
            }
        }
    }

    private static bool IsWithinFov(Vector3 position, Vector3 forward, Vector3 targetPosition)
    {
        var facingDir = forward;
        var diff = Vector3.Normalize(targetPosition - position);

        if (Vector3.Dot(facingDir, diff) < 0)
        {
            return false;
        }

        if (Vector3.Angle(facingDir, diff) > 90)
        {
            return false;
        }

        return true;
    }

    private static bool IsPositionObstructed(Vector3 cameraPosition, Vector3 targetPosition, out RaycastHit outObstructtionHit)
    {
        var isObstructed = Physics.Linecast(cameraPosition, targetPosition, out var hit, collisionLayerMask);
        if (isObstructed)
        {
            outObstructtionHit = hit;
        }
        else
        {
            outObstructtionHit = default;
        }

        return isObstructed;
    }
}
