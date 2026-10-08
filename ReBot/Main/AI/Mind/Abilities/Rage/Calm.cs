using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Rage;

internal struct Calm(BotRef botRef)
{
    NativeReference<bool> calmRef = botRef.Value.RageState.CalmRef;

    public bool Process()
    {
        calmRef.Value = true;

        return true;
    }
}
