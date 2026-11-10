using ReBot.Navigation.Mesh;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Movement.Pathfinding;

internal static class PathfindingSystem
{
    const float SineOfJumpThresholdAngles = 0.5f;    // 30 degrees

    public static void Process(NativeList<(BotRef, Vector3, bool)> outClosestPositions)
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
                outClosestPositions.Add((new(botIdx), targetPos, default));
                PathfindingStore.GoalPositions[botIdx] = targetPos;

                continue;
            }

            PathfindingStore.PrevCellWithins[botIdx] = PathfindingStore.CellWithins[botIdx];
            PathfindingStore.CellWithins[botIdx] = startingCellResult;

            NavigationMesh.FindShortestPath(startingCellResult.Value, targetCellResult.Value, PathfindingStore.CellsPath[botIdx]);
            if (PathfindingStore.CellsPath[botIdx].Count < 1)
            {
                outClosestPositions.Add((new(botIdx), targetPos, default));
                PathfindingStore.GoalPositions[botIdx] = targetPos;

                continue;
            }

            var needsJump = false;
            var closestPosition = PathfindingStore.CellsPath[botIdx].Count > 1
                ? GetNextCorner(botIdx, startingPos, targetPos, startingCellResult.Value, out needsJump) 
                : targetPos;

            outClosestPositions.Add((new(botIdx), closestPosition, needsJump));
            PathfindingStore.GoalPositions[botIdx] = targetPos;
        }
    }

    private static Vector3 GetNextCorner(int botIdx, Vector3 playerPosition, Vector3 goalPosition, TransformCell currentCell, out bool jump)
    {
        jump = false;

        var cellsPath = PathfindingStore.CellsPath[botIdx];
        
        var startPathIdx = 0;

        var nextCell = cellsPath[startPathIdx + 1];
        if (!currentCell.AdjacentCellEdges.TryGetValue(nextCell, out var nextCellEdge)
            && !NavigationMesh.ForeignConnectedCellEdges[currentCell].TryGetValue(nextCell, out nextCellEdge))
        {
            return currentCell.MeanPosition;
        }

        var firstEdgeMiddlePosition = Vector3.Lerp(nextCellEdge.From.Position, nextCellEdge.To.Position, 0.5f);
        var directionToFirstEdge = Vector3.Normalize(firstEdgeMiddlePosition - playerPosition);

        var jumpEdgeResult = default(TransformEdge?);

        if (Vector3.Dot(directionToFirstEdge, Vector3.up) > SineOfJumpThresholdAngles)
        {
            jumpEdgeResult = nextCellEdge;
        }

        var nextPosition = firstEdgeMiddlePosition;

        var nextWithinCellEdge = nextCellEdge;
        var aheadPathIdx = startPathIdx + 1;

        while (firstEdgeMiddlePosition == nextPosition && aheadPathIdx < cellsPath.Count - 1)
        {
            aheadPathIdx++;

            var relNextEdgePos = (
                from: nextWithinCellEdge.From.Position - playerPosition,
                to: nextWithinCellEdge.To.Position - playerPosition);

            var aheadCell = cellsPath[aheadPathIdx];
            if (!nextCell.AdjacentCellEdges.TryGetValue(aheadCell, out var aheadCellEdge)
                && !NavigationMesh.ForeignConnectedCellEdges[currentCell].TryGetValue(nextCell, out aheadCellEdge))
            {
                goalPosition = nextCell.MeanPosition;
                break;
            }

            if (!jumpEdgeResult.HasValue)
            {
                var nextEdgeMiddlePosition = Vector3.Lerp(nextCellEdge.From.Position, nextCellEdge.To.Position, 0.5f);
                var aheadEdgeMiddlePosition = Vector3.Lerp(aheadCellEdge.From.Position, aheadCellEdge.To.Position, 0.5f);
                var directionToAheadEdge = Vector3.Normalize(aheadEdgeMiddlePosition - nextEdgeMiddlePosition);

                if (Vector3.Dot(directionToAheadEdge, Vector3.up) > SineOfJumpThresholdAngles)
                {
                    jumpEdgeResult = aheadCellEdge;
                }
            }

            var relAheadTargetEdgePos = (
                from: aheadCellEdge.From.Position - playerPosition,
                to: aheadCellEdge.To.Position - playerPosition);

            var dirToAheadTargetEdgeNormals = (
                from: Vector3.Cross(relAheadTargetEdgePos.from, Vector3.up),
                to: Vector3.Cross(relAheadTargetEdgePos.to, Vector3.up));

            if (Vector3.Dot(relNextEdgePos.from, dirToAheadTargetEdgeNormals.from) < 0)
            {
                nextWithinCellEdge.From = aheadCellEdge.From;
            }

            if (Vector3.Dot(relNextEdgePos.to, dirToAheadTargetEdgeNormals.to) > 0)
            {
                nextWithinCellEdge.To = aheadCellEdge.To;
            }


            if (Vector3.Dot(relNextEdgePos.from, dirToAheadTargetEdgeNormals.to) > 0)
            {
                nextPosition = nextWithinCellEdge.From.Position;
            }

            if (Vector3.Dot(relNextEdgePos.to, dirToAheadTargetEdgeNormals.from) < 0)
            {
                nextPosition = nextWithinCellEdge.To.Position;
            }

            nextCell = aheadCell;
            nextCellEdge = aheadCellEdge;
        }

        if (nextPosition == firstEdgeMiddlePosition)
        {
            nextPosition = goalPosition;

            var relNextTargetEdgePos = (
                from: nextWithinCellEdge.From.Position - playerPosition,
                to: nextWithinCellEdge.To.Position - playerPosition);

            var relGoalPos = goalPosition - playerPosition;
            var dirToGoalNormal = Vector3.Cross(relGoalPos, Vector3.up);

            if (Vector3.Dot(relNextTargetEdgePos.from, dirToGoalNormal) > 0)
            {
                nextPosition = nextWithinCellEdge.From.Position;
            }

            if (Vector3.Dot(relNextTargetEdgePos.to, dirToGoalNormal) < 0)
            {
                nextPosition = nextWithinCellEdge.To.Position;
            }
        }

        if (jumpEdgeResult.HasValue)
        {
            var plane = new Plane(playerPosition, nextPosition, playerPosition + Vector3.up);
            var ray = new Ray(jumpEdgeResult.Value.From.Position, jumpEdgeResult.Value.To.Position - jumpEdgeResult.Value.From.Position);

            if (plane.Raycast(ray, out var enterDist))
            {
                var intersectPoint = ray.GetPoint(enterDist);
                if (Vector3.Distance(playerPosition, intersectPoint) < 1f)
                {
                    jump = true;
                }
            }
        }

        return nextPosition;
    }
}
