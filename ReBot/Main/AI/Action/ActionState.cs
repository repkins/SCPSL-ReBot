using System;
using System.Collections.Generic;
using System.Linq;

namespace ReBot.Main.AI.Action;

public readonly struct ActionState() : IDisposable
{
    public readonly Dictionary<ActionName, bool> IsHeld = Enum.GetValues(typeof(ActionName))
        .Cast<ActionName>()
        .ToDictionary(a => a, _ => default(bool));

    public void Dispose()
    {
    }
}
