namespace BalartroLike.Battle
{
    public sealed class TrigramDefinition
    {
        public TrigramId Id { get; }
        public string DisplayName { get; }
        public string Symbol { get; }
        public ElementType Element { get; }
        public YinYangType YinYang { get; }
        public int BaseQi { get; }
        public int InnerDamageMultiplier { get; }
        public int InnerHitCount { get; }
        public EffectOperation[] InnerEffects { get; }
        public int OuterDamageMultiplier { get; }
        public bool OuterGuaranteedCritical { get; }
        public EffectOperation[] OuterEffects { get; }

        public TrigramDefinition(
            TrigramId id,
            string displayName,
            string symbol,
            ElementType element,
            YinYangType yinYang,
            int baseQi,
            int innerDamageMultiplier,
            int innerHitCount,
            EffectOperation[] innerEffects,
            int outerDamageMultiplier,
            bool outerGuaranteedCritical,
            EffectOperation[] outerEffects)
        {
            Id = id;
            DisplayName = displayName;
            Symbol = symbol;
            Element = element;
            YinYang = yinYang;
            BaseQi = baseQi;
            InnerDamageMultiplier = innerDamageMultiplier;
            InnerHitCount = innerHitCount;
            InnerEffects = innerEffects;
            OuterDamageMultiplier = outerDamageMultiplier;
            OuterGuaranteedCritical = outerGuaranteedCritical;
            OuterEffects = outerEffects;
        }
    }
}