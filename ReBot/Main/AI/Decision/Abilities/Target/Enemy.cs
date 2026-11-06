using ReBot.Main.AI.Decision;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Target;

internal struct Enemy(BotRef botRef) : ITarget
{
    public NativeList<Vector3> Locations { get; } = botRef.Value.TerminationStore.EnemyLocations;

    public ActionEnum Enum { get; } = ActionEnum.Enemy;
}
