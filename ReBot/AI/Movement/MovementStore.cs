using PlayerRoles.FirstPersonControl;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Movement;

public static class MovementStore
{
    public static List<NativeReference<(bool, Vector3)>> MoveToPositionRefs = [];

    // TODO: To Perception
    public static List<Vector3> CameraPositions = [];
    public static List<Vector3> PlayerPositions = [];
    public static List<Transform> PlayerTransforms = [];
    public static List<Transform> CameraTransforms = [];

    public static List<Move> DesiredMoves = [];
    public static List<Look> DesiredLooks = [];
}
