using BalartroLike.Battle;

namespace BalartroLike.Run
{
    public sealed class RunDeckCard
    {
        public TrigramId Trigram { get; }
        public int Rank { get; }
        public int Qi { get; }

        public RunDeckCard(TrigramId trigram, int rank, int qi)
        {
            Trigram = trigram;
            Rank = rank;
            Qi = qi;
        }
    }
}
