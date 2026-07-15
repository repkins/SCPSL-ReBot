using ReBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Spacial;

public struct ObstacleState() : IDisposable
{
    public static readonly NativeList<Vector3> ObstacleLocations = new(Allocator.Persistent);

    internal static readonly Dictionary<TransformCell, ObstacleRef> ObstaclesByCell = [];
    internal static readonly Dictionary<ObstacleRef, TransformCell> CellsByObstacle = [];

    public NativeHashMap<Vector3, ObstacleRef> ObstaclesByTarget = new(1, Allocator.Persistent);

    public void Dispose()
    {
        ObstaclesByTarget.Dispose();
    }
}
