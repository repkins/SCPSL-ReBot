using System;
using Unity.Collections;

namespace ReBot.Main.AI.Mind.Role;

internal struct Scp106Mind(BotRef botRef) : IMind
{
    public bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> impactingBeliefs)
    {
        switch (beliefToEnable.Type)
        {

        }

        return false;
    }

    public struct Factory : IMind.IFactory<Scp106Mind>
    {
        public readonly Scp106Mind Create(BotRef botRef)
        {
            return new Scp106Mind(botRef);
        }
    }
}
