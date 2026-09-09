namespace BalartroLike.Battle
{
    public sealed class DeckEntryDefinition
    {
        public TrigramId Trigram { get; }
        public int Rank { get; }
        public int Qi { get; }
        public int Count { get; }

        public DeckEntryDefinition(TrigramId trigram, int rank, int qi, int count)
        {
            Trigram = trigram;
            Rank = rank;
            Qi = qi;
            Count = count;
        }
    }
}