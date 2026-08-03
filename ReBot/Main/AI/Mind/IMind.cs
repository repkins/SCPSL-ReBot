using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public interface IMind
{
    bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<ImpactingBelief> enablingBeliefs);
    void ProcessGoals(NativeList<ImpactingBelief> impactingBeliefs);

    public interface IFactory<TMind>
    {
        TMind Create(BotRef botRef);
    }
}