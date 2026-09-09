using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class WeaponState
    {
        public string Id { get; }
        public string DisplayName { get; }
        public int BasePower { get; }
        public AttackPattern AttackPattern { get; }
        public int HitCount { get; }
        public ElementType? ElementAffinity { get; }
        public int MaxEnchantSlots { get; }
        public List<EnchantInstance> Enchantments { get; }
        private int _nextOrder = 1;

        public int NextOrder
        {
            get { return _nextOrder; }
        }

        public WeaponState(
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
            Enchantments = new List<EnchantInstance>();
        }

        public void Restore(int nextOrder, IReadOnlyList<EnchantInstance> enchantments)
        {
            Enchantments.Clear();
            if (enchantments != null)
            {
                Enchantments.AddRange(enchantments);
            }

            _nextOrder = nextOrder > 0 ? nextOrder : 1;
            for (int i = 0; i < Enchantments.Count; i++)
            {
                if (Enchantments[i].Order >= _nextOrder)
                {
                    _nextOrder = Enchantments[i].Order + 1;
                }
            }
        }

        public void AddEnchant(TrigramId source, EffectOperation effect)
        {
            if (effect.Duration <= 0)
            {
                return;
            }

            while (Enchantments.Count >= MaxEnchantSlots)
            {
                Enchantments.RemoveAt(0);
            }

            Enchantments.Add(new EnchantInstance(_nextOrder, source, effect));
            _nextOrder++;
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
