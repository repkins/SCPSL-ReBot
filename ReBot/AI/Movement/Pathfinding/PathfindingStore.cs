using ReBot.AI.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI.Movement.Pathfinding;

internal static class PathfindingStore
{
    public static List<(FpcRoleRef, Vector3)> ClosestPositions = [];
}
