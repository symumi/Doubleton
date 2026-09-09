using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public static class BattleConfigDatabase
    {
        private static readonly Dictionary<string, WeaponDefinition> WeaponLookup = new Dictionary<string, WeaponDefinition>();
        private static readonly List<WeaponDefinition> WeaponDefinitions = new List<WeaponDefinition>();
        private static readonly Dictionary<string, EnemyDefinition> EnemyLookup = new Dictionary<string, EnemyDefinition>();
        private static readonly List<EnemyDefinition> EnemyDefinitions = new List<EnemyDefinition>();
        private static readonly List<DeckEntryDefinition> DeckEntries = new List<DeckEntryDefinition>();
        private static readonly Dictionary<string, ArtifactDefinition> ArtifactLookup = new Dictionary<string, ArtifactDefinition>();
        private static readonly Dictionary<string, TalismanDefinition> TalismanLookup = new Dictionary<string, TalismanDefinition>();
        private static readonly List<ArtifactDefinition> ArtifactDefinitions = new List<ArtifactDefinition>();
        private static readonly List<TalismanDefinition> TalismanDefinitions = new List<TalismanDefinition>();

        public static bool IsLoaded { get; private set; }
        public static BattleDefaultDefinition Default { get; private set; }
        public static IReadOnlyList<DeckEntryDefinition> Deck { get { return DeckEntries; } }
        public static IReadOnlyList<WeaponDefinition> Weapons { get { return WeaponDefinitions; } }
        public static IReadOnlyList<EnemyDefinition> Enemies { get { return EnemyDefinitions; } }
        public static IReadOnlyList<ArtifactDefinition> Artifacts { get { return ArtifactDefinitions; } }
        public static IReadOnlyList<TalismanDefinition> Talismans { get { return TalismanDefinitions; } }

        public static void Load(
            BattleDefaultDefinition defaultDefinition,
            IEnumerable<WeaponDefinition> weapons,
            IEnumerable<EnemyDefinition> enemies,
            IEnumerable<DeckEntryDefinition> deckEntries,
            IEnumerable<ArtifactDefinition> artifacts,
            IEnumerable<TalismanDefinition> talismans)
        {
            if (defaultDefinition == null)
            {
                throw new ArgumentNullException(nameof(defaultDefinition));
            }

            WeaponLookup.Clear();
            WeaponDefinitions.Clear();
            EnemyLookup.Clear();
            EnemyDefinitions.Clear();
            DeckEntries.Clear();
            ArtifactLookup.Clear();
            TalismanLookup.Clear();
            ArtifactDefinitions.Clear();
            TalismanDefinitions.Clear();

            foreach (WeaponDefinition weapon in weapons)
            {
                WeaponLookup.Add(weapon.Id, weapon);
                WeaponDefinitions.Add(weapon);
            }

            foreach (EnemyDefinition enemy in enemies)
            {
                EnemyLookup.Add(enemy.Id, enemy);
                EnemyDefinitions.Add(enemy);
            }

            foreach (DeckEntryDefinition entry in deckEntries)
            {
                DeckEntries.Add(entry);
            }

            foreach (ArtifactDefinition artifact in artifacts)
            {
                ArtifactLookup.Add(artifact.Id, artifact);
                ArtifactDefinitions.Add(artifact);
            }

            foreach (TalismanDefinition talisman in talismans)
            {
                TalismanLookup.Add(talisman.Id, talisman);
                TalismanDefinitions.Add(talisman);
            }

            if (WeaponLookup.Count == 0 || EnemyLookup.Count == 0 || DeckEntries.Count == 0
                || ArtifactLookup.Count == 0 || TalismanLookup.Count == 0)
            {
                throw new InvalidOperationException("Battle config database is incomplete.");
            }

            Default = defaultDefinition;
            IsLoaded = true;
        }

        public static WeaponDefinition GetWeapon(string id)
        {
            if (TryGetWeapon(id, out WeaponDefinition weapon))
            {
                return weapon;
            }

            throw new ArgumentException("Unknown weapon: " + id);
        }

        public static bool TryGetWeapon(string id, out WeaponDefinition weapon)
        {
            return WeaponLookup.TryGetValue(id, out weapon);
        }

        public static bool TryGetEnemy(string id, out EnemyDefinition enemy)
        {
            return EnemyLookup.TryGetValue(id, out enemy);
        }
        public static EnemyDefinition GetEnemy(string id)
        {
            if (EnemyLookup.TryGetValue(id, out EnemyDefinition enemy))
            {
                return enemy;
            }

            throw new ArgumentException("Unknown enemy: " + id);
        }

        public static ArtifactDefinition GetArtifact(string id)
        {
            if (ArtifactLookup.TryGetValue(id, out ArtifactDefinition artifact))
            {
                return artifact;
            }

            throw new ArgumentException("Unknown artifact: " + id);
        }

        public static bool TryGetArtifact(string id, out ArtifactDefinition artifact)
        {
            return ArtifactLookup.TryGetValue(id, out artifact);
        }

        public static TalismanDefinition GetTalisman(string id)
        {
            if (TalismanLookup.TryGetValue(id, out TalismanDefinition talisman))
            {
                return talisman;
            }

            throw new ArgumentException("Unknown talisman: " + id);
        }

        public static bool TryGetTalisman(string id, out TalismanDefinition talisman)
        {
            return TalismanLookup.TryGetValue(id, out talisman);
        }
    }
}
