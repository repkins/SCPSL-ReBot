using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal static class PlayerStore
{
    public static readonly List<Vector3> PlayerPositions = [];

    public static readonly List<Vector3> PlayerCameraPositions = [];
    public static readonly List<Vector3> PlayerCameraForwards = [];
}
