using ReBot.Main.AI.State.Blink;
using ReBot.Main.AI.State.Rage;
using ReBot.Main.AI.State.Spacial;
using Unity.Jobs;
using UnityEngine;
using ReBot.Main.AI.State.Termination;

namespace ReBot.Main.AI.State;

internal class PerceptionSystem
{
    private const float maxSightDistance = 30f;
    private static readonly LayerMask sightLayerMask = LayerMask.GetMask("Default", "Door", "Ignore Raycast");
    private static readonly LayerMask obstacleSightLayerMask = LayerMask.GetMask("Default", "Door", "Fence");

    public static void Initialize()
    {
        EnemyPerception.Initialize();
        ObstaclePerception.Initialize();
    }

    public static void Teardown()
    {
        ObstaclePerception.Teardown();
        EnemyPerception.Teardown();
    }

    public static void Process()
    {
        var playerPerception = new PlayerPerception(BotStore.Bots.Count);
        var roomPerception = new RoomPerception(BotStore.Bots.Count);
        var enemyPerception = new EnemyPerception(BotStore.Bots.Count, maxSightDistance, sightLayerMask);
        var obstaclePerception = new ObstaclePerception(BotStore.Bots.Count, maxSightDistance, obstacleSightLayerMask);
        var blinkPerception = new BlinkPerception(BotStore.Bots.Count);
        var ragePerception = new RagePerception(BotStore.Bots.Count);

        enemyPerception.ProcessSightDistance();
        enemyPerception.PrepareEnemiesWithinRadius();

        var enemiesSensingHandle = enemyPerception.ProcessEnemySighting();
        var obstaclesSensingHandle = obstaclePerception.PrepareSensableObstacles();

        JobHandle.CompleteAll(ref enemiesSensingHandle, ref obstaclesSensingHandle);

        enemyPerception.ProcessEnemySightResults();
        ragePerception.Process();
        obstaclePerception.ProcessSensedObstacleResults();

        playerPerception.Process();
        roomPerception.Process();
        blinkPerception.Process();

        obstaclePerception.ProcessSharedElevationState();

        enemyPerception.ProcessEnemiesLostSight();
        enemyPerception.ProcessEnemiesSensedTime();
        enemyPerception.ProcessEnemiesWithinDistance();
    }
}
