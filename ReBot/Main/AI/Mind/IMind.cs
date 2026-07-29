using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public interface IMind
{
    bool ProcessBeliefToEnable(BeliefRef beliefToEnable, NativeList<(float cost, BeliefRef beliefRef)> enablingBeliefs);

    public interface IFactory<TMind>
    {
        TMind Create(BotRef botRef);
    }
}