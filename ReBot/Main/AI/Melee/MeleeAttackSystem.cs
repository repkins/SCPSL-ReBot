namespace ReBot.Main.AI.Melee;

internal class MeleeAttackSystem
{
    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var meleeAttackPositionRef = BotStore.Bots[botIdx].MeleeState.MeleeAttackPositionRef;
            var (wantsAttack, _) = meleeAttackPositionRef.Value;
            meleeAttackPositionRef.Value = default;

            if (!wantsAttack)
            {
                BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Shoot] = false;
                continue;
            }

            BotStore.Bots[botIdx].ActionState.IsHeld[ActionName.Shoot] = true;
        }
    }
}
