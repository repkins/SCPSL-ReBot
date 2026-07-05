using ReBot.AI.Movement;
using ReBot.Navigation.Mesh;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI.Movement.Pathfinding;

internal static class PathfindingStore
{
    public static readonly List<(BotRef, Vector3)> ClosestPositions = [];
    
    public static readonly List<Vector3> GoalPositions = [];

    public static readonly List<List<TransformCell>> CellsPath = [];
    public static readonly List<IEnumerable<(TransformCell Cell, TransformCell NextCell)>> CellPathSegments = [];

    public static readonly List<List<Vector3>> PointsPath = [];
    public static readonly List<IEnumerable<(Vector3 point, Vector3 nextPoint)>> PathSegments = [];
}
