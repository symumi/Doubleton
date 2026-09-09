namespace BalartroLike.Battle
{
    public sealed class EnchantInstance
    {
        public int Order { get; }
        public TrigramId Source { get; }
        public EffectOperation Effect { get; }
        public int RemainingTurns { get; private set; }

        public EnchantInstance(int order, TrigramId source, EffectOperation effect)
        {
            Order = order;
            Source = source;
            Effect = effect;
            RemainingTurns = effect.Duration;
        }

        public void RestoreRemainingTurns(int remainingTurns)
        {
            RemainingTurns = remainingTurns < 0 ? 0 : remainingTurns;
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
