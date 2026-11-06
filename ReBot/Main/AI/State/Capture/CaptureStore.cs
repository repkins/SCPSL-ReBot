using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Capture;

public struct CaptureStore() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> CapturePositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        CapturePositionRef.Dispose();
    }
}
