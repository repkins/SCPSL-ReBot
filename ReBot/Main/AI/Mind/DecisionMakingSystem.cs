using ReBot.Main.AI.Spacial;
using Unity.Collections;
using Unity.Jobs;

namespace ReBot.Main.AI.Mind;

internal class DecisionMakingSystem
{
    public static void Process()
    {
        var botCount = BotStore.Bots.Count;

        var findActionHandles = new NativeArray<JobHandle>(botCount, Allocator.Temp);

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

                            obstaclesByTarget = bot.ObstacleState.ObstaclesByTarget,
                            obstacleLocations = ObstacleState.ObstacleLocations,
                            elevationTravelObstacles = ObstacleState.ElevationTravelObstacles,
                            elevationTravelInteractLocations = ObstacleState.ElevationTravelInteractLocations,
                            elevationTravelEnterLocations = ObstacleState.ElevationTravelEnterLocations,
                            elevationCallObstacles = ObstacleState.ElevationCallObstacles,
                            elevationCallInteractLocations = ObstacleState.ElevationCallInteractLocations,
                            interactPositionRef = bot.InteractionState.InteractWithPositionRef,

                            roomEnterLocations = bot.RoomState.RoomEnterLocations,

                            enemiesWithinSight = bot.TerminationState.EnemiesWithinSight,
                            enemiesWithinDistance = bot.TerminationState.EnemiesWithinDistance,
                            cardiacPositionRef = bot.CardiacState.CardiacPositionRef,
                        };

                        findActionHandles[botIdx] = findActionJob.ScheduleByRef();
                    }
                    break;
            }
        }

        var decisionMakingHandle = JobHandle.CombineDependencies(findActionHandles);
        decisionMakingHandle.Complete();
    }
}
