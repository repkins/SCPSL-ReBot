using MapGeneration;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Spacial;

public struct RoomState() : IDisposable
{
    public RoomIdentifier PrevRoomWithin;
    public Dictionary<RoomIdentifier, float> RoomLastVisitTime = [];
    public NativeList<Vector3> RoomEnterLocations = new(Allocator.Persistent);

    public void Dispose()
    {
        RoomEnterLocations.Dispose();
    }
}
