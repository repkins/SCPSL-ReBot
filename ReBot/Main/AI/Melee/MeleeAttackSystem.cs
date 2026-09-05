namespace ReBot.Main.AI.Melee;

internal class MeleeAttackSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var clawAttackPositionRef = BotStore.Bots[botIdx].Scp939MeleeState.MeleeAttackPositionRef;
            var (wantsClaw, _) = clawAttackPositionRef.Value;
            clawAttackPositionRef.Value = default;

            var slapAttackPositionRef = BotStore.Bots[botIdx].Scp096MeleeState.MeleeAttackPositionRef;
            var (wantsSlap, _) = slapAttackPositionRef.Value;
            slapAttackPositionRef.Value = default;

            if (!wantsClaw && !wantsSlap)
            {
                BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Shoot] = false;
                continue;
            }

            BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Shoot] = true;
        }
    }
}
