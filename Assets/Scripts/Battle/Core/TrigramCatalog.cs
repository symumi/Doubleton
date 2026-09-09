using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public static class TrigramCatalog
    {
        private static readonly Dictionary<TrigramId, TrigramDefinition> Definitions;

        static TrigramCatalog()
        {
            Definitions = new Dictionary<TrigramId, TrigramDefinition>();
        }

        public static bool IsLoaded
        {
            get { return Definitions.Count > 0; }
        }

        public static IEnumerable<TrigramDefinition> All
        {
            get { return Definitions.Values; }
        }

        public static void Load(IEnumerable<TrigramDefinition> definitions)
        {
            Definitions.Clear();
            foreach (TrigramDefinition definition in definitions)
            {
                Definitions.Add(definition.Id, definition);
            }
        }

        public static TrigramDefinition Get(TrigramId id)
        {
            if (Definitions.TryGetValue(id, out TrigramDefinition definition))
            {
                return definition;
            }

            throw new ArgumentException("Unknown trigram: " + id);
        }
    }
}