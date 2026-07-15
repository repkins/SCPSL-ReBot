using UnityEngine;

namespace ReBot.Main.AI.Movement;

public record struct Look(Quaternion DesiredHorizontalRotation, Quaternion DesiredVerticalRotation, Quaternion TargetHorizontalRotation);
