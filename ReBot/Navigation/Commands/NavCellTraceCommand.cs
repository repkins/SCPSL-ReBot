using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using ReBot.Navigation.Mesh;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(NavCell))]
    internal class NavCellTraceCommand : ICommand
    {
        public string Command { get; } = "trace";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Traces cells path from cached cell to cell at current position.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
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

            if (!NavigationMeshEditor.Instance.TracePath(playerCommandSender.ReferenceHub.transform.position))
            {
                response = $"Failed to trace cells path.";
                return false;
            }

            response = $"Path traced.";
            return true;
        }
    }
}
