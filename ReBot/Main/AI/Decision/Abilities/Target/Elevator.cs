using ReBot.Main.AI.Decision;
using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Target;

internal struct Elevator() : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationTravelEnterLocations;

    public ActionEnum Enum { get; } = ActionEnum.Elevator;
}
