namespace ReBot.Main.AI.State.Melee;

internal class MeleeAttackSystem
{
    static readonly string ShootDummyActionName = $"{ActionName.Shoot}->Click";

    public static void Process()
    {
        for (var botIdx = 0; botIdx < BotStore.Bots.Count; botIdx++)
        {
            var meleeAttackPositionRef = BotStore.Bots[botIdx].MeleeStore.MeleeAttackPositionRef;
            var (wantsAttack, _) = meleeAttackPositionRef.Value;
            meleeAttackPositionRef.Value = default;

            var isHeld = BotStore.Bots[botIdx].MeleeStore.IsHeld;
            var prevIsHeld = isHeld;

            if (wantsAttack && !isHeld)
            {
                isHeld = true;
            }
            else
            {
                isHeld = false;
            }

            BotStore.Bots[botIdx].MeleeStore.IsHeld = isHeld;

            if (prevIsHeld != isHeld)
            {
                if (isHeld)
                {
                    BotStore.Bots[botIdx].ActionStore.WantSet.Add(ShootDummyActionName);
                }
            }
        }
    }
}
