using Unity.Collections;

namespace ReBot.Main.AI.Decision.Abilities.Rage;

internal struct Enrage(BotRef botRef)
{
    NativeReference<bool> enrageRef = botRef.Value.RageState.EnrageRef;

    public void Impacting(NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;  // entering rage cost

        impactingActions.Add(cost, ActionEnum.Enrages, default);
    }

    public void Enabling()
    {
        enrageRef.Value = true;
    }
}
