using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public interface IMind
{
    bool ProcessBeliefToEnable((BeliefEnum beliefEnum, BeliefRef beliefRef) beliefToEnable, NativeList<(float cost, (BeliefEnum beliefEnum, BeliefRef beliefRef))> enablingBeliefs);
}