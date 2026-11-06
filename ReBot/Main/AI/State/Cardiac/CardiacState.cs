using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Cardiac;

public struct CardiacState() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> CardiacPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        CardiacPositionRef.Dispose();
    }
}
