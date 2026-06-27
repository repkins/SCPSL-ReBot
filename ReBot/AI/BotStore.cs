using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI;

public static class BotStore
{
    public static List<ReferenceHub> BotHubs = [];
    public static Dictionary<ReferenceHub, BotRef> BotHubRefs = [];
}
