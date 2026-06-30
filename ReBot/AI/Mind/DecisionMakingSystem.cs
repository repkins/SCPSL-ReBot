using ReBot.AI.Cardiac;
using ReBot.AI.Interaction;
using ReBot.AI.Movement;
using ReBot.AI.Spacial;
using ReBot.AI.Termination;
using Unity.Collections;
using Unity.Jobs;

namespace ReBot.AI.Mind;

internal class DecisionMakingSystem
{
    public static void Process()
    {
        var botCount = BotStore.BotHubs.Count;

        var findActionHandles = new NativeArray<JobHandle>(botCount, Allocator.TempJob);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var findActionJob = new FindActionJob
            {
                playerPosition = PlayerStore.PlayerPositions[botIdx],
                targetPositions = LocationStore.TargetPositions[botIdx],
                obstacleTargets = ObstacleStore.ObstacleTargets[botIdx],
                roomEnterLocationTargets = RoomStore.RoomEnterLocationTargets[botIdx],

                moveToPositionRef = MovementStore.MoveToPositionRefs[botIdx],
                interactPositionRef = InteractionStore.InteractWithPositionRefs[botIdx],

                sightedEnemyTargets = TerminationState.SightedEnemyTargets[botIdx],
                enemiesWithinDistance = TerminationState.EnemiesWithinDistance[botIdx],
                cardiacPositionRef = CardiacStore.CardiacPositionRefs[botIdx],
            };

            findActionHandles[botIdx] = findActionJob.Schedule();
        }

        var decisionMakingHandle = JobHandle.CombineDependencies(findActionHandles);
        decisionMakingHandle.Complete();

        findActionHandles.Dispose();
    }
}
