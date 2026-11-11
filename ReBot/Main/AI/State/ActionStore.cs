using System;
using System.Collections.Generic;

namespace ReBot.Main.AI.State;

internal readonly struct ActionStore() : IDisposable
{
    public readonly HashSet<string> WantSet = [];

    public void Dispose()
    {
    }
}
