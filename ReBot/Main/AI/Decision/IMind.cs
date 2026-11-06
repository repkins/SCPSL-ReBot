using Unity.Collections;

namespace ReBot.Main.AI.Decision;

public interface IMind
{
    void ProcessGoals(NativeList<BeliefRef> beliefsToEnable);
    void ProcessImpactingAction(ActionRef action, NativeList<BeliefRef> beliefsToEnable);
    void ProcessEnablingBelief(BeliefRef beliefRef, NativeList<ImpactingAction> impactingActions);

    public interface IFactory<TMind>
    {
        TMind Create(BotRef botRef);
    }
}