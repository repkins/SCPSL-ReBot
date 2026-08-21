using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Blink;

public struct BlinkState() : IDisposable
{
    public NativeReference<(bool, Vector3)> BlinkToPositionRef = new(Allocator.Persistent);

    public bool IsObserved;
    public float RemainingSecondsToBlink;

    public void Dispose()
    {
        BlinkToPositionRef.Dispose();
    }
}
