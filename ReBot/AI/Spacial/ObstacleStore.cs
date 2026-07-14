using ReBot.Navigation.Mesh;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal static class ObstacleStore
{
    public static readonly NativeList<Vector3> ObstacleLocations = new(Allocator.Persistent);

    public static readonly Dictionary<TransformCell, ObstacleRef> ObstaclesByCell = [];
    public static readonly Dictionary<ObstacleRef, TransformCell> CellsByObstacle = [];

    public static readonly List<NativeHashMap<Vector3, ObstacleRef>> ObstaclesByTarget = [];
}
