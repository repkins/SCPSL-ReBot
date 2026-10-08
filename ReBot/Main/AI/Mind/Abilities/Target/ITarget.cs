using ReBot.Main.AI.Spacial;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Mind.Abilities.Target;

internal interface ITarget
{
    NativeList<Vector3> Locations { get; }
    ActionEnum Enum { get; }
}
