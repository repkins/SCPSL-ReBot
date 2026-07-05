using ReBot.AI.Movement.Pathfinding;
using ReBot.Navigation.Mesh;
using System;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal class ObstaclePerceptionSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.BotHubs.Count; botIdx++)
        {
            var cellsPath = PathfindingStore.CellsPath[botIdx];
            var obstacles = ObstacleStore.ObstaclesByTarget[botIdx];

            var goalPos = PathfindingStore.GoalPositions[botIdx];

            foreach (var cellInPath in cellsPath)
            {
                if (!ObstacleStore.ObstaclesByCell.TryGetValue(cellInPath, out var obstacleRef))
                {
                    continue;
                }

                var obstaclePosition = ObstacleStore.ObstacleLocations[obstacleRef.Idx];



                if (!sightSense.IsPositionWithinFov(obstaclePosition))
                {
                    break;
                }

                if (!sightSense.IsPositionObstructed(obstaclePosition, out RaycastHit outObstructionHit))
                {
                    if (obstacles.ContainsKey(goalPos))
                    {
                        obstacles.Remove(goalPos);
                    }
                    break;
                }

                var obstacleTransform = NavigationMesh.CellsWithObstacles[cellInPath];
                if (!obstacleTransform.gameObject.GetComponentsInChildren<Collider>().Contains(outObstructionHit.collider))
                {
                    break;
                }

                obstacles.Add(goalPos, obstacleRef);
            }
        }
    }
}
