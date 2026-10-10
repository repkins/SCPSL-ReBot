using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Rage;

internal struct Enrage(BotRef botRef)
{
    NativeReference<bool> enrageRef = botRef.Value.RageState.EnrageRef;

    public bool Impacting(NativeList<ImpactingAction> impactingActions)
    {
        var cost = 5f;  // entering rage cost

        impactingActions.Add(cost, ActionEnum.Enrages, default);

        return false;
    }

    public bool Enabling()
    {
        enrageRef.Value = true;

        return true;
    }
}
