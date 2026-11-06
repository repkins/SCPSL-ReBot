using ReBot.Main.AI.Decision;
using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Target;

internal struct Obstacle() : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ObstacleLocations;

    public ActionEnum Enum { get; } = ActionEnum.Obstacle;
}
