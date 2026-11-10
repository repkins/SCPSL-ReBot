using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using ReBot.Navigation.Mesh;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(Nav))]
    internal class NavEditCommand : ICommand
    {
        public string Command { get; } = "edit";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Toggles editing of nav mesh.";

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

            var navMeshEditor = NavigationMeshEditor.Instance;

            navMeshEditor.PlayerEditing = navMeshEditor.PlayerEditing == null ? Player.Get(playerCommandSender) : null;

            response = $"Nav mesh editing is now {navMeshEditor.PlayerEditing}.";
            return true;
        }
    }
}
