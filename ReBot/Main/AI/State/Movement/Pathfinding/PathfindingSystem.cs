using ReBot.Navigation.Mesh;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Movement.Pathfinding;

internal static class PathfindingSystem
{
    public static void Process(NativeList<(BotRef, Vector3)> outClosestPositions)
    {
        foreach (var (botRef, position) in PathfindingStore.DestinationPositions)
        {
            var bot = botRef.Value;
            var botIdx = botRef.Idx;

            var startingPos = bot.PlayerStore.Position;
            var targetPos = position;

            var playerRadius = .5f;
            var startingCellResult = NavigationMesh.GetCellWithinOrClosest(startingPos, playerRadius) ?? PathfindingStore.CellWithins[botIdx];
            var targetCellResult = NavigationMesh.GetCellWithinOrClosest(targetPos, playerRadius * 1.5f);

            if (!startingCellResult.HasValue || !targetCellResult.HasValue)
            {
                continue;
            }

            PathfindingStore.CellWithins[botIdx] = startingCellResult;

            NavigationMesh.FindShortestPath(startingCellResult.Value, targetCellResult.Value, PathfindingStore.CellsPath[botIdx]);
            if (PathfindingStore.CellsPath[botIdx].Count < 1)
            {
                continue;
            }

            var closestPosition = PathfindingStore.CellsPath[botIdx].Count > 1
                ? GetNextCorner(botIdx, startingPos, targetPos, startingCellResult.Value) 
                : targetPos;

            outClosestPositions.Add((new(botIdx), closestPosition));
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

        var nextCell = cellsPath[currentPathIdx + 1];
        if (!currentCell.AdjacentCellEdges.TryGetValue(nextCell, out var nextCellEdge)
            && !NavigationMesh.ForeignConnectedCellEdges[currentCell].TryGetValue(nextCell, out nextCellEdge))
        {
            return currentCell.MeanPosition;
        }
        var nextEdgeMiddlePosition = Vector3.Lerp(nextCellEdge.From.Position, nextCellEdge.To.Position, 0.5f);

        var nextPosition = nextEdgeMiddlePosition;

        var aheadPathIdx = currentPathIdx + 1;

        while (nextEdgeMiddlePosition == nextPosition && aheadPathIdx < cellsPath.Count - 1)
        {
            aheadPathIdx++;

            var relNextEdgePos = (
                from: nextCellEdge.From.Position - playerPosition,
                to: nextCellEdge.To.Position - playerPosition);

            var aheadCell = cellsPath[aheadPathIdx];
            if (!nextCell.AdjacentCellEdges.TryGetValue(aheadCell, out var aheadCellEdge)
                && !NavigationMesh.ForeignConnectedCellEdges[currentCell].TryGetValue(nextCell, out aheadCellEdge))
            {
                goalPosition = nextCell.MeanPosition;
                break;
            }

            var relAheadTargetEdgePos = (
                from: aheadCellEdge.From.Position - playerPosition,
                to: aheadCellEdge.To.Position - playerPosition);

            var dirToAheadTargetEdgeNormals = (
                from: Vector3.Cross(relAheadTargetEdgePos.from, Vector3.up),
                to: Vector3.Cross(relAheadTargetEdgePos.to, Vector3.up));

            if (Vector3.Dot(relNextEdgePos.from, dirToAheadTargetEdgeNormals.from) < 0)
            {
                nextCellEdge.From = aheadCellEdge.From;
            }

            if (Vector3.Dot(relNextEdgePos.to, dirToAheadTargetEdgeNormals.to) > 0)
            {
                nextCellEdge.To = aheadCellEdge.To;
            }


            if (Vector3.Dot(relNextEdgePos.from, dirToAheadTargetEdgeNormals.to) > 0)
            {
                nextPosition = nextCellEdge.From.Position;
            }

            if (Vector3.Dot(relNextEdgePos.to, dirToAheadTargetEdgeNormals.from) < 0)
            {
                nextPosition = nextCellEdge.To.Position;
            }

            nextCell = aheadCell;
        }

        if (nextPosition == nextEdgeMiddlePosition)
        {
            nextPosition = goalPosition;

            var relNextTargetEdgePos = (
                from: nextCellEdge.From.Position - playerPosition,
                to: nextCellEdge.To.Position - playerPosition);

            var relGoalPos = goalPosition - playerPosition;
            var dirToGoalNormal = Vector3.Cross(relGoalPos, Vector3.up);

            if (Vector3.Dot(relNextTargetEdgePos.from, dirToGoalNormal) > 0)
            {
                nextPosition = nextCellEdge.From.Position;
            }

            if (Vector3.Dot(relNextTargetEdgePos.to, dirToGoalNormal) < 0)
            {
                nextPosition = nextCellEdge.To.Position;
            }
        }

        return nextPosition;
    }
}
