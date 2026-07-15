using PlayerRoles;

namespace ReBot.Main.AI;

public record struct BotPlayer<TRole>(TRole Role) where TRole : PlayerRoleBase;
