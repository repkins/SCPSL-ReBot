using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Cardiac;

public struct CardiacState() : IDisposable
{
    public NativeReference<(bool, Vector3)> CardiacPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        CardiacPositionRef.Dispose();
    }
}
