using ReBot.AI.Movement;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI.Movement.Pathfinding;

internal static class PathfindingStore
{
    public static List<(BotRef, Vector3)> ClosestPositions = [];
}
