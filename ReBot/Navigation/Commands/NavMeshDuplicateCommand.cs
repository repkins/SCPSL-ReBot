using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using ReBot.Navigation.Mesh;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(NavMesh))]
    internal class NavMeshDuplicateCommand : ICommand
    {
        public string Command { get; } = "duplicate";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Duplicates navigation mesh at current room from specified room form.";

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

            if (arguments.Count < 1)
            {
                response = "Please specify room form identifier!";
                return false;
            }

            var roomForm = arguments[0];

            var playerPosition = playerCommandSender.ReferenceHub.transform.position;

            if (!NavigationMeshEditor.Instance.DuplicateMesh(playerPosition, roomForm))
            {
                response = "Could not duplicate mesh!";
                return false;
            }

            response = $"Mesh duplicated.";
            return true;
        }
    }
}
