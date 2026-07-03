using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal static class ObstacleStore
{
    public static readonly List<NativeHashMap<Vector3, ObstacleRef>> Obstacles = [];
    public static readonly List<NativeList<Vector3>> ObstacleLocations = [];
}
