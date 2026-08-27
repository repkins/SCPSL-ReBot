using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities;

internal struct Scp096Rage(BotRef botRef)
{
    NativeReference<bool> enrageRef = botRef.Value.RageState.EnrageRef;
    NativeReference<bool> calmRef = botRef.Value.RageState.CalmRef;

    public bool Enrages()
    {
        enrageRef.Value = true;

        return true;
    }

    public bool Calms()
    {
        calmRef.Value = true;

        return true;
    }
}
