using Unity.Collections;

namespace ReBot.Main.AI.Mind.Abilities.Rage;

internal struct EnrageEnabling(BotRef botRef)
{
    NativeReference<bool> enrageRef = botRef.Value.RageState.EnrageRef;

    public bool Process()
    {
        enrageRef.Value = true;

        return true;
    }
}
