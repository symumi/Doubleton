namespace BalartroLike.Battle
{
    public sealed class BattleEvent
    {
        public BattleEventType Type { get; }
        public string Message { get; }
        public int Amount { get; }
        public CardInstance Card { get; }
        public HexagramDefinition Hexagram { get; }

        public BattleEvent(
            BattleEventType type,
            string message,
            int amount = 0,
            CardInstance card = null,
            HexagramDefinition hexagram = null)
        {
            Type = type;
            Message = message;
            Amount = amount;
            Card = card;
            Hexagram = hexagram;
        }

        public static BattleEvent CardDrawn(CardInstance card)
        {
            return new BattleEvent(BattleEventType.CardDrawn, "抽到" + TrigramCatalog.Get(card.Trigram).DisplayName, 0, card);
        }

        public static BattleEvent HexagramFormed(HexagramDefinition hexagram)
        {
            return new BattleEvent(BattleEventType.HexagramFormed, hexagram.DisplayName, 0, null, hexagram);
        }

        public static BattleEvent DamageDealt(int amount)
        {
            return new BattleEvent(BattleEventType.DamageDealt, "造成 " + amount + " 点伤害", amount);
        }

        public static BattleEvent EnergyChanged(int amount)
        {
            return new BattleEvent(BattleEventType.EnergyChanged, "灵力 " + amount, amount);
        }

        public static BattleEvent BattleEnded(BattleResultType result)
        {
            string message = result == BattleResultType.Victory ? "战斗胜利" : "战斗失败";
            return new BattleEvent(BattleEventType.BattleEnded, message, (int)result);
        }
    }
}