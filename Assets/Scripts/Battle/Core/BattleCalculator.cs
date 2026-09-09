using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleCalculator
    {
        private const int BasisPoints = 10000;
        private const int CriticalMultiplier = 15000;

        public BattleCalculation CalculatePlay(BattleState state, CardInstance innerCard, CardInstance outerCard)
        {
            TrigramDefinition inner = TrigramCatalog.Get(innerCard.Trigram);
            TrigramDefinition outer = TrigramCatalog.Get(outerCard.Trigram);
            HexagramDefinition hexagram = HexagramCatalog.Get(outerCard.Trigram, innerCard.Trigram);

            int hexagramMultiplier = hexagram.DamageMultiplierOverride
                ?? Multiply(inner.InnerDamageMultiplier, outer.OuterDamageMultiplier);
            int elementMultiplier = ElementRules.GetDamageMultiplier(inner.Element, state.Enemy.Element);
            // TODO: 法宝系统接入后从 BattleState 汇总加法乘区和乘法乘区。
            int artifactAdditive = BasisPoints;
            int artifactMultiplicative = BasisPoints;
            int statusMultiplier = GetStatusMultiplier(state);
            bool guaranteedCritical = hexagram.GuaranteedCriticalOverride ?? outer.OuterGuaranteedCritical;
            int criticalMultiplier = guaranteedCritical ? CriticalMultiplier : BasisPoints;

            int baseDamage = state.Player.Weapon.BasePower + innerCard.Qi + outerCard.Qi;
            int rawDamage = ApplyMultiplier(baseDamage, hexagramMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, elementMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, artifactAdditive);
            rawDamage = ApplyMultiplier(rawDamage, artifactMultiplicative);
            rawDamage = ApplyMultiplier(rawDamage, statusMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, criticalMultiplier);

            int shieldAbsorbed = state.Enemy.Shield >= rawDamage ? rawDamage : state.Enemy.Shield;
            int finalDamage = rawDamage - shieldAbsorbed;
            DamageBreakdown damage = new DamageBreakdown(
                baseDamage,
                hexagramMultiplier,
                elementMultiplier,
                artifactAdditive,
                artifactMultiplicative,
                statusMultiplier,
                criticalMultiplier,
                rawDamage,
                shieldAbsorbed,
                finalDamage);

            List<EffectOperation> effects = CollectEffects(inner, outer, hexagram);
            return new BattleCalculation(innerCard.Uid, outerCard.Uid, hexagram, damage, inner.InnerHitCount, effects);
        }

        private static int GetStatusMultiplier(BattleState state)
        {
            int multiplier = BasisPoints;
            StatusInstance vulnerable = state.Enemy.GetStatus(StatusId.Vulnerable);
            if (vulnerable != null)
            {
                multiplier += vulnerable.Stacks * 2500;
            }

            StatusInstance weak = state.Player.GetStatus(StatusId.Weak);
            if (weak != null)
            {
                multiplier -= weak.Stacks * 2500;
            }

            return Math.Max(multiplier, 1000);
        }

        private static List<EffectOperation> CollectEffects(
            TrigramDefinition inner,
            TrigramDefinition outer,
            HexagramDefinition hexagram)
        {
            List<EffectOperation> effects = new List<EffectOperation>();
            effects.AddRange(inner.InnerEffects);
            effects.AddRange(outer.OuterEffects);
            effects.AddRange(hexagram.AdditionalEffects);
            return effects;
        }

        private static int Multiply(int left, int right)
        {
            return (int)(((long)left * right + BasisPoints / 2) / BasisPoints);
        }

        private static int ApplyMultiplier(int value, int multiplier)
        {
            return (int)(((long)value * multiplier + BasisPoints / 2) / BasisPoints);
        }
    }
}