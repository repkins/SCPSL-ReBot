using Unity.Jobs;
using UnityEngine;

namespace ReBot.Main.AI.Perception;

internal class SightPerceptionSystem
{
    private const float maxSightRadius = 30f;
    private static readonly LayerMask sightLayerMask = LayerMask.GetMask("Default", "Door", "Ignore Raycast");

    public static void Process()
    {
        var enemySightPerception = new EnemySightPerception(BotStore.Bots.Count, maxSightRadius, sightLayerMask);
        var obstacleSightPerception = new ObstacleSightPerception(BotStore.Bots.Count, maxSightRadius, sightLayerMask);

        enemySightPerception.PrepareEnemiesWithinRadius();

        var enemiesRaycastCommandsHandle = enemySightPerception.ProcessEnemySighting();
        var obstaclesRaycastCommandsHandle = obstacleSightPerception.ProcessObstacleSighting();

        JobHandle.CompleteAll(ref enemiesRaycastCommandsHandle, ref obstaclesRaycastCommandsHandle);

        enemySightPerception.ProcessEnemySightResults();
        obstacleSightPerception.ProcessObstacleSightResults();

        enemySightPerception.ProcessEnemiesWithinDistance();
    }
}
