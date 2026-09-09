namespace BalartroLike.Run
{
    public sealed class RunNodeDefinition
    {
        public string Id { get; }
        public string RealmId { get; }
        public string DisplayName { get; }
        public RunNodeType Type { get; }
        public string EnemyId { get; }
        public int RewardSpiritStones { get; }
        public RunEncounterType EncounterType { get; }
        public string EncounterId { get; }

        public RunNodeDefinition(
            string id,
            string realmId,
            string displayName,
            RunNodeType type,
            string enemyId,
            int rewardSpiritStones,
            RunEncounterType encounterType,
            string encounterId)
        {
            Id = id;
            RealmId = realmId;
            DisplayName = displayName;
            Type = type;
            EnemyId = enemyId;
            RewardSpiritStones = rewardSpiritStones;
            EncounterType = encounterType;
            EncounterId = encounterId;
        }
    }
}
