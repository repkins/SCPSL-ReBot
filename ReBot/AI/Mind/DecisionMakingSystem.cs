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
            var botHub = BotStore.BotHubs[botIdx];

            switch (botHub.roleManager.CurrentRole.RoleTypeId)
            {
                case PlayerRoles.RoleTypeId.Scp049:
                    {
                        var findActionJob = new FindActionJob
                        {
                            playerPosition = PlayerStore.PlayerPositions[botIdx],
                            moveToPositionRef = MovementStore.MoveToPositionRefs[botIdx],

                            obstacles = ObstacleStore.ObstaclesByTarget[botIdx],
                            obstacleLocations = ObstacleStore.ObstacleLocations,
                            interactPositionRef = InteractionStore.InteractWithPositionRefs[botIdx],

                            roomEnterLocations = RoomStore.RoomEnterLocations[botIdx],

                            enemiesWithinSight = TerminationState.EnemiesWithinSight[botIdx],
                            enemiesWithinDistance = TerminationState.EnemiesWithinDistance[botIdx],
                            cardiacPositionRef = CardiacStore.CardiacPositionRefs[botIdx],
                        };

                        findActionHandles[botIdx] = findActionJob.Schedule();
                    }
                    break;
            }
        }

        var decisionMakingHandle = JobHandle.CombineDependencies(findActionHandles);
        decisionMakingHandle.Complete();

        findActionHandles.Dispose();
    }
}
