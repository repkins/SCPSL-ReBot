using CommandSystem;
using RemoteAdmin;
using ReBot.Navigation.Mesh;
using System;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(Nav))]
    internal class NavLoadCommand : ICommand
    {
        public string Command { get; } = "load";

        public string[] Aliases { get; } = new string[] { };

        public string Description { get; } = "Re-loads navigation mesh from storage.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerCommandSender)
            {
                response = "You must be in-game to use this command!";
                return false;
            }

            NavigationSystem.Instance.ResetMeshes();
            NavigationSystem.Instance.LoadConnectMeshes();

            response = $"Navigation mesh re-loaded.";
            return true;
        }
    }
}
