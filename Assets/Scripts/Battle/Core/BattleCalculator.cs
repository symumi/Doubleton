using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public sealed class BattleCalculator
    {
        private const int BasisPoints = 10000;
        private const int CriticalMultiplier = 15000;
        private const int WeaponAffinityMultiplier = 12500;

        public BattleCalculation CalculatePlay(BattleState state, CardInstance innerCard, CardInstance outerCard)
        {
            TrigramDefinition inner = TrigramCatalog.Get(innerCard.Trigram);
            TrigramDefinition outer = TrigramCatalog.Get(outerCard.Trigram);
            HexagramDefinition hexagram = HexagramCatalog.Get(outerCard.Trigram, innerCard.Trigram);

            int weaponAffinityMultiplier = GetWeaponAffinityMultiplier(state.Player.Weapon, inner);
            int hexagramMultiplier = hexagram.DamageMultiplierOverride
                ?? Multiply(inner.InnerDamageMultiplier, outer.OuterDamageMultiplier);
            int masteryMultiplier = HexagramMastery.GetDamageMultiplier(state.GetHexagramUseCount(hexagram.Id));
            int elementMultiplier = ElementRules.GetDamageMultiplier(inner.Element, state.Enemy.Element);
            ArtifactRules.CollectDamageModifiers(
                state,
                hexagram,
                out int artifactAdditive,
                out int artifactMultiplicative);
            int statusMultiplier = GetStatusMultiplier(state);
            bool guaranteedCritical = hexagram.GuaranteedCriticalOverride ?? outer.OuterGuaranteedCritical;
            int criticalMultiplier = guaranteedCritical ? CriticalMultiplier : BasisPoints;

            int baseDamage = state.Player.Weapon.BasePower + innerCard.Qi + outerCard.Qi;
            int rawDamage = ApplyMultiplier(baseDamage, weaponAffinityMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, hexagramMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, masteryMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, elementMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, artifactAdditive);
            rawDamage = ApplyMultiplier(rawDamage, artifactMultiplicative);
            rawDamage = ApplyMultiplier(rawDamage, statusMultiplier);
            rawDamage = ApplyMultiplier(rawDamage, criticalMultiplier);

            int shieldAbsorbed = state.Enemy.Shield >= rawDamage ? rawDamage : state.Enemy.Shield;
            int finalDamage = rawDamage - shieldAbsorbed;
            DamageBreakdown damage = new DamageBreakdown(
                baseDamage,
                weaponAffinityMultiplier,
                hexagramMultiplier,
                masteryMultiplier,
                elementMultiplier,
                artifactAdditive,
                artifactMultiplicative,
                statusMultiplier,
                criticalMultiplier,
                rawDamage,
                shieldAbsorbed,
                finalDamage);

            List<EffectOperation> effects = CollectEffects(inner, outer, hexagram);
            int hitCount = GetHitCount(state.Player.Weapon, inner);
            return new BattleCalculation(
                innerCard.Uid,
                outerCard.Uid,
                hexagram,
                damage,
                state.Player.Weapon.AttackPattern,
                hitCount,
                effects);
        }

        private static int GetWeaponAffinityMultiplier(WeaponState weapon, TrigramDefinition inner)
        {
            return weapon.ElementAffinity.HasValue && weapon.ElementAffinity.Value == inner.Element
                ? WeaponAffinityMultiplier
                : BasisPoints;
        }

        private static int GetHitCount(WeaponState weapon, TrigramDefinition inner)
        {
            return Math.Max(1, weapon.HitCount) * Math.Max(1, inner.InnerHitCount);
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