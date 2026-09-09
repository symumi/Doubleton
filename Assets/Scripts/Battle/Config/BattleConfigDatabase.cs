using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public static class BattleConfigDatabase
    {
        private static readonly Dictionary<string, WeaponDefinition> Weapons = new Dictionary<string, WeaponDefinition>();
        private static readonly Dictionary<string, EnemyDefinition> Enemies = new Dictionary<string, EnemyDefinition>();
        private static readonly List<DeckEntryDefinition> DeckEntries = new List<DeckEntryDefinition>();

        public static bool IsLoaded { get; private set; }
        public static BattleDefaultDefinition Default { get; private set; }
        public static IReadOnlyList<DeckEntryDefinition> Deck { get { return DeckEntries; } }

        public static void Load(
            BattleDefaultDefinition defaultDefinition,
            IEnumerable<WeaponDefinition> weapons,
            IEnumerable<EnemyDefinition> enemies,
            IEnumerable<DeckEntryDefinition> deckEntries)
        {
            if (defaultDefinition == null)
            {
                throw new ArgumentNullException(nameof(defaultDefinition));
            }

            Weapons.Clear();
            Enemies.Clear();
            DeckEntries.Clear();

            foreach (WeaponDefinition weapon in weapons)
            {
                Weapons.Add(weapon.Id, weapon);
            }

            foreach (EnemyDefinition enemy in enemies)
            {
                Enemies.Add(enemy.Id, enemy);
            }

            foreach (DeckEntryDefinition entry in deckEntries)
            {
                DeckEntries.Add(entry);
            }

            if (Weapons.Count == 0 || Enemies.Count == 0 || DeckEntries.Count == 0)
            {
                throw new InvalidOperationException("Battle config database is incomplete.");
            }

            Default = defaultDefinition;
            IsLoaded = true;
        }

        public static WeaponDefinition GetWeapon(string id)
        {
            if (Weapons.TryGetValue(id, out WeaponDefinition weapon))
            {
                return weapon;
            }

            throw new ArgumentException("Unknown weapon: " + id);
        }

        public static EnemyDefinition GetEnemy(string id)
        {
            if (Enemies.TryGetValue(id, out EnemyDefinition enemy))
            {
                return enemy;
            }

            throw new ArgumentException("Unknown enemy: " + id);
        }
    }
}