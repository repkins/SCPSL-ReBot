using ReBot.Main.AI.Blink;
using ReBot.Main.AI.Rage;
using ReBot.Main.AI.Spacial;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

internal class PerceptionSystem
{
    private const float maxSightRadius = 30f;
    private static readonly LayerMask sightLayerMask = LayerMask.GetMask("Default", "Door", "Ignore Raycast");

    public static void Initialize()
    {
        ObstaclePerception.Initialize();
    }

    public static void Teardown()
    {
        ObstaclePerception.Teardown();
    }

    public static void Process()
    {
        var playerPerception = new PlayerPerception(BotStore.Bots.Count);
        var roomPerception = new RoomPerception(BotStore.Bots.Count);
        var enemySightPerception = new EnemySightPerception(BotStore.Bots.Count, maxSightRadius, sightLayerMask);
        var obstaclePerception = new ObstaclePerception(BotStore.Bots.Count, maxSightRadius, sightLayerMask);
        var blinkPerception = new BlinkPerception(BotStore.Bots.Count);
        var ragePerception = new RagePerception(BotStore.Bots.Count);

        enemySightPerception.PrepareEnemiesWithinRadius();

        var enemiesSightingHandle = enemySightPerception.ProcessEnemySighting();
        var obstaclesSensingHandle = obstaclePerception.PrepareSensableObstacles();

        JobHandle.CompleteAll(ref enemiesSightingHandle, ref obstaclesSensingHandle);

        enemySightPerception.ProcessEnemySightResults();
        obstaclePerception.ProcessSensedObstacleResults();

        playerPerception.Process();
        roomPerception.Process();
        blinkPerception.Process();
        ragePerception.Process();

        obstaclePerception.ProcessSharedElevationState();

        enemySightPerception.ProcessEnemiesLostSight();
        enemySightPerception.ProcessEnemiesWithinDistance();
    }
}
