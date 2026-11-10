using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using ReBot.Navigation.Mesh;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(NavCell))]
    internal class NavCellCacheCommand : ICommand
    {
        public string Command { get; } = "cache";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Caches navigation graph cell at current position.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission(PlayerPermissions.FacilityManagement, out response))
            {
                return false;
            }

            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            if (!playerCommandSender.ReferenceHub.IsAlive())
            {
                response = "Command disabled when you are not alive!";
                return false;
            }

            if (!NavigationMeshEditor.Instance.CacheNearestCell())
            {
                response = $"Failed to cache cell.";
                return false;
            }

            response = $"Cell cached.";
            return true;
        }
    }
}
