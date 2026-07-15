using CommandSystem;
using PlayerRoles;
using RemoteAdmin;
using ReBot.Navigation.Mesh;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(NavVertex))]
    internal class NavVertexSelectClearCommand : ICommand
    {
        public string Command { get; } = "select_clear";

        public string[] Aliases { get; } = new string[] { "nvsc" };

        public string Description { get; } = "Clears navigation mesh vertex selection.";

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

            NavigationMeshEditor.Instance.ClearVertexSelection();

            response = $"Vertex selection cleared.";
            return true;
        }
    }
}
