using CommandSystem;
using System;
using System.Linq;

namespace ReBot.Navigation.Commands
{
    [CommandHandler(typeof(Nav))]
    internal class NavMesh : ParentCommand
    {
        public override string Command { get; } = "mesh";

        public override string[] Aliases { get; } = new string[] { };

        public override string Description { get; } = "Operations on entire navigation mesh.";

        public override void LoadGeneratedCommands()
        {
            this.RegisterCommand(new NavMeshDuplicateCommand());
        }

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            var subCommands = Commands.Keys.ToArray();
            response = $"Please specify a valid subcommand. ({string.Join("/", subCommands)})";
            return false;
        }
    }
}
