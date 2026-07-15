using ReBot.Main.AI.Cardiac;
using ReBot.Main.AI.Interaction;
using ReBot.Main.AI.Movement;
using ReBot.Main.AI.Spacial;
using ReBot.Main.AI.Termination;
using Unity.Collections;
using Unity.Jobs;

namespace ReBot.Main.AI.Mind;

internal class DecisionMakingSystem
{
    public static void Process()
    {
        var botCount = BotStore.Bots.Count;

        var findActionHandles = new NativeArray<JobHandle>(botCount, Allocator.TempJob);

        for (int botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];

            switch (bot.Hub.roleManager.CurrentRole.RoleTypeId)
            {
                case PlayerRoles.RoleTypeId.Scp049:
                    {
                        var findActionJob = new FindActionJob
                        {
                            playerPosition = bot.PlayerState.Position,
                            moveToPositionRef = bot.MovementState.MoveToPositionRef,

                            obstacles = bot.ObstacleState.ObstaclesByTarget,
                            obstacleLocations = ObstacleState.ObstacleLocations,
                            interactPositionRef = bot.InteractionState.InteractWithPositionRef,

                            roomEnterLocations = bot.RoomState.RoomEnterLocations,

                            enemiesWithinSight = bot.TerminationState.EnemiesWithinSight,
                            enemiesWithinDistance = bot.TerminationState.EnemiesWithinDistance,
                            cardiacPositionRef = bot.CardiacState.CardiacPositionRef,
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
