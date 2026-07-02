using MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal static class RoomStore
{
    public static readonly List<RoomIdentifier> PrevRoomWithin = [];
    public static readonly Dictionary<RoomIdentifier, float> RoomLastVisitTime = [];

    public static readonly List<NativeList<TargetRef>> RoomEnterLocationTargets = [];
}
