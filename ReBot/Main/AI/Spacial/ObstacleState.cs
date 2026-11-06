using Interactables.Interobjects;
using Interactables.Interobjects.DoorUtils;
using ReBot.Navigation.Mesh;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Spacial;

public struct ObstacleState() : IDisposable
{
    public static readonly NativeList<Vector3> ObstacleLocations = new(Allocator.Persistent);
    public static readonly NativeList<DoorPermissionFlags> DoorObstaclePermissions = new(Allocator.Persistent);

    internal static readonly Dictionary<TransformCell, ObstacleRef> ObstaclesByCell = [];
    internal static readonly Dictionary<ObstacleRef, TransformCell> CellsByObstacle = [];

    public static readonly NativeList<ObstacleRef> ElevationObstacles = new(Allocator.Persistent);

    public static readonly NativeList<ObstacleRef> ElevationTravelObstacles = new(Allocator.Persistent);
    public static readonly NativeList<Vector3> ElevationTravelEnterLocations = new(Allocator.Persistent);
    public static readonly List<ElevatorPanel> ElevationTravelInteractPanels = [];
    public static NativeList<Vector3> ElevationTravelInteractLocations = new(Allocator.Persistent);
    public static NativeList<Vector3> ElevationTravelChamberLocations = new(Allocator.Persistent);

    public static readonly NativeList<ObstacleRef> ElevationCallObstacles = new(Allocator.Persistent);
    public static readonly NativeList<Vector3> ElevationCallInteractLocations = new(Allocator.Persistent);
    public static readonly List<ElevatorPanel> ElevationCallInteractPanels = [];
    public static NativeList<bool> ElevationCallInteractAbles = new(Allocator.Persistent);

    public NativeHashMap<Vector3, ObstacleRef> ObstaclesByTarget = new(1, Allocator.Persistent);

    public void Dispose()
    {
        ObstaclesByTarget.Dispose();
    }
}
