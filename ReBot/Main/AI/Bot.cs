using ReBot.LocalNetworking;
using ReBot.Main.AI.Capture;
using ReBot.Main.AI.Cardiac;
using ReBot.Main.AI.Interaction;
using ReBot.Main.AI.Movement;
using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using UnityEngine;

namespace ReBot.Main.AI;

public class Bot(ReferenceHub hub, LocalConnectionToServer connectionToServer) : IDisposable
{
    public readonly ReferenceHub Hub = hub;
    public readonly LocalConnectionToServer ConnectionToServer = connectionToServer;

    public Transform PlayerTransform;
    public Transform CameraTransform;

    public PlayerState PlayerState = new();
    public RoomState RoomState = new();
    public ObstacleState ObstacleState = new();
    public TerminationState TerminationState = new();

    public MovementState MovementState = new();
    public InteractionState InteractionState = new();

    public CardiacState CardiacState = new();
    public CaptureState CaptureState = new();

    public void Dispose()
    {
        PlayerState.Dispose();
        RoomState.Dispose();
        ObstacleState.Dispose();
        TerminationState.Dispose();

        MovementState.Dispose();
        InteractionState.Dispose();

        CardiacState.Dispose();
        CaptureState.Dispose();
    }
}
