using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Blink;

public struct BlinkState() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> BlinkToPositionRef = new(Allocator.Persistent);

    public Vector3 LastCameraPosition;
    public float DownAimAmount = 0f;

    public bool IsObserved;
    public float RemainingSecondsToBlink;

    public void Dispose()
    {
        BlinkToPositionRef.Dispose();
    }
}
