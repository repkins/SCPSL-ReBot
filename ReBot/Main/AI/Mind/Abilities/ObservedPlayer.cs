using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct ObservablePlayer(BotRef botRef)
{
    public bool isObserved = botRef.Value.BlinkState.IsObserved;
}
