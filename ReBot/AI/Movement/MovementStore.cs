using PlayerRoles.FirstPersonControl;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI.Movement;

public static class MovementStore
{
    public static List<(BotRef, FpcStandardRoleBase)> CurrentFpcPlayerRoles = [];
    public static Dictionary<ReferenceHub, FpcRoleRef> BotFpcRoleIds = [];

    public static Dictionary<FpcRoleRef, Vector3> MoveToPositions = [];

    public static List<Vector3> CameraPositions = [];
    public static List<Transform> PlayerTransforms = [];
    public static List<Transform> CameraTransforms = [];

    public static List<Move> DesiredMoves = [];
    public static List<Look> DesiredLooks = [];
}
