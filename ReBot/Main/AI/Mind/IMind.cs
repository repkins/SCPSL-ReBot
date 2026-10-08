using Unity.Collections;

namespace ReBot.Main.AI.Mind;

public interface IMind
{
    bool ProcessActionToEnable(ActionRef actionToEnable, NativeList<ImpactingAction> enablingBeliefs);
    void ProcessGoals(NativeList<ImpactingAction> impactingActions);

    public interface IFactory<TMind>
    {
        TMind Create(BotRef botRef);
    }
}