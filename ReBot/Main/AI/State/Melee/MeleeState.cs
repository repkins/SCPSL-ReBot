using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Melee;

public struct MeleeState() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> MeleeAttackPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        MeleeAttackPositionRef.Dispose();
    }
}
