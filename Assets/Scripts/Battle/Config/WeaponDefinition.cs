namespace BalartroLike.Battle
{
    public sealed class WeaponDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int BasePower { get; }
        public ElementType? ElementAffinity { get; }
        public int MaxEnchantSlots { get; }

        public WeaponDefinition(string id, string displayName, int basePower, ElementType? elementAffinity, int maxEnchantSlots)
        {
            Id = id;
            DisplayName = displayName;
            BasePower = basePower;
            ElementAffinity = elementAffinity;
            MaxEnchantSlots = maxEnchantSlots;
        }
    }
}