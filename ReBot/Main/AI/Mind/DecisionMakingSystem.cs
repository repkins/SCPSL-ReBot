using PlayerRoles;
using ReBot.Main.AI.Mind.Role;
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
                case RoleTypeId.Scp049:
                    {
                        var findActionJob = new FindActionJob<Scp049Mind, Scp049Mind.Factory>
                        {
                            botRef = new(botIdx),
                        };

                        findActionHandles[botIdx] = findActionJob.ScheduleByRef();
                    }
                    break;

                case RoleTypeId.Scp106:
                    {
                        var findActionJob = new FindActionJob<Scp106Mind, Scp106Mind.Factory>
                        {
                            botRef = new(botIdx),
                        };

                        findActionHandles[botIdx] = findActionJob.ScheduleByRef();
                    }
                    break;

                case RoleTypeId.Scp173:
                    {
                        var findActionJob = new FindActionJob<Scp173Mind, Scp173Mind.Factory>
                        {
                            botRef = new(botIdx),
                        };

                        findActionHandles[botIdx] = findActionJob.ScheduleByRef();
                    }
                    break;
            }
        }

        JobHandle.CompleteAll(findActionHandles);
    }
}
