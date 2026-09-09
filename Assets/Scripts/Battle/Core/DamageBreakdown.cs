namespace BalartroLike.Battle
{
    public sealed class DamageBreakdown
    {
        public int BaseDamage { get; }
        public int WeaponAffinityMultiplier { get; }
        public int HexagramMultiplier { get; }
        public int MasteryMultiplier { get; }
        public int ElementMultiplier { get; }
        public int ArtifactAdditive { get; }
        public int ArtifactMultiplicative { get; }
        public int StatusMultiplier { get; }
        public int CriticalMultiplier { get; }
        public int RawDamage { get; }
        public int ShieldAbsorbed { get; }
        public int FinalDamage { get; }

        public DamageBreakdown(
            int baseDamage,
            int weaponAffinityMultiplier,
            int hexagramMultiplier,
            int masteryMultiplier,
            int elementMultiplier,
            int artifactAdditive,
            int artifactMultiplicative,
            int statusMultiplier,
            int criticalMultiplier,
            int rawDamage,
            int shieldAbsorbed,
            int finalDamage)
        {
            BaseDamage = baseDamage;
            WeaponAffinityMultiplier = weaponAffinityMultiplier;
            HexagramMultiplier = hexagramMultiplier;
            MasteryMultiplier = masteryMultiplier;
            ElementMultiplier = elementMultiplier;
            ArtifactAdditive = artifactAdditive;
            ArtifactMultiplicative = artifactMultiplicative;
            StatusMultiplier = statusMultiplier;
            CriticalMultiplier = criticalMultiplier;
            RawDamage = rawDamage;
            ShieldAbsorbed = shieldAbsorbed;
            FinalDamage = finalDamage;
        }

        public string ToFormula()
        {
            return BaseDamage
                + " × " + Format(WeaponAffinityMultiplier)
                + " × " + Format(HexagramMultiplier)
                + " × " + Format(MasteryMultiplier)
                + " × " + Format(ElementMultiplier)
                + " × " + Format(ArtifactAdditive)
                + " × " + Format(ArtifactMultiplicative)
                + " × " + Format(StatusMultiplier)
                + " × " + Format(CriticalMultiplier)
                + " = " + RawDamage
                + "（护盾吸收 " + ShieldAbsorbed + "，最终 " + FinalDamage + "）";
        }

        private static string Format(int basisPoints)
        {
            return (basisPoints / 10000f).ToString("0.00");
        }
    }
}