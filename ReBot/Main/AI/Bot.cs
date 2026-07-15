using ReBot.Main.AI.Cardiac;
using ReBot.Main.AI.Interaction;
using ReBot.Main.AI.Movement;
using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using System;
using UnityEngine;

namespace ReBot.Main.AI;

public class Bot(ReferenceHub hub) : IDisposable
{
    public readonly ReferenceHub Hub = hub;

    public Transform PlayerTransform;
    public Transform CameraTransform;

    public PlayerState PlayerState;
    public RoomState RoomState;
    public ObstacleState ObstacleState;
    public TerminationState TerminationState;

    public MovementState MovementState;
    public InteractionState InteractionState;
    public CardiacState CardiacState;

    public void Dispose()
    {
        RoomState.Dispose();
        ObstacleState.Dispose();
        TerminationState.Dispose();

        MovementState.Dispose();
        InteractionState.Dispose();
    }
}
