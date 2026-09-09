using System;
using System.Collections.Generic;

namespace BalartroLike.Run
{
    public sealed class RunNodeDefinition
    {
        public string Id { get; }
        public string RealmId { get; }
        public string DisplayName { get; }
        public RunNodeType Type { get; }
        public IReadOnlyList<string> EnemyIds { get; }
        public string EnemyId
        {
            get { return EnemyIds.Count > 0 ? EnemyIds[0] : string.Empty; }
        }

        public int RewardSpiritStones { get; }
        public RunEncounterType EncounterType { get; }
        public string EncounterId { get; }

        public RunNodeDefinition(
            string id,
            string realmId,
            string displayName,
            RunNodeType type,
            IReadOnlyList<string> enemyIds,
            int rewardSpiritStones,
            RunEncounterType encounterType,
            string encounterId)
        {
            Id = id;
            RealmId = realmId;
            DisplayName = displayName;
            Type = type;
            EnemyIds = enemyIds ?? new string[0];
            RewardSpiritStones = rewardSpiritStones;
            EncounterType = encounterType;
            EncounterId = encounterId;
        }

        public string SelectEnemyId(int seed)
        {
            if (EnemyIds.Count == 0)
            {
                return string.Empty;
            }

            int index = (int)(Math.Abs((long)seed) % EnemyIds.Count);
            return EnemyIds[index];
        }
    }
}