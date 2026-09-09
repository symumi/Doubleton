namespace BalartroLike.Battle
{
    public sealed class EnchantInstance
    {
        public int Order { get; }
        public TrigramId Source { get; }
        public int RemainingTurns { get; private set; }

        public EnchantInstance(int order, TrigramId source, int remainingTurns)
        {
            Order = order;
            Source = source;
            RemainingTurns = remainingTurns;
        }

        public void TickTurn()
        {
            if (RemainingTurns > 0)
            {
                RemainingTurns--;
            }
        }

        public bool IsExpired()
        {
            return RemainingTurns == 0;
        }
    }
}