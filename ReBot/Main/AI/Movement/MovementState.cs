using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Movement;

public struct MovementState() : IDisposable
{
    public NativeReference<(bool, Vector3)> MoveToPositionRef = new(Allocator.Persistent);

    public Move DesiredMove;
    public Look DesiredLook;

    public void Dispose()
    {
        MoveToPositionRef.Dispose();
    }
}
