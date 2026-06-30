using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;

namespace ReBot.AI.Cardiac;

internal static class CardiacStore
{
    public static List<NativeReference<(bool, Vector3)>> CardiacPositionRefs = [];
}
