using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Interaction;

internal struct InteractionStore() : IDisposable
{
    public NativeReference<(bool, Vector3)> InteractWithPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        InteractWithPositionRef.Dispose();
    }
}
