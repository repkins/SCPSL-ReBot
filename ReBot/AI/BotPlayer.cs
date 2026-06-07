using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using System.Collections;
using UnityEngine;

namespace ReBot.AI;

public record struct BotPlayer<TRole>(TRole Role) where TRole : PlayerRoleBase;
