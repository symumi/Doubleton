namespace BalartroLike.Battle
{
    public sealed class WeaponDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int BasePower { get; }
        public AttackPattern AttackPattern { get; }
        public int HitCount { get; }
        public ElementType? ElementAffinity { get; }
        public int MaxEnchantSlots { get; }

        public WeaponDefinition(
            string id,
            string displayName,
            int basePower,
            AttackPattern attackPattern,
            int hitCount,
            ElementType? elementAffinity,
            int maxEnchantSlots)
        {
            Id = id;
            DisplayName = displayName;
            BasePower = basePower;
            AttackPattern = attackPattern;
            HitCount = hitCount;
            ElementAffinity = elementAffinity;
            MaxEnchantSlots = maxEnchantSlots;
        }
    }
}