using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Melee;

internal struct MeleeStore() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> MeleeAttackPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        MeleeAttackPositionRef.Dispose();
    }
}
