namespace BalartroLike.Battle
{
    public sealed class StatusInstance
    {
        public StatusId Id { get; }
        public int Stacks { get; private set; }
        public int RemainingTurns { get; private set; }

        public StatusInstance(StatusId id, int stacks, int remainingTurns)
        {
            Id = id;
            Stacks = stacks;
            RemainingTurns = remainingTurns;
        }

        public void Add(int stacks, int duration)
        {
            Stacks += stacks;
            if (duration > RemainingTurns)
            {
                RemainingTurns = duration;
            }
        }

        public void ConsumeStacks(int amount)
        {
            Stacks -= amount;
            if (Stacks < 0)
            {
                Stacks = 0;
            }
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
            return RemainingTurns <= 0;
        }
    }
}