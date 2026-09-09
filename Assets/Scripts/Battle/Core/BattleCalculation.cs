using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleCalculation
    {
        public int InnerCardUid { get; }
        public int OuterCardUid { get; }
        public HexagramDefinition Hexagram { get; }
        public DamageBreakdown Damage { get; }
        public int HitCount { get; }
        public List<EffectOperation> Effects { get; }

        public BattleCalculation(
            int innerCardUid,
            int outerCardUid,
            HexagramDefinition hexagram,
            DamageBreakdown damage,
            int hitCount,
            List<EffectOperation> effects)
        {
            InnerCardUid = innerCardUid;
            OuterCardUid = outerCardUid;
            Hexagram = hexagram;
            Damage = damage;
            HitCount = hitCount;
            Effects = effects;
        }
    }
}