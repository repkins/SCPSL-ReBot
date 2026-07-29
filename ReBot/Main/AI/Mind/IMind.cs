using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public interface IMind
{
    bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> enablingBeliefs);

    public interface IFactory<TMind>
    {
        TMind Create(BotRef botRef);
    }
}