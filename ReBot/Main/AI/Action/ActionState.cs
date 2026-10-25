using System;
using System.Collections.Generic;
using System.Linq;

namespace ReBot.Main.AI.Action;

public readonly struct ActionState() : IDisposable
{
    public readonly HashSet<string> WantSet = [];

    public void Dispose()
    {
    }
}
