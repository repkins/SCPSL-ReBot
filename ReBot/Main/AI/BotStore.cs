using CentralAuth;
using LabApi.Features.Wrappers;
using MEC;
using Mirror;
using ReBot.Main.AI.Mind;
using ReBot.Main.AI.Movement.Pathfinding;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ReBot.Main.AI;

public static class BotStore
{
    public readonly static List<Bot> Bots = [];
    public readonly static Dictionary<ReferenceHub, BotRef> BotHubRefs = [];

    private static int lastConnNum = 0;

    public static void AddBotPlayer()
    {
        var referenceHub = InstantiatePlayerObject();

        var nextBotIdx = Bots.Count;
        Bots.Add(new(referenceHub));
        BotHubRefs.Add(referenceHub, new(nextBotIdx));

        PathfindingStore.AddBot(new(nextBotIdx));
        MindStore.AddBot(new(nextBotIdx));
    }

    public static void RemoveBotPlayer(BotRef botRef)
    {
        PathfindingStore.RemoveBot(botRef);

        var hub = Bots[botRef.Idx].Hub;
        BotHubRefs.Remove(hub);

        Bots[botRef.Idx].Dispose();
        Bots.RemoveAt(botRef.Idx);

        for (int botIdx = 0; botIdx < Bots.Count; botIdx++)
        {
            var botHub = Bots[botIdx].Hub;
            BotHubRefs[botHub] = new(botIdx);
        }
    }

    private static ReferenceHub InstantiatePlayerObject()
    {
        var player = Object.Instantiate(NetworkManager.singleton.playerPrefab);
        player.name = string.Format("{0} [bot]", NetworkManager.singleton.playerPrefab.name);

        var connectionToClient = new LocalNetworking.LocalConnectionToClient(--lastConnNum);
        var connectionToServer = new LocalNetworking.LocalConnectionToServer() { connectionToClient = connectionToClient };
        connectionToClient.connectionToServer = connectionToServer;

        Debug.Log($"connectionToClient = {connectionToClient}");
        //NetworkDiagnostics.InMessageEvent += LogInMessage;

        NetworkServer.AddConnection(connectionToClient);
        NetworkServer.AddPlayerForConnection(connectionToClient, player);
        NetworkServer.OnConnectedEvent?.Invoke(connectionToClient);

        var referenceHub = player.GetComponent<ReferenceHub>();
        Debug.Log($"referenceHub = {referenceHub}");
        Debug.Log($"connectionToClient.identity = {connectionToClient.identity}");

        Timing.CallDelayed(1f, () => AssignUserId(player));

        return referenceHub;
    }

    private static void AssignUserId(GameObject player)
    {
        PlayerAuthenticationManager playerAuthManager = player.GetComponent<PlayerAuthenticationManager>();
        playerAuthManager.UserId = $"BotUserId{lastConnNum}";
    }
}
