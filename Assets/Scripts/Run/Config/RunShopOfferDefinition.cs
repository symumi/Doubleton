namespace BalartroLike.Run
{
    public sealed class RunShopOfferDefinition
    {
        public string Id { get; }
        public string ShopId { get; }
        public string DisplayName { get; }
        public RunEffectType EffectType { get; }
        public int EffectValue { get; }
        public int Price { get; }
        public string ContentId { get; }
        public string Description { get; }

        public RunShopOfferDefinition(
            string id,
            string shopId,
            string displayName,
            RunEffectType effectType,
            int effectValue,
            int price,
            string contentId,
            string description)
        {
            Id = id;
            ShopId = shopId;
            DisplayName = displayName;
            EffectType = effectType;
            EffectValue = effectValue;
            Price = price;
            ContentId = contentId;
            Description = description;
        }
    }
}