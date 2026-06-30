using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Interaction;

internal static class InteractionStore
{
    public static List<NativeReference<(bool, Vector3)>> InteractWithPositionRefs = [];

}
