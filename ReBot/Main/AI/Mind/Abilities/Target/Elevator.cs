using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Target;

internal struct Elevator() : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public ActionEnum Enum { get; } = ActionEnum.Elevator;
}
