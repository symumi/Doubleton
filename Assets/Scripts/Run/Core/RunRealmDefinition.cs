namespace BalartroLike.Run
{
    public sealed class RunRealmDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string PromotionTargetRealmId { get; }
        public int PromotionRewardSpiritStones { get; }

        public RunRealmDefinition(string id, string displayName, string promotionTargetRealmId, int promotionRewardSpiritStones)
        {
            Id = id;
            DisplayName = displayName;
            PromotionTargetRealmId = promotionTargetRealmId;
            PromotionRewardSpiritStones = promotionRewardSpiritStones;
        }
    }
    public sealed class HeavenTribulationDefinition
    {
        public int Level { get; }
        public string DisplayName { get; }
        public int EnemyHpPercent { get; }
        public int EnemyPowerBonus { get; }
        public int ShopPricePercent { get; }
        public string Description { get; }

        public HeavenTribulationDefinition(
            int level,
            string displayName,
            int enemyHpPercent,
            int enemyPowerBonus,
            int shopPricePercent,
            string description)
        {
            Level = level;
            DisplayName = displayName;
            EnemyHpPercent = enemyHpPercent;
            EnemyPowerBonus = enemyPowerBonus;
            ShopPricePercent = shopPricePercent;
            Description = description;
        }
    }
}