using PlayerRoles.FirstPersonControl;

namespace ReBot.Main.AI.Spacial;

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

            bot.PlayerState.CameraPosition = fpcRole.CameraPosition;
            bot.PlayerState.CameraForward = bot.CameraTransform.forward;
            bot.PlayerState.Position = fpcRole.FpcModule.Position;
            bot.PlayerState.Forward = fpcRole.FpcModule.transform.forward;
        }
    }
}
