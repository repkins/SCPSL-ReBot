using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Cardiac;

internal struct CardiacStore() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> CardiacPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        CardiacPositionRef.Dispose();
    }
}
