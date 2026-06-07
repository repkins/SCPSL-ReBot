using HarmonyLib;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using Mirror.LiteNetLib4Mirror;
using ReBot.Navigation;
using System;
using System.IO;
using System.Reflection;

namespace ReBot
{
    public class LabApiPlugin : Plugin
    {
        public override string Name { get; } = "ReBot";
        public override string Description { get; } = "Bot players plugin";
        public override string Author { get; } = "repkins(19)";
        public override Version Version { get; } = new ("0.0.1");
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);

        private Harmony harmonyInstance;

        public override void Enable()
        {
            harmonyInstance = new Harmony($"ReBot.{DateTime.Now.Ticks}");
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Logger.Info("Patching successful.");

            NavigationSystem.Instance.Init();
            //BotManager.Instance.Init();


            Logger.Info("Enabled plugin.");

            // TODO: remove for release build
            LiteNetLib4MirrorTransport.Singleton.disconnectTimeout = 1000 * 60 * 60 * 1;
        }

        public override void Disable()
        {
            //BotManager.Instance.Teardown();
            NavigationSystem.Instance.Teardown();

            harmonyInstance.UnpatchAll();
            Logger.Info("Unpatching successful.");

            Logger.Info("Disabled plugin.");
        }
    }
}
