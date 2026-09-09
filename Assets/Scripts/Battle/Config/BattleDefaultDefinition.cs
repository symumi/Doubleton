namespace BalartroLike.Battle
{
    public sealed class BattleDefaultDefinition
    {
        public int PlayerMaxHp { get; }
        public int PlayerMaxEnergy { get; }
        public int EnergyPerTurn { get; }
        public int DiscardLimit { get; }
        public int HandSize { get; }
        public string DefaultWeaponId { get; }
        public string DefaultEnemyId { get; }
        public int DefaultSeed { get; }
        public string[] DefaultArtifactIds { get; }
        public string[] DefaultTalismanIds { get; }

        public BattleDefaultDefinition(
            int playerMaxHp,
            int playerMaxEnergy,
            int energyPerTurn,
            int discardLimit,
            int handSize,
            string defaultWeaponId,
            string defaultEnemyId,
            int defaultSeed,
            string[] defaultArtifactIds,
            string[] defaultTalismanIds)
        {
            PlayerMaxHp = playerMaxHp;
            PlayerMaxEnergy = playerMaxEnergy;
            EnergyPerTurn = energyPerTurn;
            DiscardLimit = discardLimit;
            HandSize = handSize;
            DefaultWeaponId = defaultWeaponId;
            DefaultEnemyId = defaultEnemyId;
            DefaultSeed = defaultSeed;
            DefaultArtifactIds = defaultArtifactIds;
            DefaultTalismanIds = defaultTalismanIds;
        }
    }
}
