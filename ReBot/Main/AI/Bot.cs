using ReBot.Main.AI.State;
using ReBot.Main.AI.State.Blink;
using ReBot.Main.AI.State.Capture;
using ReBot.Main.AI.State.Cardiac;
using ReBot.Main.AI.State.Interaction;
using ReBot.Main.AI.State.Melee;
using ReBot.Main.AI.State.Movement;
using ReBot.Main.AI.State.NeckSnap;
using ReBot.Main.AI.State.Rage;
using ReBot.Main.AI.State.Spacial;
using ReBot.Main.AI.State.Termination;
using System;
using UnityEngine;

namespace ReBot.Main.AI;

public class Bot(ReferenceHub hub) : IDisposable
{
    public readonly ReferenceHub Hub = hub;

    public Transform PlayerTransform;
    public Transform CameraTransform;

    public ActionState ActionState = new();

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
    public RageState RageState = new();
    public MeleeState MeleeState = new();

    public void Dispose()
    {
        ActionState.Dispose();
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
        RageState.Dispose();
        MeleeState.Dispose();
    }
}
