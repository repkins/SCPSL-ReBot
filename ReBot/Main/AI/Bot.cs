using ReBot.LocalNetworking;
using ReBot.Main.AI.Blink;
using ReBot.Main.AI.Capture;
using ReBot.Main.AI.Cardiac;
using ReBot.Main.AI.Interaction;
using ReBot.Main.AI.Melee;
using ReBot.Main.AI.Movement;
using ReBot.Main.AI.NeckSnap;
using ReBot.Main.AI.Rage;
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
    public NeckSnapState NeckSnapState = new();
    public BlinkState BlinkState = new();
    public MeleeState Scp096MeleeState = new();
    public RageState RageState = new();
    public MeleeState Scp939MeleeState = new();

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
        NeckSnapState.Dispose();
        BlinkState.Dispose();
        Scp096MeleeState.Dispose();
        RageState.Dispose();
        Scp939MeleeState.Dispose();
    }
}
