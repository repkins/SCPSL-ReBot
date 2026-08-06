using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Movement;

public struct MovementState() : IDisposable
{
    public NativeReference<(bool, Vector3)> MoveToPositionRef = new(Allocator.Persistent);
    public NativeReference<(bool, Vector3)> LookAtPositionRef = new(Allocator.Persistent);

    public Move DesiredMove;
    public Look DesiredLook;

    public void Dispose()
    {
        MoveToPositionRef.Dispose();
        LookAtPositionRef.Dispose();
    }
}
