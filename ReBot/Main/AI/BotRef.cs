namespace ReBot.Main.AI;

public readonly record struct BotRef(int Idx)
{
    public Bot Value => BotStore.Bots[Idx];
}
