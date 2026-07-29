using ReBot.Main.AI.Mind.Role;
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
                        var findActionJob = new FindActionJob<Scp049Mind, Scp049Mind.Factory>
                        {
                            botRef = new(botIdx),
                        };

                        findActionHandles[botIdx] = findActionJob.ScheduleByRef();
                    }
                    break;

                case PlayerRoles.RoleTypeId.Scp106:
                    {

                    }
                    break;
            }
        }

        JobHandle.CompleteAll(findActionHandles);
    }
}
