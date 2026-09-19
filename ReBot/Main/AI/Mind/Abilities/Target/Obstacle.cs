using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Target;

internal struct Obstacle() : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ObstacleLocations;

    public BeliefEnum Enum { get; } = BeliefEnum.Obstacle;
}
