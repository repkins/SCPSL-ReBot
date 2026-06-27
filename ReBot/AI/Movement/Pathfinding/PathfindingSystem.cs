using ReBot.AI.Movement;
using ReBot.Navigation.Mesh;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI.Movement.Pathfinding;

internal static class PathfindingSystem
{
    private static readonly List<TransformCell> cellsPath = [];

    public static void Process()
    {
        PathfindingStore.ClosestPositions.Clear();

        for (int botIdx = 0; botIdx < MovementStore.MoveToPositionRefs.Count; botIdx++)
        {
            var moveToPositionRef = MovementStore.MoveToPositionRefs[botIdx];

            var (hasTargetPos, targetPos) = moveToPositionRef.Value;
            moveToPositionRef.Value = default;

            if (!hasTargetPos)
            {
                continue;
            }

            var startingPos = MovementStore.PlayerPositions[botIdx];

            var startingCellResult = NavigationMesh.GetCellWithinOrClosest(startingPos);
            var targetCellResult = NavigationMesh.GetCellWithinOrClosest(targetPos);

            if (!startingCellResult.HasValue || !targetCellResult.HasValue)
            {
                continue;
            }

            NavigationMesh.FindShortestPath(startingCellResult.Value, targetCellResult.Value, cellsPath);
            var closestPosition = GetNextCorner(startingPos, targetPos, startingCellResult.Value);

            PathfindingStore.ClosestPositions.Add((new(botIdx), closestPosition));
        }
    }

    private static Vector3 GetNextCorner(Vector3 playerPosition, Vector3 goalPosition, TransformCell currentCell)
    {
        var currentPathIdx = 0;

        var nextTargetCell = cellsPath[currentPathIdx + 1];
        if (!currentCell.AdjacentCellEdges.TryGetValue(nextTargetCell, out var targetCellEdge)
            && !NavigationMesh.ForeignConnectedCellEdges[currentCell].TryGetValue(nextTargetCell, out targetCellEdge))
        {
            return currentCell.MeanPosition;
        }
        var nextTargetEdgeMiddlePosition = Vector3.Lerp(targetCellEdge.From.Position, targetCellEdge.To.Position, 0.5f);

        var nextTargetPosition = nextTargetEdgeMiddlePosition;

        var aheadPathIdx = currentPathIdx + 1;

        while (nextTargetEdgeMiddlePosition == nextTargetPosition && aheadPathIdx < cellsPath.Count - 1)
        {
            aheadPathIdx++;

            var relTargetEdgePos = (
                from: targetCellEdge.From.Position - playerPosition,
                to: targetCellEdge.To.Position - playerPosition);

            var aheadTargetCell = cellsPath[aheadPathIdx];
            if (!nextTargetCell.AdjacentCellEdges.TryGetValue(aheadTargetCell, out var aheadTargetCellEdge)
                && !NavigationMesh.ForeignConnectedCellEdges[currentCell].TryGetValue(nextTargetCell, out aheadTargetCellEdge))
            {
                goalPosition = nextTargetCell.MeanPosition;
                break;
            }

            var relAheadTargetEdgePos = (
                from: aheadTargetCellEdge.From.Position - playerPosition,
                to: aheadTargetCellEdge.To.Position - playerPosition);

            var dirToAheadTargetEdgeNormals = (
                from: Vector3.Cross(relAheadTargetEdgePos.from, Vector3.up),
                to: Vector3.Cross(relAheadTargetEdgePos.to, Vector3.up));

            if (Vector3.Dot(relTargetEdgePos.from, dirToAheadTargetEdgeNormals.from) < 0)
            {
                targetCellEdge.From = aheadTargetCellEdge.From;
            }

            if (Vector3.Dot(relTargetEdgePos.to, dirToAheadTargetEdgeNormals.to) > 0)
            {
                targetCellEdge.To = aheadTargetCellEdge.To;
            }


            if (Vector3.Dot(relTargetEdgePos.from, dirToAheadTargetEdgeNormals.to) > 0)
            {
                nextTargetPosition = targetCellEdge.From.Position;
            }

            if (Vector3.Dot(relTargetEdgePos.to, dirToAheadTargetEdgeNormals.from) < 0)
            {
                nextTargetPosition = targetCellEdge.To.Position;
            }

            nextTargetCell = aheadTargetCell;
        }

        if (nextTargetPosition == nextTargetEdgeMiddlePosition)
        {
            nextTargetPosition = goalPosition;

            var relNextTargetEdgePos = (
                from: targetCellEdge.From.Position - playerPosition,
                to: targetCellEdge.To.Position - playerPosition);

            var relGoalPos = goalPosition - playerPosition;
            var dirToGoalNormal = Vector3.Cross(relGoalPos, Vector3.up);

            if (Vector3.Dot(relNextTargetEdgePos.from, dirToGoalNormal) > 0)
            {
                nextTargetPosition = targetCellEdge.From.Position;
            }

            if (Vector3.Dot(relNextTargetEdgePos.to, dirToGoalNormal) < 0)
            {
                nextTargetPosition = targetCellEdge.To.Position;
            }
        }

        return nextTargetPosition;
    }
}
