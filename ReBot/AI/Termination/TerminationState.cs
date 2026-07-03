using ReBot.AI.Movement;
using ReBot.AI.Spacial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Termination;

internal static class TerminationState
{
    public static List<NativeList<Vector3>> SightedEnemyLocations = [];
    public static List<NativeList<bool>> EnemiesWithinDistance = [];
}
