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
}
