namespace ReBot.Main.AI.Melee;

internal class MeleeAttackSystem
{
    static readonly string ReleaseShootDummyActionName = $"{ActionName.Shoot}->Release";
    static readonly string HoldShootDummyActionName = $"{ActionName.Shoot}->Hold";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var meleeAttackPositionRef = BotStore.Bots[botIdx].MeleeState.MeleeAttackPositionRef;
            var (wantsAttack, _) = meleeAttackPositionRef.Value;
            meleeAttackPositionRef.Value = default;

            if (!wantsAttack)
            {
                BotStore.Bots[botIdx].ActionState.WantSet.Add(ReleaseShootDummyActionName);
                continue;
            }

            BotStore.Bots[botIdx].ActionState.WantSet.Add(HoldShootDummyActionName);
        }
    }
}
