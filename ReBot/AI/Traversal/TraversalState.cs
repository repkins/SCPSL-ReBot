using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Traversal
{
    internal static class TraversalState
    {
        public static readonly List<NativeList<Vector3>> Targets = [];

        // Indexed by BotRef
        public static readonly List<NativeList<TargetRef>> RoomEnterLocations = [];
    }
}
