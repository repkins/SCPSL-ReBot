using HarmonyLib;
using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features;
using LabApi.Features.Console;
using LabApi.Loader.Features.Plugins;
using Mirror.LiteNetLib4Mirror;
using PlayerRoles;
using ReBot.Main.AI;
using ReBot.Navigation;
using ReBot.Navigation.Mesh;
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

            NavigationSystem.Instance.BaseDir = Path.Combine(Path.GetDirectoryName(FilePath), Path.GetFileNameWithoutExtension(FilePath));
            if (!Directory.Exists(NavigationSystem.Instance.BaseDir))
            {
                Directory.CreateDirectory(NavigationSystem.Instance.BaseDir);
            }
            NavigationSystem.Instance.Init();
            NavigationMeshEditor.Instance.Init();

            BotSystem.Initalize();

            Logger.Info("Enabled plugin.");

            // TODO: remove for release build
            LiteNetLib4MirrorTransport.Singleton.disconnectTimeout = 1000 * 60 * 60 * 1;

            PlayerRoleManager.OnRoleChanged += OnRoleChanged;
            ReferenceHub.OnPlayerRemoved += OnPlayerRemoved;
            ServerEvents.RoundRestarted += OnRoundRestarted;
            PlayerEvents.Kicked += Kicked;
            ServerEvents.RoundStarted += RoundStarted;

            //var seed = (int)DateTime.Now.Ticks;
            var seed = -810608422;
            Logger.Info($"Seed value: {seed}");
            UnityEngine.Random.InitState(seed);
        }

        public override void Disable()
        {
            PlayerRoleManager.OnRoleChanged -= OnRoleChanged;
            ReferenceHub.OnPlayerRemoved -= OnPlayerRemoved;
            ServerEvents.RoundRestarted -= OnRoundRestarted;
            PlayerEvents.Kicked -= Kicked;
            ServerEvents.RoundStarted -= RoundStarted;

            BotSystem.Teardown();

            NavigationMeshEditor.Instance.Teardown();
            NavigationSystem.Instance.Teardown();

            harmonyInstance.UnpatchAll();
            Logger.Info("Unpatching successful.");

            Logger.Info("Disabled plugin.");
        }

        private static void OnRoundRestarted()
        {

        }

        private static void OnRoleChanged(ReferenceHub userHub, PlayerRoleBase prevRole, PlayerRoleBase newRole)
        {
            if (BotStore.BotHubRefs.TryGetValue(userHub, out var botRef))
            {
                BotSystem.PrintRoleChanged(userHub, prevRole, newRole);
            }
        }

        private static void OnPlayerRemoved(ReferenceHub userHub)
        {
            if (BotStore.BotHubRefs.TryGetValue(userHub, out var botRef))
            {
                BotSystem.RemoveBotPlayer(userHub, botRef);
            }
        }

        private static void Kicked(PlayerKickedEventArgs args)
        {
            var hub = args.Player.ReferenceHub;
            if (BotStore.BotHubRefs.ContainsKey(hub))
            {
                BotSystem.PrintKickReason(hub, args.Reason);
            }
        }

        private static void RoundStarted()
        {
            BotSystem.AssignSupportedRole();
        }
    }
}
