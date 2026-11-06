using ReBot.Main.AI.State;
using ReBot.Main.AI.State.Blink;
using ReBot.Main.AI.State.Capture;
using ReBot.Main.AI.State.Cardiac;
using ReBot.Main.AI.State.Interaction;
using ReBot.Main.AI.State.Melee;
using ReBot.Main.AI.State.Movement;
using ReBot.Main.AI.State.Rage;
using ReBot.Main.AI.State.Snap;
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

    public ActionStore ActionStore = new();

    public PlayerStore PlayerStore = new();
    public RoomStore RoomStore = new();
    public ObstacleState ObstacleStore = new();
    public TerminationStore TerminationStore = new();

    public MovementStore MovementStore = new();
    public InteractionStore InteractionStore = new();

    public CardiacStore CardiacStore = new();
    public CaptureStore CaptureStore = new();
    public SnapStore SnapStore = new();
    public BlinkStore BlinkStore = new();
    public RageStore RageStore = new();
    public MeleeStore MeleeStore = new();

    public void Dispose()
    {
        ActionStore.Dispose();
        PlayerStore.Dispose();
        RoomStore.Dispose();
        ObstacleStore.Dispose();
        TerminationStore.Dispose();

        MovementStore.Dispose();
        InteractionStore.Dispose();

        CardiacStore.Dispose();
        CaptureStore.Dispose();
        SnapStore.Dispose();
        BlinkStore.Dispose();
        RageStore.Dispose();
        MeleeStore.Dispose();
    }
}
