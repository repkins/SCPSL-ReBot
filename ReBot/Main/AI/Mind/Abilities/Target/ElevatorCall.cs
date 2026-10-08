using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Target;

internal struct ElevatorCall() : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleState.ElevationCallInteractLocations;

    public ActionEnum Enum { get; } = ActionEnum.ElevatorCall;
}
