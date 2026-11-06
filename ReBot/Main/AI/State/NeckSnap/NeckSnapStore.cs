using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.NeckSnap;

public struct NeckSnapStore() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> SnapPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        SnapPositionRef.Dispose();
    }
}
