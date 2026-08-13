using ReBot.Navigation.Mesh;
using UnityEngine;

namespace ReBot.Main.AI.Movement.Pathfinding;

internal static class PathfindingSystem
{
    public static void Process()
    {
        PathfindingStore.ClosestPositions.Clear();

        for (int botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            var moveToPositionRef = bot.MovementState.MoveToPositionRef;

            var (hasTargetPos, targetPos) = moveToPositionRef.Value;
            moveToPositionRef.Value = default;

            if (!hasTargetPos)
            {
                continue;
            }

            var startingPos = bot.PlayerState.Position;

            var playerRadius = .5f;
            var startingCellResult = NavigationMesh.GetCellWithinOrClosest(startingPos, playerRadius);
            var targetCellResult = NavigationMesh.GetCellWithinOrClosest(targetPos, playerRadius);

            if (!startingCellResult.HasValue || !targetCellResult.HasValue)
            {
                continue;
            }

            NavigationMesh.FindShortestPath(startingCellResult.Value, targetCellResult.Value, PathfindingStore.CellsPath[botIdx]);
            if (PathfindingStore.CellsPath[botIdx].Count < 1)
            {
                continue;
            }

            var closestPosition = PathfindingStore.CellsPath[botIdx].Count > 1
                ? GetNextCorner(botIdx, startingPos, targetPos, startingCellResult.Value) 
                : targetPos;

            PathfindingStore.ClosestPositions.Add((new(botIdx), closestPosition));
            PathfindingStore.GoalPositions[botIdx] = targetPos;

            var pointsPath = PathfindingStore.PointsPath[botIdx];
            pointsPath.Clear();
            pointsPath.Add(startingPos);

            var partialPath = false;
            foreach (var (cell, nextCell) in PathfindingStore.CellPathSegments[botIdx])
            {
                if (!cell.AdjacentCellEdges.TryGetValue(nextCell, out var e)
                    && !NavigationMesh.ForeignConnectedCellEdges[cell].TryGetValue(nextCell, out e))
                {
                    partialPath = true;
                    break;
                }
                pointsPath.Add(Vector3.Lerp(e.From.Position, e.To.Position, .5f));
            }

            if (!partialPath)
            {
                pointsPath.Add(targetPos);
            }
        }
    }

    private static Vector3 GetNextCorner(int botIdx, Vector3 playerPosition, Vector3 goalPosition, TransformCell currentCell)
    {
        var cellsPath = PathfindingStore.CellsPath[botIdx];
        
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
