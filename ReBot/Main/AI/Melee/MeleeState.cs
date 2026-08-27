using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Melee;

public struct MeleeState() : IDisposable
{
    public NativeReference<(bool, Vector3)> MeleeAttackPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        MeleeAttackPositionRef.Dispose();
    }
}
