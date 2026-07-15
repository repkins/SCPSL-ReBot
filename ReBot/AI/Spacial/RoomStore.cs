using MapGeneration;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Spacial;

internal static class RoomStore
{
    public static readonly List<RoomIdentifier> PrevRoomWithin = [];
    public static readonly List<Dictionary<RoomIdentifier, float>> RoomLastVisitTime = [];

    public static readonly List<NativeList<Vector3>> RoomEnterLocations = [];
}
