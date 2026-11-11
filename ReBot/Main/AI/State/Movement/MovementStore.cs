using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.State.Movement;

internal struct MovementStore() : IDisposable
{
    public NativeReference<(bool, Vector3)> MoveToPositionRef = new(Allocator.Persistent);
    public NativeReference<(bool, Vector3)> LookAtPositionRef = new(Allocator.Persistent);

    public Move DesiredMove;
    public Look DesiredLook;

    public bool JumpIsHeld;

    public void Dispose()
    {
        MoveToPositionRef.Dispose();
        LookAtPositionRef.Dispose();
    }
}
