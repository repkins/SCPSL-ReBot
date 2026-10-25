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

            var isHeld = BotStore.Bots[botIdx].MeleeState.IsHeld;
            var prevIsHeld = isHeld;

            if (wantsAttack)
            {
                isHeld = true;
            }
            else
            {
                isHeld = false;
            }

            BotStore.Bots[botIdx].MeleeState.IsHeld = isHeld;

            if (prevIsHeld != isHeld)
            {
                if (isHeld)
                {
                    BotStore.Bots[botIdx].ActionState.WantSet.Add(HoldShootDummyActionName);
                }
                else
                {
                    BotStore.Bots[botIdx].ActionState.WantSet.Add(ReleaseShootDummyActionName);
                }
            }
        }
    }
}
