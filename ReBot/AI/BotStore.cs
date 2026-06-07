using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.PlayableScps.Scp049;
using System.Collections.Generic;
using UnityEngine;

namespace ReBot.AI;

public static class BotStore
{
    public static Dictionary<ReferenceHub, BotRef> BotHubIds = [];
    public static List<BotHub> BotHubs = [];
}
