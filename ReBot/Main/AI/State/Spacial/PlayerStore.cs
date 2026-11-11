using System;
using UnityEngine;

namespace ReBot.Main.AI.State.Spacial;

internal struct PlayerStore : IDisposable
{
    public Vector3 Position;
    public Vector3 Forward;
    public Vector3 CameraPosition;
    public Vector3 CameraForward;

    public void Dispose()
    {
    }
}
