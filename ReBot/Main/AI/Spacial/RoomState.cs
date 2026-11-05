using MapGeneration;
using System;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI.Spacial;

public struct RoomState() : IDisposable
{
    public RoomIdentifier PrevRoomWithin;
    public NativeHashMap<int, float> RoomLastVisitTime = new(10, Allocator.Persistent);

    public NativeList<int> RoomEnterInstanceIds = new(Allocator.Persistent);
    public NativeList<Vector3> RoomEnterLocations = new(Allocator.Persistent);

    public void Dispose()
    {
        RoomLastVisitTime.Dispose();
        RoomEnterInstanceIds.Dispose();
        RoomEnterLocations.Dispose();
    }
}
