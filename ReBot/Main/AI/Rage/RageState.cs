using System;
using Unity.Collections;

namespace ReBot.Main.AI.Rage;

public struct RageState() : IDisposable
{
    public bool IsEnraged;
    public bool HasTargets;

    public NativeReference<bool> EnrageRef = new(Allocator.Persistent);
    public NativeReference<bool> CalmRef = new(Allocator.Persistent);

    public void Dispose()
    {
        EnrageRef.Dispose();
        CalmRef.Dispose();
    }
}
