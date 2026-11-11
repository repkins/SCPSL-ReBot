using UnityEngine;

namespace ReBot.Main.AI.State.Movement;

internal record struct Look(Quaternion DesiredHorizontalRotation, Quaternion DesiredVerticalRotation, Quaternion TargetHorizontalRotation);
