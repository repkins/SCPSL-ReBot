using ReBot.Main.AI.Decision.Abilities.Target;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Target;

internal struct Enemy(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.TerminationStore.EnemyLocations;

    public ActionEnum Enum { get; } = ActionEnum.Enemy;
}
