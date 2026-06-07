using System.Collections;
using UnityEngine;

namespace ReBot.AI.Movement;

public record struct Look(Quaternion DesiredHorizontalRotation, Quaternion DesiredVerticalRotation, Quaternion TargetHorizontalRotation);
