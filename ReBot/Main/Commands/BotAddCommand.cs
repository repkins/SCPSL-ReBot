using CommandSystem;
using ReBot.Main.AI;
using System;

namespace ReBot.Main.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BotAddCommand : ICommand
    {
        public string Command => "bot_add";

        public string[] Aliases => new string[] { };

        public string Description => "Bot add";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            BotStore.AddBotPlayer();

            response = "Done.";
            return true;
        }
    }
}
