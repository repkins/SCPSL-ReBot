using CommandSystem;
using NetworkManagerUtils.Dummies;
using ReBot.Main.AI;
using System;

namespace ReBot.Main.Commands
{
	[CommandHandler(typeof(GameConsoleCommandHandler))]
	internal class BotAddCommand : ICommand
	{
		public string Command => "rebot_add";

        public string[] Aliases => [];

		public string Description => "Spawn dummy with bot + id suffix";

		public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
		{
            var botIdx = BotStore.Bots.Count;
            DummyUtils.SpawnDummy($"ReBot{botIdx}");

			response = "Done.";
			return true;
		}
	}
}
