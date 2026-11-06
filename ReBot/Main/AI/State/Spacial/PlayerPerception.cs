using PlayerRoles.FirstPersonControl;

namespace ReBot.Main.AI.State.Spacial;

internal struct PlayerPerception(int botCount)
{
    public void Process()
    {
        for (var botIdx = 0; botIdx < botCount; botIdx++)
        {
            var bot = BotStore.Bots[botIdx];
            if (bot.Hub.roleManager.CurrentRole is not FpcStandardRoleBase fpcRole)
            {
                continue;
            }

            bot.PlayerTransform = fpcRole.FpcModule.transform;
            bot.CameraTransform = bot.Hub.PlayerCameraReference;

            bot.PlayerStore.CameraPosition = fpcRole.CameraPosition;
            bot.PlayerStore.CameraForward = bot.CameraTransform.forward;
            bot.PlayerStore.Position = fpcRole.FpcModule.Position;
            bot.PlayerStore.Forward = fpcRole.FpcModule.transform.forward;
        }
    }
}
