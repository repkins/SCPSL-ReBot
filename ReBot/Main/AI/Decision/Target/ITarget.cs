using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Decision.Abilities.Target;

internal interface ITarget
{
    NativeList<Vector3> Locations { get; }
    ActionEnum Enum { get; }
}
