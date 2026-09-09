using System;
using System.Collections.Generic;

namespace BalartroLike.Run
{
    public static class RunConfigDatabase
    {
        private static readonly List<RunNodeDefinition> NodeDefinitions = new List<RunNodeDefinition>();
        private static readonly List<RunRealmDefinition> RealmDefinitions = new List<RunRealmDefinition>();
        private static readonly List<HeavenTribulationDefinition> TribulationDefinitions = new List<HeavenTribulationDefinition>();
        private static readonly HeavenTribulationDefinition BaseTribulation = new HeavenTribulationDefinition(0, "天劫 0", 100, 0, 100, "无额外词条");

        public static bool IsLoaded
        {
            get { return NodeDefinitions.Count > 0 && RealmDefinitions.Count > 0 && TribulationDefinitions.Count > 0; }
        }

        public static IReadOnlyList<RunNodeDefinition> Nodes
        {
            get { return NodeDefinitions; }
        }

        public static IReadOnlyList<RunRealmDefinition> Realms
        {
            get { return RealmDefinitions; }
        }

        public static IReadOnlyList<HeavenTribulationDefinition> Tribulations
        {
            get { return TribulationDefinitions; }
        }

        public static int MaxTribulationLevel
        {
            get { return TribulationDefinitions.Count > 0 ? TribulationDefinitions[TribulationDefinitions.Count - 1].Level : BaseTribulation.Level; }
        }

        public static bool TryGetRealm(string realmId, out RunRealmDefinition realm)
        {
            for (int i = 0; i < RealmDefinitions.Count; i++)
            {
                if (RealmDefinitions[i].Id == realmId)
                {
                    realm = RealmDefinitions[i];
                    return true;
                }
            }

            realm = null;
            return false;
        }

        public static bool TryGetTribulation(int level, out HeavenTribulationDefinition tribulation)
        {
            for (int i = 0; i < TribulationDefinitions.Count; i++)
            {
                if (TribulationDefinitions[i].Level == level)
                {
                    tribulation = TribulationDefinitions[i];
                    return true;
                }
            }

            tribulation = null;
            return false;
        }

        public static HeavenTribulationDefinition GetTribulation(int level)
        {
            return TryGetTribulation(level, out HeavenTribulationDefinition tribulation) ? tribulation : BaseTribulation;
        }

        public static void Load(
            IEnumerable<RunNodeDefinition> nodes,
            IEnumerable<RunRealmDefinition> realms,
            IEnumerable<HeavenTribulationDefinition> tribulations = null)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (realms == null)
            {
                throw new ArgumentNullException(nameof(realms));
            }

            NodeDefinitions.Clear();
            RealmDefinitions.Clear();
            TribulationDefinitions.Clear();
            HashSet<string> realmIds = new HashSet<string>();
            foreach (RunRealmDefinition realm in realms)
            {
                if (!realmIds.Add(realm.Id))
                {
                    throw new InvalidOperationException("Duplicate run realm id: " + realm.Id);
                }

                RealmDefinitions.Add(realm);
            }

            if (RealmDefinitions.Count == 0)
            {
                throw new InvalidOperationException("Run realm config database is empty.");
            }

            HashSet<string> nodeIds = new HashSet<string>();
            foreach (RunNodeDefinition node in nodes)
            {
                if (!nodeIds.Add(node.Id))
                {
                    throw new InvalidOperationException("Duplicate run node id: " + node.Id);
                }

                if (!realmIds.Contains(node.RealmId))
                {
                    throw new InvalidOperationException("Unknown run realm id: " + node.RealmId);
                }

                NodeDefinitions.Add(node);
            }

            if (NodeDefinitions.Count == 0)
            {
                throw new InvalidOperationException("Run node config database is empty.");
            }

            HashSet<int> tribulationLevels = new HashSet<int>();
            IEnumerable<HeavenTribulationDefinition> tribulationSource = tribulations ?? CreateDefaultTribulations();
            foreach (HeavenTribulationDefinition tribulation in tribulationSource)
            {
                if (tribulation == null)
                {
                    throw new InvalidOperationException("Run tribulation config contains a null row.");
                }

                if (!tribulationLevels.Add(tribulation.Level))
                {
                    throw new InvalidOperationException("Duplicate run tribulation level: " + tribulation.Level);
                }

                if (tribulation.Level < 0 || tribulation.EnemyHpPercent <= 0 || tribulation.ShopPricePercent <= 0)
                {
                    throw new InvalidOperationException("Invalid run tribulation config: " + tribulation.Level);
                }

                TribulationDefinitions.Add(tribulation);
            }

            TribulationDefinitions.Sort((left, right) => left.Level.CompareTo(right.Level));
            if (TribulationDefinitions.Count == 0 || TribulationDefinitions[0].Level != 0)
            {
                throw new InvalidOperationException("Run tribulation config must start at level 0.");
            }

            for (int i = 0; i < RealmDefinitions.Count; i++)
            {
                string targetId = RealmDefinitions[i].PromotionTargetRealmId;
                if (!string.IsNullOrEmpty(targetId) && !realmIds.Contains(targetId))
                {
                    throw new InvalidOperationException("Unknown promotion target realm id: " + targetId);
                }
            }
        }

        private static IEnumerable<HeavenTribulationDefinition> CreateDefaultTribulations()
        {
            return new[] { BaseTribulation };
        }
    }
}