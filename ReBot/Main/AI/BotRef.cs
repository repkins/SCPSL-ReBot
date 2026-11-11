namespace ReBot.Main.AI;

internal readonly record struct BotRef(int Idx)
{
    public Bot Value => BotStore.Bots[Idx];
}
