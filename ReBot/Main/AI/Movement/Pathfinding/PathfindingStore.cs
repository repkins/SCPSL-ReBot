using ReBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ReBot.Main.AI.Movement.Pathfinding;

internal static class PathfindingStore
{
    public static readonly List<(BotRef, Vector3)> ClosestPositions = [];
    
    public static readonly List<Vector3> GoalPositions = [];

    public static readonly List<List<TransformCell>> CellsPath = [];
    public static readonly List<IEnumerable<(TransformCell Cell, TransformCell NextCell)>> CellPathSegments = [];

    public static readonly List<List<Vector3>> PointsPath = [];
    public static readonly List<IEnumerable<(Vector3 point, Vector3 nextPoint)>> PathSegments = [];

    public static void AddBot(BotRef botRef)
    {
        GoalPositions.Add(default);

        CellsPath.Add([]);
        CellPathSegments.Add(CellsPath[botRef.Idx].Zip(CellsPath[botRef.Idx].Skip(1), (cell, nextCell) => (cell, nextCell)));

        PointsPath.Add([]);
        PathSegments.Add(PointsPath[botRef.Idx].Zip(PointsPath[botRef.Idx].Skip(1), (point, nextPoint) => (point, nextPoint)));
    }

    public static void RemoveBot(BotRef botRef)
    {
        PathSegments.RemoveAt(botRef.Idx);
        PointsPath.RemoveAt(botRef.Idx);

        CellPathSegments.RemoveAt(botRef.Idx);
        CellsPath.RemoveAt(botRef.Idx);

        GoalPositions.RemoveAt(botRef.Idx);
    }
}
