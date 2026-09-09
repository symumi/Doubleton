namespace BalartroLike.Battle
{
    public sealed class HexagramDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public TrigramId Outer { get; }
        public TrigramId Inner { get; }
        public int? DamageMultiplierOverride { get; }
        public bool? GuaranteedCriticalOverride { get; }
        public EffectOperation[] AdditionalEffects { get; }
        public string Description { get; }

        public bool IsSpecial
        {
            get
            {
                return DamageMultiplierOverride.HasValue
                    || GuaranteedCriticalOverride.HasValue
                    || AdditionalEffects.Length > 0;
            }
        }

        public HexagramDefinition(
            string id,
            string displayName,
            TrigramId outer,
            TrigramId inner,
            int? damageMultiplierOverride = null,
            bool? guaranteedCriticalOverride = null,
            EffectOperation[] additionalEffects = null,
            string description = null)
        {
            Id = id;
            DisplayName = displayName;
            Outer = outer;
            Inner = inner;
            DamageMultiplierOverride = damageMultiplierOverride;
            GuaranteedCriticalOverride = guaranteedCriticalOverride;
            AdditionalEffects = additionalEffects ?? new EffectOperation[0];
            Description = description ?? string.Empty;
        }
    }

    public static class HexagramMastery
    {
        public const int MasteredLevel = 5;
        public const int MasteryDamageMultiplier = 12000;

        private static readonly int[] LevelThresholds = { 0, 1, 3, 6, 10, 15 };

        public static int GetLevel(int useCount)
        {
            int level = 0;
            for (int i = 1; i < LevelThresholds.Length; i++)
            {
                if (useCount < LevelThresholds[i])
                {
                    break;
                }

                level = i;
            }

            return level;
        }

        public static bool IsMastered(int useCount)
        {
            return GetLevel(useCount) >= MasteredLevel;
        }

        public static int GetDamageMultiplier(int useCount)
        {
            return IsMastered(useCount) ? MasteryDamageMultiplier : 10000;
        }
    }
}