using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleCalculation
    {
        public int InnerCardUid { get; }
        public int OuterCardUid { get; }
        public HexagramDefinition Hexagram { get; }
        public DamageBreakdown Damage { get; }
        public AttackPattern AttackPattern { get; }
        public int HitCount { get; }
        public List<EffectOperation> Effects { get; }

        public bool CanDefeat(EnemyState enemy)
        {
            return Damage.RawDamage * HitCount >= enemy.Hp + enemy.Shield;
        }

        public BattleCalculation(
            int innerCardUid,
            int outerCardUid,
            HexagramDefinition hexagram,
            DamageBreakdown damage,
            AttackPattern attackPattern,
            int hitCount,
            List<EffectOperation> effects)
        {
            InnerCardUid = innerCardUid;
            OuterCardUid = outerCardUid;
            Hexagram = hexagram;
            Damage = damage;
            AttackPattern = attackPattern;
            HitCount = hitCount;
            Effects = effects;
        }
    }
}