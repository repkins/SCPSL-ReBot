using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.NeckSnap;

public struct NeckSnapState() : IDisposable
{
    public bool IsHeld;

    public NativeReference<(bool, Vector3)> SnapPositionRef = new(Allocator.Persistent);

    public void Dispose()
    {
        SnapPositionRef.Dispose();
    }
}
