using System;
using System.Collections.Generic;

namespace BalartroLike.Run
{
    public static class RunConfigDatabase
    {
        private static readonly List<RunNodeDefinition> NodeDefinitions = new List<RunNodeDefinition>();
        private static readonly List<RunRealmDefinition> RealmDefinitions = new List<RunRealmDefinition>();

        public static bool IsLoaded
        {
            get { return NodeDefinitions.Count > 0 && RealmDefinitions.Count > 0; }
        }

        public static IReadOnlyList<RunNodeDefinition> Nodes
        {
            get { return NodeDefinitions; }
        }

        public static IReadOnlyList<RunRealmDefinition> Realms
        {
            get { return RealmDefinitions; }
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

        public static void Load(IEnumerable<RunNodeDefinition> nodes, IEnumerable<RunRealmDefinition> realms)
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

            for (int i = 0; i < RealmDefinitions.Count; i++)
            {
                string targetId = RealmDefinitions[i].PromotionTargetRealmId;
                if (!string.IsNullOrEmpty(targetId) && !realmIds.Contains(targetId))
                {
                    throw new InvalidOperationException("Unknown promotion target realm id: " + targetId);
                }
            }
        }
    }
}
