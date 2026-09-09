using System;
using System.Collections.Generic;

namespace BalartroLike.Battle
{
    public static class HexagramCatalog
    {
        private static readonly Dictionary<TrigramId, Dictionary<TrigramId, HexagramDefinition>> Definitions;

        static HexagramCatalog()
        {
            Definitions = new Dictionary<TrigramId, Dictionary<TrigramId, HexagramDefinition>>();
        }

        public static bool IsLoaded
        {
            get { return Count > 0; }
        }

        public static void Load(IEnumerable<HexagramDefinition> definitions)
        {
            Definitions.Clear();
            foreach (HexagramDefinition definition in definitions)
            {
                if (!Definitions.TryGetValue(definition.Outer, out Dictionary<TrigramId, HexagramDefinition> row))
                {
                    row = new Dictionary<TrigramId, HexagramDefinition>();
                    Definitions.Add(definition.Outer, row);
                }

                row.Add(definition.Inner, definition);
            }
        }

        public static HexagramDefinition Get(TrigramId outer, TrigramId inner)
        {
            if (Definitions.TryGetValue(outer, out Dictionary<TrigramId, HexagramDefinition> row)
                && row.TryGetValue(inner, out HexagramDefinition definition))
            {
                return definition;
            }

            throw new ArgumentException("Unknown hexagram: outer=" + outer + ", inner=" + inner);
        }

        public static int Count
        {
            get
            {
                int count = 0;
                foreach (Dictionary<TrigramId, HexagramDefinition> row in Definitions.Values)
                {
                    count += row.Count;
                }

                return count;
            }
        }
    }
}