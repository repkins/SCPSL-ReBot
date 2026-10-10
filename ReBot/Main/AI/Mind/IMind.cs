using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public interface IMind
{
    void ProcessGoals(NativeList<BeliefRef> beliefsToEnable);
    bool ProcessImpactingAction(ActionRef action, NativeList<BeliefRef> beliefsToEnable);
    bool ProcessEnablingBelief(BeliefRef beliefRef, NativeList<ImpactingAction> impactingActions);

    public interface IFactory<TMind>
    {
        TMind Create(BotRef botRef);
    }
}