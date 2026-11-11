using Unity.Collections;

namespace ReBot.Main.AI.Decision;

internal static class NativeContainerExtensions
{
    public static void Add(this NativeList<ImpactingAction> impactingActions, float cost, float multiplier, ActionEnum actionEnum, int beliefIdx)
    {
        impactingActions.Add(new(cost, multiplier, new(actionEnum, beliefIdx)));
    }

    public static void Add(this NativeList<ImpactingAction> impactingActions, float cost, ActionEnum actionEnum, int beliefIdx)
    {
        impactingActions.Add(new(cost, 1f, new(actionEnum, beliefIdx)));
    }
}
