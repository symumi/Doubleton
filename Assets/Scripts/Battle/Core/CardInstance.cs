namespace BalartroLike.Battle
{
    public sealed class CardInstance
    {
        public int Uid { get; }
        public TrigramId Trigram { get; }
        public int Rank { get; }
        public int Qi { get; private set; }

        public CardInstance(int uid, TrigramId trigram, int rank, int qi)
        {
            Uid = uid;
            Trigram = trigram;
            Rank = rank;
            Qi = qi;
        }

        public void ModifyQi(int delta)
        {
            Qi += delta;
        }
    }
}