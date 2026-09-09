namespace BalartroLike.Battle
{
    public static class ElementRules
    {
        public const int NormalMultiplier = 10000;
        public const int SameElementEnemyMultiplier = 8500;
        public const int RestrainsMultiplier = 15000;
        public const int RestrainedMultiplier = 6700;

        public static int GetDamageMultiplier(ElementType attack, ElementType defense)
        {
            if (attack == defense)
            {
                return SameElementEnemyMultiplier;
            }

            if (Restrains(attack, defense))
            {
                return RestrainsMultiplier;
            }

            if (Restrains(defense, attack))
            {
                return RestrainedMultiplier;
            }

            return NormalMultiplier;
        }

        private static bool Restrains(ElementType attacker, ElementType defender)
        {
            return (attacker == ElementType.Metal && defender == ElementType.Wood)
                || (attacker == ElementType.Wood && defender == ElementType.Earth)
                || (attacker == ElementType.Earth && defender == ElementType.Water)
                || (attacker == ElementType.Water && defender == ElementType.Fire)
                || (attacker == ElementType.Fire && defender == ElementType.Metal);
        }
    }
}