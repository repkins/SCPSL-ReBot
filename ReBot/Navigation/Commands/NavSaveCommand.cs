using CommandSystem;
using RemoteAdmin;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(Nav))]
    internal class NavSaveCommand : ICommand
    {
        public string Command { get; } = "save";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Saves navigation mesh to storage.";

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

            NavigationSystem.Instance.SaveMeshes(NavigationSystem.Instance.MeshFileName);

            response = $"Navigation mesh saved.";
            return true;
        }
    }
}
