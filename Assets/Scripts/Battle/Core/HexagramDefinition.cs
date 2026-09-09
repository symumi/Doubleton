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

        public HexagramDefinition(
            string id,
            string displayName,
            TrigramId outer,
            TrigramId inner,
            int? damageMultiplierOverride = null,
            bool? guaranteedCriticalOverride = null,
            EffectOperation[] additionalEffects = null)
        {
            Id = id;
            DisplayName = displayName;
            Outer = outer;
            Inner = inner;
            DamageMultiplierOverride = damageMultiplierOverride;
            GuaranteedCriticalOverride = guaranteedCriticalOverride;
            AdditionalEffects = additionalEffects ?? new EffectOperation[0];
        }
    }
}