using CentralAuth;
using MEC;
using Mirror;
using System.Collections.Generic;
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
    }

    public static void RemoveBotPlayer(BotRef botRef)
    {
        var hub = Bots[botRef.Idx].Hub;
        BotHubRefs.Remove(hub);

        Bots[botRef.Idx].Dispose();
        Bots.RemoveAt(botRef.Idx);
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

        Timing.CallDelayed(1f, delegate
        {
            PlayerAuthenticationManager playerAuthManager = player.GetComponent<PlayerAuthenticationManager>();
            playerAuthManager.UserId = $"BotUserId{lastConnNum}";
        });

        return referenceHub;
    }
}
