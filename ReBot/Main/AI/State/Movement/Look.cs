using UnityEngine;

namespace ReBot.Main.AI.State.Movement;

public record struct Look(Quaternion DesiredHorizontalRotation, Quaternion DesiredVerticalRotation, Quaternion TargetHorizontalRotation);
