using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using ReBot.AI.Movement.Pathfinding;
using ReBot.AI.Spacial;
using ReBot.AI.Termination;
using ReBot.Navigation.Mesh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ReBot.AI.Perception;

internal class SightPerceptionSystem
{
    private const float maxSightRadius = 30f;
    private static readonly LayerMask sightLayerMask = LayerMask.GetMask("Default", "Door", "Ignore Raycast");

    public static void Process()
    {
        var enemySightPerception = new EnemySightPerception(BotStore.BotHubs.Count, maxSightRadius, sightLayerMask);
        var obstacleSightPerception = new ObstacleSightPerception(BotStore.BotHubs.Count, maxSightRadius, sightLayerMask);

        enemySightPerception.PrepareEnemiesWithinRadius();

        var enemiesRaycastCommandsHandle = enemySightPerception.ProcessEnemySighting();
        var obstaclesRaycastCommandsHandle = obstacleSightPerception.ProcessObstacleSighting();

        JobHandle.CompleteAll(ref enemiesRaycastCommandsHandle, ref obstaclesRaycastCommandsHandle);

        enemySightPerception.ProcessEnemySightResults();
        obstacleSightPerception.ProcessObstacleSightResults();

        enemySightPerception.ProcessEnemiesWithinDistance();
    }
}
