using System;
using UnityEngine;

namespace ReBot.Main.AI.Spacial;

public struct PlayerState : IDisposable
{
    public Vector3 Position;
    public Vector3 Forward;
    public Vector3 CameraPosition;
    public Vector3 CameraForward;

    public void Dispose()
    {
    }
}
