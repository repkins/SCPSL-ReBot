using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Rage;

internal struct Calm(BotRef botRef)
{
    NativeReference<bool> calmRef = botRef.Value.RageState.CalmRef;

    public void Impacting(NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;  // calming cost

        impactingActions.Add(cost, ActionEnum.Calms, default);
    }

    public void Enabling()
    {
        calmRef.Value = true;
    }
}
