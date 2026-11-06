using ReBot.Main.AI.State.Termination;
using System;
using Unity.Collections;

namespace ReBot.Main.AI.State.Rage;

public struct RageStore() : IDisposable
{
    public bool IsEnraged;
    public bool HasTargets;

    public NativeList<EnemyRef> Targets = new(Allocator.Persistent);

    public NativeReference<bool> EnrageRef = new(Allocator.Persistent);
    public NativeReference<bool> CalmRef = new(Allocator.Persistent);

    public void Dispose()
    {
        Targets.Dispose();

        EnrageRef.Dispose();
        CalmRef.Dispose();
    }
}
