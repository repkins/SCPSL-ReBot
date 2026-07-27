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
        ObstacleSensePerception.Initialize();
    }

    public static void Teardown()
    {
        ObstacleSensePerception.Teardown();
    }

    public static void Process()
    {
        var enemySightPerception = new EnemySightPerception(BotStore.Bots.Count, maxSightRadius, sightLayerMask);
        var obstacleSensePerception = new ObstacleSensePerception(BotStore.Bots.Count, maxSightRadius, sightLayerMask);
        var roomPerception = new RoomPerception(BotStore.Bots.Count);

        enemySightPerception.PrepareEnemiesWithinRadius();

        var enemiesSightingHandle = enemySightPerception.ProcessEnemySighting();
        var obstaclesSensingHandle = obstacleSensePerception.PrepareSensableObstacles();

        JobHandle.CompleteAll(ref enemiesSightingHandle, ref obstaclesSensingHandle);

        enemySightPerception.ProcessEnemySightResults();
        obstacleSensePerception.ProcessSensedObstacleResults();

        obstacleSensePerception.ProcessSharedElevationState();
        enemySightPerception.ProcessEnemiesWithinDistance();

        roomPerception.Process();
    }
}
