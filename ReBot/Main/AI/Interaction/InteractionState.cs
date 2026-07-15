using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Interaction;

public struct InteractionState() : IDisposable
{
    public NativeReference<(bool, Vector3)> InteractWithPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        InteractWithPositionRef.Dispose();
    }
}
