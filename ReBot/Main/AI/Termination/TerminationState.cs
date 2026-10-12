using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Termination;

public struct TerminationState() : IDisposable
{
    public NativeList<int> EnemyInstanceIds = new(5, Allocator.Persistent);
    public NativeList<Vector3> EnemyLocations = new(5, Allocator.Persistent);
    public NativeList<EnemyRef> EnemiesWithinSight = new(Allocator.Persistent);
    public NativeList<EnemyRef> EnemiesWithinDistance = new(Allocator.Persistent);

    public void Dispose()
    {
        EnemyInstanceIds.Dispose();
        EnemyLocations.Dispose();
        EnemiesWithinSight.Dispose();
        EnemiesWithinDistance.Dispose();
    }
}
