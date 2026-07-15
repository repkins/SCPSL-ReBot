using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Termination;

public struct TerminationState() : IDisposable
{
    public NativeList<Vector3> EnemiesWithinSight = new(Allocator.Persistent);
    public NativeList<EnemyRef> EnemiesWithinDistance = new(Allocator.Persistent);

    public void Dispose()
    {
        EnemiesWithinSight.Dispose();
        EnemiesWithinDistance.Dispose();
    }
}
