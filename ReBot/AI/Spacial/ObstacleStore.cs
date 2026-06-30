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
    public static readonly List<NativeList<(bool, TargetRef)>> ObstacleTargets = [];
}
