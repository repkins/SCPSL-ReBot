using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Capture;

public struct CaptureState() : IDisposable
{
    public NativeReference<(bool, Vector3)> CapturePositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        CapturePositionRef.Dispose();
    }
}
