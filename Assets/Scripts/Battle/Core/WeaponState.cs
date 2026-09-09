using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class WeaponState
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int BasePower { get; }
        public ElementType? ElementAffinity { get; }
        public int MaxEnchantSlots { get; }
        public List<EnchantInstance> Enchantments { get; }

        public WeaponState(
            string id,
            string displayName,
            int basePower,
            ElementType? elementAffinity,
            int maxEnchantSlots)
        {
            Id = id;
            DisplayName = displayName;
            BasePower = basePower;
            ElementAffinity = elementAffinity;
            MaxEnchantSlots = maxEnchantSlots;
            Enchantments = new List<EnchantInstance>();
        }

        public void AddEnchant(TrigramId source, int duration)
        {
            while (Enchantments.Count >= MaxEnchantSlots)
            {
                Enchantments.RemoveAt(0);
            }

            Enchantments.Add(new EnchantInstance(Enchantments.Count + 1, source, duration));
        }

        public void TickTurn()
        {
            for (int i = Enchantments.Count - 1; i >= 0; i--)
            {
                Enchantments[i].TickTurn();
                if (Enchantments[i].IsExpired())
                {
                    Enchantments.RemoveAt(i);
                }
            }
        }
    }
}