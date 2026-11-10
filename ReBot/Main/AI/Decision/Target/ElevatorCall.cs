using ReBot.Main.AI.Decision.Abilities.Target;
using ReBot.Main.AI.State.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Target;

internal struct ElevatorCall() : ITarget
{
    public NativeList<Vector3> Locations { get; } = ObstacleStore.ElevationCallInteractLocations;

    public ActionEnum Enum { get; } = ActionEnum.ElevatorCall;
}
