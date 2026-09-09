using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleState
    {
        public int Seed { get; }
        public int Turn { get; set; }
        public int DiscardsRemaining { get; set; }
        public int DiscardLimit { get; set; }
        public BattlePhase Phase { get; set; }
        public BattleResultType Result { get; set; }
        public PlayerState Player { get; }
        public EnemyState Enemy { get; }
        public List<CardInstance> DrawPile { get; }
        public List<CardInstance> DiscardPile { get; }
        public List<CardInstance> Hand { get; }
        public List<BattleEvent> Events { get; }

        public BattleState(int seed, PlayerState player, EnemyState enemy)
        {
            Seed = seed;
            Player = player;
            Enemy = enemy;
            DrawPile = new List<CardInstance>();
            DiscardPile = new List<CardInstance>();
            Hand = new List<CardInstance>();
            Events = new List<BattleEvent>();
            DiscardsRemaining = 3;
            DiscardLimit = 3;
            Phase = BattlePhase.BattleStart;
            Result = BattleResultType.None;
        }

        public CardInstance FindHandCard(int uid)
        {
            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Uid == uid)
                {
                    return Hand[i];
                }
            }

            return null;
        }

        public bool RemoveHandCard(int uid)
        {
            for (int i = 0; i < Hand.Count; i++)
            {
                if (Hand[i].Uid == uid)
                {
                    Hand.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public void AddEvent(BattleEvent battleEvent)
        {
            Events.Add(battleEvent);
        }
    }
}